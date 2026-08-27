import { Quill } from 'react-quill';
import {
  renderMatchResultListHtml,
  type MatchResultBlotValue,
  type MatchResultValue,
} from './matchResultRender';

export type { MatchResultBlotValue, MatchResultValue };

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const BlockEmbed = Quill.import('blots/block/embed') as any;

export class MatchResultTableBlot extends BlockEmbed {
  static blotName = 'matchResultTable';
  static tagName = 'div';
  static className = 'match-result-table-container';

  static create(value: MatchResultBlotValue): HTMLElement {
    const node = super.create();
    const matches = value?.matches ?? [];
    const rows = renderMatchResultListHtml(matches);
    node.innerHTML =
      `<div class="match-result-list">${rows}</div>` +
      `<script type="application/json" class="match-result-data" style="display: none;">${JSON.stringify({ matches })}</script>`;
    node.setAttribute('contenteditable', 'false');
    return node;
  }

  static value(node: HTMLElement): MatchResultBlotValue {
    const dataElement = node.querySelector('.match-result-data');
    if (dataElement?.textContent) {
      try {
        return JSON.parse(dataElement.textContent);
      } catch {
        // Fall through to default
      }
    }
    return { matches: [], title: '' };
  }
}

let blotRegistered = false;

/**
 * Registers the MatchResultTableBlot exactly once per page load. Safe to call
 * from multiple modules — Quill warns on duplicate registration which would
 * otherwise spam the console when both the news and tournament editors mount.
 */
export const ensureMatchResultBlotRegistered = (): void => {
  if (blotRegistered) return;
  Quill.register(MatchResultTableBlot);
  blotRegistered = true;
};
