import ReactQuill, { Quill } from 'react-quill';
import { MatchResultTableBlot } from './MatchResultTableBlot';
import { parseSanitizedHtmlRoot } from './parseSanitizedHtml';
import type { MatchResultValue } from './matchResultRender';

type QuillEditor = ReturnType<InstanceType<typeof ReactQuill>['getEditor']>;

const MATCH_BOX_SELECTOR = '.match-result-table-container';
const DRAG_TYPE = 'application/x-myleague-match-result';

function matchBoxes(quill: QuillEditor): HTMLElement[] {
  return Array.from(quill.root.querySelectorAll<HTMLElement>(MATCH_BOX_SELECTOR));
}

function indexOfBox(quill: QuillEditor, node: HTMLElement): number | null {
  const blot = Quill.find(node);
  if (!blot) {
    return null;
  }
  return quill.getIndex(blot);
}

function firstOp(quill: QuillEditor, index: number): { insert?: unknown } | undefined {
  return quill.getContents(index, 1).ops?.[0];
}

function isEmbedInsert(insert: unknown): boolean {
  return typeof insert === 'object' && insert !== null;
}

function ensureNewlineAfter(quill: QuillEditor, embedIndex: number): void {
  const nextIndex = embedIndex + 1;
  if (nextIndex >= quill.getLength()) {
    quill.insertText(nextIndex, '\n', 'silent');
    return;
  }
  const insert = firstOp(quill, nextIndex)?.insert;
  if (isEmbedInsert(insert) || typeof insert !== 'string' || !insert.startsWith('\n')) {
    quill.insertText(nextIndex, '\n', 'silent');
  }
}

export function insertMatchBoxes(quill: QuillEditor, matches: MatchResultValue[]): void {
  if (matches.length === 0) {
    return;
  }

  const selection = quill.getSelection(true);
  let insertAt = selection?.index ?? Math.max(0, quill.getLength() - 1);

  matches.forEach((match) => {
    quill.insertEmbed(insertAt, 'matchResultTable', { matches: [match] }, 'user');
    insertAt += 1;
    quill.insertText(insertAt, '\n', 'user');
    insertAt += 1;
  });

  quill.setSelection(insertAt, 0);
}

export function hasCombinedMatchBox(html: string): boolean {
  if (!html.includes('match-result-data')) {
    return false;
  }

  const root = parseSanitizedHtmlRoot(html);
  return Array.from(root.querySelectorAll('.match-result-data')).some((element) => {
    try {
      const parsed = JSON.parse(element.textContent ?? '') as { matches?: MatchResultValue[] };
      return Array.isArray(parsed.matches) && parsed.matches.length > 1;
    } catch {
      return false;
    }
  });
}

export function splitCombinedMatchBlots(quill: QuillEditor): boolean {
  const boxes = matchBoxes(quill);
  let changed = false;

  for (let index = boxes.length - 1; index >= 0; index -= 1) {
    const node = boxes[index];
    const matches = MatchResultTableBlot.value(node).matches ?? [];
    if (matches.length <= 1) {
      continue;
    }

    const blotIndex = indexOfBox(quill, node);
    if (blotIndex === null) {
      continue;
    }

    quill.deleteText(blotIndex, 1, 'silent');
    let insertAt = blotIndex;
    matches.forEach((match) => {
      quill.insertEmbed(insertAt, 'matchResultTable', { matches: [match] }, 'silent');
      insertAt += 1;
      quill.insertText(insertAt, '\n', 'silent');
      insertAt += 1;
    });
    changed = true;
  }

  return changed;
}

