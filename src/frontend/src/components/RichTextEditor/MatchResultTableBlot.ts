import { Quill } from 'react-quill';

export interface MatchResultValue {
  homeTeam: string;
  awayTeam: string;
  /** Score can come from the API as a number, or persisted as a string in the
   *  serialized blot data. The blot only ever reads it through `Number()` /
   *  template interpolation, so both representations are safe. */
  homeScore: string | number;
  awayScore: string | number;
  date: string;
  link: string;
  status?: string;
  homeTeamImage?: string;
  awayTeamImage?: string;
}

export interface MatchResultBlotValue {
  matches: MatchResultValue[];
  title?: string;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const BlockEmbed = Quill.import('blots/block/embed') as any;

const STATUS_LABELS: Record<string, string> = {
  completed: 'PÄÄTTYNYT',
  in_progress: 'KÄYNNISSÄ',
  cancelled: 'PERUTTU',
  postponed: 'SIIRRETTY',
};

const formatDate = (iso: string): string =>
  new Date(iso).toLocaleDateString('fi-FI', { day: 'numeric', month: 'numeric' });

const formatTime = (iso: string): string =>
  new Date(iso).toLocaleTimeString('fi-FI', { hour: '2-digit', minute: '2-digit' });

const renderTeamLogo = (src: string | undefined, alt: string): string =>
  src
    ? `<img src="${src}" alt="${alt}" class="mr-team-logo" />`
    : '<span class="mr-team-logo-placeholder"></span>';

const renderMatchRow = (match: MatchResultValue): string => {
  const isCompleted = match.status?.toLowerCase() === 'completed';
  const isLive = match.status?.toLowerCase() === 'in_progress';
  const homeScore = isCompleted || isLive ? match.homeScore : '-';
  const awayScore = isCompleted || isLive ? match.awayScore : '-';
  const homeWon = isCompleted && Number(match.homeScore) > Number(match.awayScore);
  const awayWon = isCompleted && Number(match.awayScore) > Number(match.homeScore);

  const statusKey = match.status?.toLowerCase() ?? '';
  const statusLabel = STATUS_LABELS[statusKey] ?? '';
  const statusHtml = statusLabel
    ? `<span class="mr-status"><span class="mr-status-badge mr-status-badge--${statusKey}">${statusLabel}</span></span>`
    : '';

  return (
    `<a href="/match/${match.link}" class="match-result-row" target="_blank" rel="noopener noreferrer">` +
    `<span class="mr-date"><span class="mr-date-day">${formatDate(match.date)}</span><span class="mr-date-time">${formatTime(match.date)}</span></span>` +
    `<span class="mr-teams">` +
    `<span class="mr-team-line${homeWon ? ' mr-winner' : ''}">${renderTeamLogo(match.homeTeamImage, match.homeTeam)}<span class="mr-team-name">${match.homeTeam}</span></span>` +
    `<span class="mr-team-line${awayWon ? ' mr-winner' : ''}">${renderTeamLogo(match.awayTeamImage, match.awayTeam)}<span class="mr-team-name">${match.awayTeam}</span></span>` +
    `</span>` +
    `<span class="mr-scores"><span class="mr-score${homeWon ? ' mr-score--winner' : ''}">${homeScore}</span><span class="mr-score${awayWon ? ' mr-score--winner' : ''}">${awayScore}</span></span>` +
    statusHtml +
    `</a>`
  );
};

export class MatchResultTableBlot extends BlockEmbed {
  static blotName = 'matchResultTable';
  static tagName = 'div';
  static className = 'match-result-table-container';

  static create(value: MatchResultBlotValue): HTMLElement {
    const node = super.create();
    const matches = value?.matches ?? [];
    const rows = matches.map(renderMatchRow).join('');
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
