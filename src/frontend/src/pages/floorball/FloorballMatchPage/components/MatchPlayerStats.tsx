import { useMemo } from 'react';
import { MatchPlayerStatsTable } from '../../../../components/match';
import type { FloorballMatchDto, FloorballTeamPlayer } from '../../../../types/floorball/floorballTypes';
import { buildFloorballPlayerMatchStats, type FloorballPlayerMatchStats } from './floorballPlayerMatchStats';

interface MatchPlayerStatsProps {
  match: FloorballMatchDto;
  homeRoster: FloorballTeamPlayer[];
  awayRoster: FloorballTeamPlayer[];
}

export default function MatchPlayerStats({ match, homeRoster, awayRoster }: MatchPlayerStatsProps) {
  const stats: FloorballPlayerMatchStats = useMemo(
    () => buildFloorballPlayerMatchStats(match, homeRoster, awayRoster),
    [match, homeRoster, awayRoster]
  );

  return (
    <>
      {stats.home.length > 0 && (
        <MatchPlayerStatsTable teamName={match.homeTeamName ?? ''} side="home" sport="floorball" rows={stats.home} />
      )}
      {stats.away.length > 0 && (
        <MatchPlayerStatsTable teamName={match.awayTeamName ?? ''} side="away" sport="floorball" rows={stats.away} />
      )}
    </>
  );
}
