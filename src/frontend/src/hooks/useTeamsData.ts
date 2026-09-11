import {useState, useEffect, useCallback} from 'react'
import type { FloorballTeam, GetFloorballTeamsRequest } from '../types/floorball/floorballTypes'
import type { FootballTeam, GetFootballTeamsRequest } from '../types/football/footballTypes'
import { floorballTeamService } from '../api/floorball/floorballTeamService'
import { footballTeamService } from '../api/football/footballTeamService'

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

export function useFootballTeamsData() {
  const [teams, setTeams] = useState<FootballTeam[]>([]);
  const [params, setParams] = useState<GetFootballTeamsRequest>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<unknown>(null);

  const fetchTeams = useCallback(async () => {
    setIsLoading(true);
    try {
      const { data } = await footballTeamService.getAll(params);
      setTeams(data);
      setError(null);
    } catch (err) {
      setError(err);
      setTeams([]);
    } finally {
      setIsLoading(false);
    }
  }, [params]);

  useEffect(() => {
    if (!params) return;
    fetchTeams();
  }, [params, fetchTeams]);

  return { teams, isLoading, error, setParams, refetch: fetchTeams };
}