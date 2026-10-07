import { VITE_API_URL } from '../../constants/config';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { authFetch } from '../utils/authFetch';
import type { SportKind } from '../../utils/sportRoutes';
import type { TeamCategory } from '../../types/floorball/floorballTypes';

const SEASON_CONTROLLERS: Record<SportKind, string> = {
  floorball: 'FloorballSeason',
  football: 'FootballSeason',
  hockey: 'HockeySeason',
};

/**
 * Moves a season to another audience group. Works in any season status, also for completed seasons.
 */
export async function changeSeasonTeamCategory(
  sport: SportKind,
  seasonId: string,
  teamCategory: TeamCategory,
): Promise<void> {
  const response = await authFetch(`${VITE_API_URL}/${SEASON_CONTROLLERS[sport]}/${seasonId}/team-category`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ teamCategory }),
  });
  if (!response.ok) {
    throw new Error(await parseErrorResponse(response, 'Failed to change the season group'));
  }
}
