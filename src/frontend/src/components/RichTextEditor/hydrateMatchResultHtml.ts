import { newsMatchService, type NewsMatchLogos } from '../../api/news/newsMatchService';
import { parseSanitizedHtmlRoot, replaceChildrenWithSanitizedHtml } from './parseSanitizedHtml';
import {
  renderMatchResultListHtml,
  usableTeamLogo,
  type MatchResultBlotValue,
  type MatchResultValue,
} from './matchResultRender';

function parseBlotValue(raw: string | null | undefined): MatchResultBlotValue | null {
  if (!raw?.trim()) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as MatchResultBlotValue;
    if (!parsed || !Array.isArray(parsed.matches)) {
      return null;
    }
    return parsed;
  } catch {
    return null;
  }
}

function logoCacheKey(match: MatchResultValue): string {
  return `${match.sport ?? 'floorball'}:${match.link}`;
}

async function hydrateMatchLogos(matches: MatchResultValue[]): Promise<MatchResultValue[]> {
  const toFetch = new Map<string, MatchResultValue>();
  for (const match of matches) {
    if (match.link && (!usableTeamLogo(match.homeTeamImage) || !usableTeamLogo(match.awayTeamImage))) {
      toFetch.set(logoCacheKey(match), match);
    }
  }

  if (toFetch.size === 0) {
    return matches.map((match) => ({
      ...match,
      homeTeamImage: usableTeamLogo(match.homeTeamImage),
      awayTeamImage: usableTeamLogo(match.awayTeamImage),
    }));
  }

  const logosByKey = new Map<string, NewsMatchLogos>();
  await Promise.all([...toFetch].map(async ([key, match]) => {
    try {
      const logos = await newsMatchService.getMatchLogos(match.sport ?? 'floorball', match.link);
      if (logos) {
        logosByKey.set(key, logos);
      }
    } catch {
      // Keep the stored snapshot if the live match cannot be loaded.
    }
  }));

  return matches.map((match) => {
    const live = logosByKey.get(logoCacheKey(match));
    return {
      ...match,
      homeTeamImage: usableTeamLogo(match.homeTeamImage) ?? usableTeamLogo(live?.home),
      awayTeamImage: usableTeamLogo(match.awayTeamImage) ?? usableTeamLogo(live?.away),
    };
  });
}

function replaceMatchList(container: Element, matches: MatchResultValue[]): void {
  const rowsHtml = renderMatchResultListHtml(matches);
  const existingList = container.querySelector('.match-result-list');
  if (existingList) {
    replaceChildrenWithSanitizedHtml(existingList, rowsHtml);
    return;
  }

  container.querySelectorAll('.match-result-row').forEach((row) => row.remove());
  const list = container.ownerDocument.createElement('div');
  list.className = 'match-result-list';
  replaceChildrenWithSanitizedHtml(list, rowsHtml);
  const script = container.querySelector('.match-result-data');
  if (script) {
    container.insertBefore(list, script);
  } else {
    container.appendChild(list);
  }
}

export async function hydrateMatchResultHtml(contentHtml: string): Promise<string> {
  if (!contentHtml.includes('match-result')) {
    return contentHtml;
  }

  const root = parseSanitizedHtmlRoot(contentHtml);
  const containers = root.querySelectorAll('.match-result-table-container');
  if (containers.length === 0) {
    return contentHtml;
  }

  for (const container of Array.from(containers)) {
    const script = container.querySelector('.match-result-data');
    const parsed = parseBlotValue(script?.textContent);
    if (!parsed) {
      continue;
    }

    const hydratedMatches = await hydrateMatchLogos(parsed.matches);
    parsed.matches = hydratedMatches;
    if (script) {
      script.textContent = JSON.stringify(parsed);
    }
    replaceMatchList(container, hydratedMatches);
  }

  return root.innerHTML;
}
