export interface HockeyDivisionGroupSource {
  teams: Array<{ id: string; teamId: string; isActive: boolean }>;
  divisions: Array<{
    id: string;
    divisionId: string;
    name: string;
    sortOrder: number;
    isActive: boolean;
    teams: Array<{ competitionTeamId: string; isActive: boolean }>;
  }>;
}

export interface HockeyDivisionGroup {
  id: string;
  divisionId: string;
  name: string;
  sortOrder: number;
  teamIds: Set<string>;
}

export interface HockeyDivisionStandingsSlice<T> {
  id: string;
  name: string;
  rows: T[];
}

export function activeHockeyDivisionGroups(
  season: HockeyDivisionGroupSource,
  teamDivisionByTeamId?: Map<string, string | null>,
): HockeyDivisionGroup[] {
  const teamIdByCompetitionTeam = new Map(
    season.teams.filter((team) => team.isActive).map((team) => [team.id, team.teamId]),
  );

  const groups: HockeyDivisionGroup[] = season.divisions
    .filter((division) => division.isActive)
    .map((division) => ({
      id: division.id,
      divisionId: division.divisionId,
      name: division.name,
      sortOrder: division.sortOrder,
      teamIds: new Set(
        division.teams
          .filter((member) => member.isActive)
          .map((member) => teamIdByCompetitionTeam.get(member.competitionTeamId))
          .filter((teamId): teamId is string => Boolean(teamId)),
      ),
    }))
    .sort((left, right) => left.sortOrder - right.sortOrder);

  if (teamDivisionByTeamId) {
    const placed = new Set(groups.flatMap((group) => [...group.teamIds]));
    for (const member of season.teams.filter((team) => team.isActive)) {
      if (placed.has(member.teamId)) {
        continue;
      }
      const homeDivisionId = teamDivisionByTeamId.get(member.teamId);
      if (!homeDivisionId) {
        continue;
      }
      const group = groups.find((item) => item.divisionId === homeDivisionId);
      if (!group) {
        continue;
      }
      group.teamIds.add(member.teamId);
      placed.add(member.teamId);
    }
  }

  return groups.filter((group) => group.teamIds.size > 0);
}

export function splitStandingsByDivision<T extends { teamId: string }>(
  standings: T[],
  groups: HockeyDivisionGroup[],
): HockeyDivisionStandingsSlice<T>[] | null {
  if (groups.length < 2) {
    return null;
  }

  return groups.map((group) => ({
    id: group.id,
    name: group.name,
    rows: standings.filter((row) => group.teamIds.has(row.teamId)),
  }));
}
