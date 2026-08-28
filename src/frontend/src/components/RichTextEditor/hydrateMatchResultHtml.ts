import { floorballMatchService } from '../../api/floorball/floorballMatchService';
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

async function hydrateMatchLogos(matches: MatchResultValue[]): Promise<MatchResultValue[]> {
  const idsToFetch = [...new Set(
    matches
      .filter((match) => match.link && (!usableTeamLogo(match.homeTeamImage) || !usableTeamLogo(match.awayTeamImage)))
      .map((match) => match.link)
  )];

  if (idsToFetch.length === 0) {
    return matches.map((match) => ({
      ...match,
      homeTeamImage: usableTeamLogo(match.homeTeamImage),
      awayTeamImage: usableTeamLogo(match.awayTeamImage),
    }));
  }

  const logosByMatchId = new Map<string, { home?: string; away?: string }>();
  await Promise.all(idsToFetch.map(async (id) => {
    try {
      const response = await floorballMatchService.getById(id);
      const match = response.data;
      if (!match) {
        return;
      }
      logosByMatchId.set(id, {
        home: usableTeamLogo(match.homeTeamLogo),
        away: usableTeamLogo(match.awayTeamLogo),
      });
    } catch {
      // Keep the stored snapshot if the live match cannot be loaded.
    }
  }));

  return matches.map((match) => {
    const live = logosByMatchId.get(match.link);
    return {
      ...match,
      homeTeamImage: usableTeamLogo(match.homeTeamImage) ?? live?.home,
      awayTeamImage: usableTeamLogo(match.awayTeamImage) ?? live?.away,
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
