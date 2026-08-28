import { Quill } from 'react-quill';
import { replaceChildrenWithSanitizedHtml } from './parseSanitizedHtml';
import {
  bindTeamLogoFallbacks,
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
    const node = super.create() as HTMLElement;
    const matches = value?.matches ?? [];
    const list = node.ownerDocument.createElement('div');
    list.className = 'match-result-list';
    replaceChildrenWithSanitizedHtml(list, renderMatchResultListHtml(matches));

    const payload = node.ownerDocument.createElement('script');
    payload.type = 'application/json';
    payload.className = 'match-result-data';
    payload.setAttribute('style', 'display: none;');
    payload.textContent = JSON.stringify({ matches });

    node.append(list, payload);
    node.setAttribute('contenteditable', 'false');
    bindTeamLogoFallbacks(node);
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
