export interface MatchResultValue {
  homeTeam: string;
  awayTeam: string;
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

const STATUS_LABELS: Record<string, string> = {
  completed: 'PÄÄTTYNYT',
  in_progress: 'KÄYNNISSÄ',
  cancelled: 'PERUTTU',
  postponed: 'SIIRRETTY',
};

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

export function usableTeamLogo(url: string | null | undefined): string | undefined {
  const trimmed = url?.trim();
  if (!trimmed) {
    return undefined;
  }

  try {
    const hostname = new URL(trimmed, 'https://myleague.invalid').hostname.toLowerCase();
    if (hostname === 'example.com' || hostname.endsWith('.example.com')) {
      return undefined;
    }
  } catch {
    return undefined;
  }

  return trimmed;
}

const formatDate = (iso: string): string =>
  new Date(iso).toLocaleDateString('fi-FI', { day: 'numeric', month: 'numeric' });

const formatTime = (iso: string): string =>
  new Date(iso).toLocaleTimeString('fi-FI', { hour: '2-digit', minute: '2-digit' });

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

export const renderMatchRow = (match: MatchResultValue): string => {
  const isCompleted = match.status?.toLowerCase() === 'completed';
  const isLive = match.status?.toLowerCase() === 'in_progress';
  const homeScore = isCompleted || isLive ? match.homeScore : '-';
  const awayScore = isCompleted || isLive ? match.awayScore : '-';
  const homeWon = isCompleted && Number(match.homeScore) > Number(match.awayScore);
  const awayWon = isCompleted && Number(match.awayScore) > Number(match.homeScore);

  const homeScoreHtml = escapeHtml(String(homeScore));
  const awayScoreHtml = escapeHtml(String(awayScore));
  const statusKey = match.status?.toLowerCase() ?? '';
  const statusLabel = STATUS_LABELS[statusKey] ?? '';
  const statusHtml = statusLabel
    ? `<span class="mr-status"><span class="mr-status-badge mr-status-badge--${escapeHtml(statusKey)}">${statusLabel}</span></span>`
    : '';

  return (
    `<a href="/match/${escapeHtml(match.link)}" class="match-result-row" target="_blank" rel="noopener noreferrer">` +
    `<span class="mr-date"><span class="mr-date-day">${escapeHtml(formatDate(match.date))}</span><span class="mr-date-time">${escapeHtml(formatTime(match.date))}</span></span>` +
    `<span class="mr-teams">` +
    `<span class="mr-team-line${homeWon ? ' mr-winner' : ''}">${renderTeamLogo(match.homeTeamImage)}<span class="mr-team-name">${escapeHtml(match.homeTeam)}</span></span>` +
    `<span class="mr-team-line${awayWon ? ' mr-winner' : ''}">${renderTeamLogo(match.awayTeamImage)}<span class="mr-team-name">${escapeHtml(match.awayTeam)}</span></span>` +
    `</span>` +
    `<span class="mr-scores"><span class="mr-score${homeWon ? ' mr-score--winner' : ''}">${homeScoreHtml}</span><span class="mr-score${awayWon ? ' mr-score--winner' : ''}">${awayScoreHtml}</span></span>` +
    statusHtml +
    `</a>`
  );
};

export const renderMatchResultListHtml = (matches: MatchResultValue[]): string =>
  matches.map(renderMatchRow).join('');
