import MatchRow from '../MatchRow';
import { FloorballMatchStatus } from '../../types/floorball/floorballTypes';
import {
  isHockeyMatchFinished,
  isHockeyMatchLive,
  type HockeyMatchDto,
} from '../../types/hockey/hockeyTypes';

interface HockeyMatchRowProps {
  match: HockeyMatchDto;
  teamNames: Map<string, string>;
  teamLogos?: Map<string, string | null>;
}

function toDisplayStatus(status: string): FloorballMatchStatus {
  if (isHockeyMatchLive(status)) {
    return FloorballMatchStatus.InProgress;
  }
  if (isHockeyMatchFinished(status)) {
    return FloorballMatchStatus.Completed;
  }
  return FloorballMatchStatus.Scheduled;
}

function periodScoreMap(match: HockeyMatchDto): Record<number, { homeScore: number; awayScore: number }> {
  const scores: Record<number, { homeScore: number; awayScore: number }> = {};
  for (const period of match.periodScores) {
    scores[period.periodNumber] = { homeScore: period.homeGoals, awayScore: period.awayGoals };
  }
  return scores;
}

function logoFor(
  teamId: string | null | undefined,
  teamLogos: Map<string, string | null> | undefined,
): string | undefined {
  if (!teamId || !teamLogos) {
    return undefined;
  }
  return teamLogos.get(teamId) ?? undefined;
}

function HockeyMatchRow({ match, teamNames, teamLogos }: HockeyMatchRowProps) {
  return (
    <MatchRow
      id={match.id}
      href={`/hockey/match/${match.id}`}
      scheduledDateTime={match.scheduledStartTime}
      homeTeamName={match.homeTeamId ? teamNames.get(match.homeTeamId) ?? 'TBD' : 'TBD'}
      awayTeamName={match.awayTeamId ? teamNames.get(match.awayTeamId) ?? 'TBD' : 'TBD'}
      homeTeamLogo={logoFor(match.homeTeamId, teamLogos)}
      awayTeamLogo={logoFor(match.awayTeamId, teamLogos)}
      homeScore={match.homeScore}
      awayScore={match.awayScore}
      periodScores={periodScoreMap(match)}
      status={toDisplayStatus(match.status)}
    />
  );
}

export default HockeyMatchRow;
