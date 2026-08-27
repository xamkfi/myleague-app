export type RelatedNewsTeam = {
  name: string;
  logoUrl?: string;
};

type MatchResultPayload = {
  matches?: Array<{
    homeTeam?: string;
    awayTeam?: string;
    homeTeamImage?: string;
    awayTeamImage?: string;
  }>;
};

function addTeam(teams: Map<string, RelatedNewsTeam>, name: string | undefined, logoUrl: string | undefined): void {
  const trimmedName = name?.trim();
  if (!trimmedName) {
    return;
  }

  const existing = teams.get(trimmedName);
  const trimmedLogo = logoUrl?.trim();
  if (!existing) {
    teams.set(trimmedName, { name: trimmedName, logoUrl: trimmedLogo || undefined });
    return;
  }

  if (!existing.logoUrl && trimmedLogo) {
    teams.set(trimmedName, { name: trimmedName, logoUrl: trimmedLogo });
  }
}

export function extractRelatedNewsTeams(contentHtml: string | undefined): RelatedNewsTeam[] {
  if (!contentHtml) {
    return [];
  }

  const parser = new DOMParser();
  const documentNode = parser.parseFromString(contentHtml, 'text/html');
  const scripts = documentNode.querySelectorAll('script.match-result-data');
  const teams = new Map<string, RelatedNewsTeam>();

  scripts.forEach((script) => {
    if (!script.textContent) {
      return;
    }

    try {
      const parsed = JSON.parse(script.textContent) as MatchResultPayload;
      for (const match of parsed.matches ?? []) {
        addTeam(teams, match.homeTeam, match.homeTeamImage);
        addTeam(teams, match.awayTeam, match.awayTeamImage);
      }
    } catch {
      // Ignore malformed match payloads embedded in older articles.
    }
  });

  return [...teams.values()];
}
