export interface SkaterSeasonInput {
  competitionId: string;
  seasonLabel: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
}

export interface GoalieSeasonInput {
  competitionId: string;
  seasonLabel: string;
  saves: number;
  shotsAgainst: number;
  goalsAgainst: number;
  minutesPlayed: number;
  gamesPlayed: number;
  goalsAgainstAverage: number;
}

export interface SkaterSeasonPoint {
  seasonLabel: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
  pointsPerGame: number;
}

export interface GoalieSeasonPoint {
  seasonLabel: string;
  savePercentage: number;
  goalsAgainstAverage: number;
}

interface SeasonIdentity {
  competitionId: string;
  seasonLabel: string;
}

function seasonKey(row: SeasonIdentity): string {
  const competitionId = row.competitionId.trim();
  return competitionId.length > 0 ? competitionId : row.seasonLabel;
}

function compareLabels(left: string, right: string): number {
  return left.localeCompare(right, 'fi', { numeric: true, sensitivity: 'base' });
}

export function countDistinctCompetitions(rows: SeasonIdentity[]): number {
  return new Set(rows.map(seasonKey)).size;
}

export function aggregateSkaterSeasons(rows: SkaterSeasonInput[]): SkaterSeasonPoint[] {
  const grouped = new Map<string, SkaterSeasonInput>();

  for (const row of rows) {
    const key = seasonKey(row);
    const current = grouped.get(key);
    if (!current) {
      grouped.set(key, { ...row });
      continue;
    }
    current.gamesPlayed += row.gamesPlayed;
    current.goals += row.goals;
    current.assists += row.assists;
    current.points += row.points;
  }

  return [...grouped.values()]
    .sort((left, right) => compareLabels(left.seasonLabel, right.seasonLabel))
    .map((row) => {
      const points = row.points > 0 ? row.points : row.goals + row.assists;
      return {
        seasonLabel: row.seasonLabel,
        gamesPlayed: row.gamesPlayed,
        goals: row.goals,
        assists: row.assists,
        pointsPerGame: row.gamesPlayed > 0 ? points / row.gamesPlayed : 0,
      };
    });
}

export function aggregateGoalieSeasons(rows: GoalieSeasonInput[]): GoalieSeasonPoint[] {
  const grouped = new Map<string, GoalieSeasonInput>();

  for (const row of rows) {
    const key = seasonKey(row);
    const current = grouped.get(key);
    if (!current) {
      grouped.set(key, { ...row });
      continue;
    }
    const games = current.gamesPlayed + row.gamesPlayed;
    const weightedAverage = games > 0
      ? (current.goalsAgainstAverage * current.gamesPlayed + row.goalsAgainstAverage * row.gamesPlayed) / games
      : 0;
    current.saves += row.saves;
    current.shotsAgainst += row.shotsAgainst;
    current.goalsAgainst += row.goalsAgainst;
    current.minutesPlayed += row.minutesPlayed;
    current.gamesPlayed = games;
    current.goalsAgainstAverage = weightedAverage;
  }

  return [...grouped.values()]
    .sort((left, right) => compareLabels(left.seasonLabel, right.seasonLabel))
    .map((row) => ({
      seasonLabel: row.seasonLabel,
      savePercentage: row.shotsAgainst > 0 ? (row.saves / row.shotsAgainst) * 100 : 0,
      goalsAgainstAverage: row.minutesPlayed > 0
        ? (row.goalsAgainst * 60) / row.minutesPlayed
        : row.goalsAgainstAverage,
    }));
}
