import { useEffect, useState } from 'react';
import { floorballTeamService } from '../../api/floorball/floorballTeamService';
import { footballTeamService } from '../../api/football/footballTeamService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import {
  collectJerseyNumbers,
  toJerseyNumberSport,
  type JerseyNumberSport,
} from './jerseyNumbers';

export function useTakenJerseyNumbers(
  sport: string | undefined | null,
  teamId: string | undefined | null,
): { takenNumbers: number[]; isLoading: boolean } {
  const [takenNumbers, setTakenNumbers] = useState<number[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const resolved = toJerseyNumberSport(sport);
    if (!resolved || !teamId) {
      setTakenNumbers([]);
      setIsLoading(false);
      return;
    }

    let cancelled = false;
    setIsLoading(true);

    void loadTakenJerseyNumbers(resolved, teamId)
      .then((numbers) => {
        if (!cancelled) {
          setTakenNumbers(numbers);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setTakenNumbers([]);
        }
      })
      .finally(() => {
        if (!cancelled) {
          setIsLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [sport, teamId]);

  return { takenNumbers, isLoading };
}

async function loadTakenJerseyNumbers(
  sport: JerseyNumberSport,
  teamId: string,
): Promise<number[]> {
  if (sport === 'floorball') {
    const team = await floorballTeamService.getById(teamId);
    return collectJerseyNumbers(team.roster);
  }
  if (sport === 'football') {
    const team = await footballTeamService.getById(teamId);
    return collectJerseyNumbers(team.roster);
  }
  const team = await hockeyTeamService.getById(teamId);
  return collectJerseyNumbers(team.roster);
}
