import {useState, useEffect, useCallback} from 'react'
import type { FloorballTeam, GetFloorballTeamsRequest } from '../types/floorball/floorballTypes'
import { floorballTeamService } from '../api/floorball/floorballTeamService'

export function useFloorballTeamsData() {
  const [teams, setTeams] = useState<FloorballTeam[]>([]);
  const [params, setParams] = useState<GetFloorballTeamsRequest>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<unknown>(null);

  const fetchTeams = useCallback(async () => {
    setIsLoading(true)
    try {
      const { data } = await floorballTeamService.getAll(params);
      setTeams(data);
      setError(null);
    }catch (err) {
      setError(err);
      setTeams([]);
    }finally {
      setIsLoading(false);       
    }
  }, [params]);

  // Skip initial fetch until params are defined—avoids loading *all* teams on first render
  useEffect(() => {
    if (!params) return; // wait until there are meaningful params
    fetchTeams();
  }, [params, fetchTeams]);

  return { teams, isLoading, error, setParams, refetch: fetchTeams };
}