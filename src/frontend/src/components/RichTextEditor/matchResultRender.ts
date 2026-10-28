import i18n from '../../i18n/i18n';
import { resolveLogoUrl } from '../../utils/resolveLogoUrl';
import { getMatchPath, type SportKind } from '../../utils/sportRoutes';

export interface MatchResultValue {
  homeTeam: string;
  awayTeam: string;
  homeScore: string | number;
  awayScore: string | number;
  date: string;
  link: string;
  /** Missing on embeds saved before football and hockey were supported; those are floorball. */
  sport?: SportKind;
  status?: string;
  homeTeamImage?: string;
  awayTeamImage?: string;
}

export interface MatchResultBlotValue {
  matches: MatchResultValue[];
  title?: string;
}

const KNOWN_STATUSES: ReadonlySet<string> = new Set([
  'scheduled',
  'in_progress',
  'completed',
  'cancelled',
  'postponed',
]);

/**
 * Status badge text in the reader's language. Stored embeds are rendered again on display
 * (see `hydrateMatchResultHtml`), so the badge follows the reader, not the author.
 */
function statusLabel(statusKey: string): string {
  return KNOWN_STATUSES.has(statusKey) ? i18n.t(`newsPage.matchStatus.${statusKey}`) : '';
}

function displayLocale(): string {
  return i18n.language || 'fi';
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

export function usableTeamLogo(url: string | null | undefined): string | undefined {
  return resolveLogoUrl(url);
}

const formatDate = (iso: string): string =>
  new Date(iso).toLocaleDateString(displayLocale(), { weekday: 'short', day: 'numeric', month: 'numeric' });

const formatTime = (iso: string): string =>
  new Date(iso).toLocaleTimeString(displayLocale(), { hour: '2-digit', minute: '2-digit' });

const logoPlaceholderHtml = '<span class="mr-team-logo-placeholder" aria-hidden="true"></span>';

function createLogoPlaceholder(documentNode: Document): HTMLElement {
  const placeholder = documentNode.createElement('span');
  placeholder.className = 'mr-team-logo-placeholder';
  placeholder.setAttribute('aria-hidden', 'true');
  return placeholder;
}

/** Replaces logos that fail to load with the same grey circle used when no URL exists. */
export function bindTeamLogoFallbacks(root: ParentNode): void {
  root.querySelectorAll<HTMLImageElement>('img.mr-team-logo').forEach((img) => {
    const replaceWithPlaceholder = (): void => {
      if (!img.isConnected) {
        return;
      }
      img.replaceWith(createLogoPlaceholder(img.ownerDocument));
    };

    img.addEventListener('error', replaceWithPlaceholder, { once: true });
    if (img.complete && img.naturalWidth === 0) {
      replaceWithPlaceholder();
    }
  });
}

const renderTeamLogo = (src: string | undefined): string => {
  const logo = usableTeamLogo(src);
  if (!logo) {
    return logoPlaceholderHtml;
  }

  return `<img src="${escapeHtml(logo)}" alt="" class="mr-team-logo" />`;
};

/**
 * One match as a compact scoreboard: a small line with the status and the date on top,
 * then home team, result and away team on one line. A scheduled match shows its start
 * time where the result would be.
 */
export const renderMatchRow = (match: MatchResultValue): string => {
  const statusKey = match.status?.toLowerCase() ?? '';
  const isCompleted = statusKey === 'completed';
  const isLive = statusKey === 'in_progress';
  const isScheduled = statusKey === 'scheduled' || statusKey === '';
  const hasScore = isCompleted || isLive;
  const homeWon = isCompleted && Number(match.homeScore) > Number(match.awayScore);
  const awayWon = isCompleted && Number(match.awayScore) > Number(match.homeScore);
  const decided = homeWon || awayWon;

  const label = statusLabel(statusKey || 'scheduled');
  const statusHtml = label
    ? `<span class="mr-status-badge mr-status-badge--${escapeHtml(statusKey || 'scheduled')}">${escapeHtml(label)}</span>`
    : '';
  const whenText = isScheduled ? formatDate(match.date) : `${formatDate(match.date)} ${formatTime(match.date)}`;

  let resultHtml: string;
  if (hasScore) {
    resultHtml = `<span class="mr-result${isLive ? ' mr-result--live' : ''}">${escapeHtml(String(match.homeScore))}<span class="mr-result-sep">–</span>${escapeHtml(String(match.awayScore))}</span>`;
  } else if (isScheduled) {
    resultHtml = `<span class="mr-result mr-result--time">${escapeHtml(formatTime(match.date))}</span>`;
  } else {
    resultHtml = '<span class="mr-result mr-result--none">–</span>';
  }

  const teamClass = (won: boolean): string =>
    `mr-team${won ? ' mr-team--winner' : decided ? ' mr-team--loser' : ''}`;

  return (
    `<a href="${escapeHtml(getMatchPath(match.sport ?? 'floorball', match.link))}" class="match-result-row mr-row mr-row--${escapeHtml(statusKey || 'scheduled')}" target="_blank" rel="noopener noreferrer">` +
    `<span class="mr-meta">${statusHtml}<span class="mr-when">${escapeHtml(whenText)}</span></span>` +
    `<span class="mr-board">` +
    `<span class="${teamClass(homeWon)} mr-team--home"><span class="mr-team-name">${escapeHtml(match.homeTeam)}</span>${renderTeamLogo(match.homeTeamImage)}</span>` +
    resultHtml +
    `<span class="${teamClass(awayWon)} mr-team--away">${renderTeamLogo(match.awayTeamImage)}<span class="mr-team-name">${escapeHtml(match.awayTeam)}</span></span>` +
    `</span>` +
    `</a>`
  );
};

export const renderMatchResultListHtml = (matches: MatchResultValue[]): string =>
  matches.map(renderMatchRow).join('');
