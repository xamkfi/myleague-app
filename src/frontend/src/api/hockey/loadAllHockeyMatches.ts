import { hockeyMatchService } from './hockeyMatchService';
import { hockeySeasonService } from './hockeySeasonService';
import { hockeyTournamentService } from './hockeyTournamentService';
import type { HockeyMatchDto } from '../../types/hockey/hockeyTypes';

export async function loadAllHockeyMatches(teamCategory?: string): Promise<HockeyMatchDto[]> {
  const [seasons, tournaments] = await Promise.all([
    hockeySeasonService.getAll(teamCategory),
    hockeyTournamentService.getAll(teamCategory),
  ]);
  const competitionIds = [...seasons.map((item) => item.id), ...tournaments.map((item) => item.id)];
  const batches = await Promise.all(
    competitionIds.map(async (competitionId) => {
      try {
        return await hockeyMatchService.getByCompetition(competitionId);
      } catch {
        return [];
      }
    }),
  );
  const seen = new Set<string>();
  const matches: HockeyMatchDto[] = [];
  for (const match of batches.flat()) {
    if (!seen.has(match.id)) {
      seen.add(match.id);
      matches.push(match);
    }
  }
  return matches;
}