function dropIndexFromPoint(quill: QuillEditor, x: number, y: number): number {
  const box = document.elementFromPoint(x, y)?.closest<HTMLElement>(MATCH_BOX_SELECTOR);
  if (box && quill.root.contains(box) && !box.classList.contains('is-dragging')) {
    const blotIndex = indexOfBox(quill, box);
    if (blotIndex !== null) {
      const rect = box.getBoundingClientRect();
      return y < rect.top + rect.height / 2 ? blotIndex : blotIndex + 1;
    }
  }

  const documentWithCaret = document as Document & {
    caretPositionFromPoint?: (x: number, y: number) => { offsetNode: Node; offset: number } | null;
  };
  const caret = documentWithCaret.caretPositionFromPoint?.(x, y);
  const range = !caret ? document.caretRangeFromPoint?.(x, y) : null;
  const node = caret?.offsetNode ?? range?.startContainer;
  const offset = caret?.offset ?? range?.startOffset ?? 0;
  if (!node) {
    return Math.max(0, quill.getLength() - 1);
  }

  try {
    const blot = Quill.find(node, true);
    if (!blot) {
      return Math.max(0, quill.getLength() - 1);
    }
    return quill.getIndex(blot) + offset;
  } catch {
    return Math.max(0, quill.getLength() - 1);
  }
}

function moveMatchBox(quill: QuillEditor, sourceNode: HTMLElement, dropIndex: number): void {
  const from = indexOfBox(quill, sourceNode);
  if (from === null) {
    return;
  }

  let to = Math.max(0, Math.min(dropIndex, quill.getLength() - 1));
  if (to === from || to === from + 1) {
    return;
  }

  const value = MatchResultTableBlot.value(sourceNode);
  quill.deleteText(from, 1, 'silent');
  if (from < to) {
    to -= 1;
  }
  quill.insertEmbed(to, 'matchResultTable', value, 'user');
  ensureNewlineAfter(quill, to);
}

export function enableMatchResultDrag(
  quill: QuillEditor,
  dragLabel: string
): () => void {
  const root = quill.root;
  let dragging: HTMLElement | null = null;

  const applyLabels = (): void => {
    matchBoxes(quill).forEach((box) => {
      box.setAttribute('title', dragLabel);
    });
  };

  const onPointerDown = (event: PointerEvent): void => {
    const box = (event.target as Element | null)?.closest<HTMLElement>(MATCH_BOX_SELECTOR);
    if (!box || !root.contains(box)) {
      return;
    }
    box.setAttribute('draggable', 'true');
  };

  const onDragStart = (event: DragEvent): void => {
    const box = (event.target as Element | null)?.closest<HTMLElement>(MATCH_BOX_SELECTOR);
    if (!box || !root.contains(box) || !event.dataTransfer) {
      return;
    }
    dragging = box;
    box.classList.add('is-dragging');
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('text/plain', DRAG_TYPE);
    event.dataTransfer.setData(DRAG_TYPE, DRAG_TYPE);
  };

  const onDragOver = (event: DragEvent): void => {
    if (!dragging) {
      return;
    }
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
    root.classList.add('has-match-drop');
  };

  const onDrop = (event: DragEvent): void => {
    if (!dragging) {
      return;
    }
    event.preventDefault();
    const index = dropIndexFromPoint(quill, event.clientX, event.clientY);
    moveMatchBox(quill, dragging, index);
  };

  const onDragEnd = (): void => {
    dragging?.classList.remove('is-dragging');
    dragging?.removeAttribute('draggable');
    dragging = null;
    root.classList.remove('has-match-drop');
    applyLabels();
  };

  applyLabels();
  root.addEventListener('pointerdown', onPointerDown);
  root.addEventListener('dragstart', onDragStart);
  root.addEventListener('dragover', onDragOver);
  root.addEventListener('drop', onDrop);
  root.addEventListener('dragend', onDragEnd);

  return () => {
    root.removeEventListener('pointerdown', onPointerDown);
    root.removeEventListener('dragstart', onDragStart);
    root.removeEventListener('dragover', onDragOver);
    root.removeEventListener('drop', onDrop);
    root.removeEventListener('dragend', onDragEnd);
    root.classList.remove('has-match-drop');
  };
}
