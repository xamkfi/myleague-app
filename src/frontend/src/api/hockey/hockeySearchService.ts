import { hockeyTeamService } from './hockeyTeamService';
import { hockeySeasonService } from './hockeySeasonService';
import { hockeyTournamentService } from './hockeyTournamentService';

export interface HockeyDropdownOption {
  id: string;
  name: string;
}

export interface HockeySearchResult {
  data: HockeyDropdownOption[];
  pagination: {
    hasNextPage: boolean;
    totalCount: number;
  };
}

function filterPage(
  items: HockeyDropdownOption[],
  query: string,
  page: number,
  pageSize = 50,
): HockeySearchResult {
  const needle = query.trim().toLowerCase();
  const filtered = needle
    ? items.filter((item) => item.name.toLowerCase().includes(needle))
    : items;
  const start = (page - 1) * pageSize;
  const slice = filtered.slice(start, start + pageSize);
  return {
    data: slice,
    pagination: {
      hasNextPage: start + pageSize < filtered.length,
      totalCount: filtered.length,
    },
  };
}

export const hockeyTeamSearchService = {
  searchTeams: async (query: string, page: number): Promise<HockeySearchResult> => {
    const teams = await hockeyTeamService.getAll();
    return filterPage(
      teams.map((team) => ({ id: team.id, name: team.name })),
      query,
      page,
    );
  },
};

export const hockeySeasonSearchService = {
  searchSeasons: async (query: string, page: number): Promise<HockeySearchResult> => {
    const seasons = await hockeySeasonService.getAll();
    return filterPage(
      seasons.map((season) => ({ id: season.id, name: season.name })),
      query,
      page,
    );
  },
};

export const hockeyTournamentSearchService = {
  searchTournaments: async (query: string, page: number): Promise<HockeySearchResult> => {
    const tournaments = await hockeyTournamentService.getAll();
    return filterPage(
      tournaments.map((tournament) => ({ id: tournament.id, name: tournament.name })),
      query,
      page,
    );
  },
};
