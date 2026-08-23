import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ClubAdminPageTemplate from '../components/ClubAdminPageTemplate';
import { clubAdminService } from '../../../api/clubAdmin/clubAdminService';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { footballTeamService } from '../../../api/football/footballTeamService';
import { hockeyTeamService } from '../../../api/hockey/hockeyTeamService';
import { floorballMatchService } from '../../../api/floorball/floorballMatchService';
import { footballMatchService } from '../../../api/football/footballMatchService';
import { hockeyMatchService } from '../../../api/hockey/hockeyMatchService';
import { FloorballPosition } from '../../../types/floorball/floorballTypes';
import { FootballPosition } from '../../../types/football/footballTypes';
import type { ClubAdminSport } from '../../../types/clubAdmin/clubAdminTypes';
import { loadHockeyRosterNameMaps, loadTeamNameMap } from '../../../utils/hockeyLookups';
import './ClubAdminMatchRosterPage.scss';

interface RosterPlayerOption {
  playerId: string;
  playerName: string;
  jerseyNumber: number | null;
  defaultPosition: string;
}

interface SelectionState {
  selected: boolean;
  position: string;
  isOnField: boolean;
}

interface MatchInfo {
  homeTeamName?: string | null;
  awayTeamName?: string | null;
  scheduledDateTime: string;
  venue?: string | null;
  status: string;
}

const FLOORBALL_FIELD_POSITIONS: FloorballPosition[] = [
  FloorballPosition.Defender,
  FloorballPosition.Center,
  FloorballPosition.Forward,
];

const FOOTBALL_POSITIONS: FootballPosition[] = [
  FootballPosition.Goalkeeper,
  FootballPosition.Defender,
  FootballPosition.Midfielder,
  FootballPosition.Forward,
];

function defaultFloorballFieldPosition(position: string): string {
  return FLOORBALL_FIELD_POSITIONS.includes(position as FloorballPosition)
    ? position
    : FloorballPosition.Defender;
}

function defaultFootballPosition(position: string): string {
  return FOOTBALL_POSITIONS.includes(position as FootballPosition)
    ? position
    : FootballPosition.Midfielder;
}

function ClubAdminMatchRosterPage() {
  const { t, i18n } = useTranslation();
  const { sport, teamId, matchId } = useParams<{ sport: ClubAdminSport; teamId: string; matchId: string }>();
  const isFloorball = sport === 'floorball';
  const isHockey = sport === 'hockey';

  const [matchInfo, setMatchInfo] = useState<MatchInfo | null>(null);
  const [players, setPlayers] = useState<RosterPlayerOption[]>([]);
  const [selections, setSelections] = useState<Record<string, SelectionState>>({});
  const [goalieId, setGoalieId] = useState<string>('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [hasExistingRoster, setHasExistingRoster] = useState(false);

  const load = useCallback(async () => {
    if (!sport || !teamId || !matchId) return;
    try {
      if (isHockey) {
        const [team, match] = await Promise.all([
          hockeyTeamService.getById(teamId),
          hockeyMatchService.getById(matchId),
        ]);
        const teamNames = await loadTeamNameMap();
        const names = await loadHockeyRosterNameMaps([team]);
        const matchTeam = match.matchTeams.find((side) => side.teamId === teamId);
        const dressed = matchTeam?.activePlayers ?? [];

        setMatchInfo({
          homeTeamName: match.homeTeamId ? teamNames.get(match.homeTeamId) ?? null : null,
          awayTeamName: match.awayTeamId ? teamNames.get(match.awayTeamId) ?? null : null,
          scheduledDateTime: match.scheduledStartTime,
          venue: match.venue,
          status: match.status,
        });

        const activeRoster = team.roster.filter((p) => p.isActive);
        setPlayers(activeRoster.map((p) => ({
          playerId: p.id,
          playerName: names.byTeamPlayerId.get(p.id) ?? p.playerId.slice(0, 8),
          jerseyNumber: p.jerseyNumber ?? null,
          defaultPosition: p.position,
        })));

        const initialSelections: Record<string, SelectionState> = {};
        activeRoster.forEach((p) => {
          const existing = dressed.find((ap) => ap.teamPlayerId === p.id);
          initialSelections[p.id] = {
            selected: existing !== undefined,
            position: p.position,
            isOnField: true,
          };
        });
        setSelections(initialSelections);
        setHasExistingRoster(Boolean(matchTeam?.isConfirmedRoster || dressed.length > 0));
      } else if (isFloorball) {
        const [team, matchResponse] = await Promise.all([
          floorballTeamService.getById(teamId),
          floorballMatchService.getById(matchId),
        ]);
        const match = matchResponse.data;
        const isHome = match.homeTeamId === teamId;
        const activePlayers = (isHome ? match.homeActivePlayers : match.awayActivePlayers) ?? [];
        const activeGoalie = isHome ? match.homeActiveGoalieId : match.awayActiveGoalieId;

        setMatchInfo({
          homeTeamName: match.homeTeamName,
          awayTeamName: match.awayTeamName,
          scheduledDateTime: match.scheduledDateTime,
          venue: match.venue,
          status: match.status,
        });

        const activeRoster = team.roster.filter((p) => p.isActive);
        setPlayers(activeRoster.map((p) => ({
          playerId: p.playerId,
          playerName: p.playerName,
          jerseyNumber: p.jerseyNumber ?? null,
          defaultPosition: p.position,
        })));

        const initialSelections: Record<string, SelectionState> = {};
        activeRoster.forEach((p) => {
          const existing = activePlayers.find((ap) => ap.playerId === p.playerId);
          initialSelections[p.playerId] = {
            selected: existing !== undefined,
            position: existing?.position ?? defaultFloorballFieldPosition(p.position),
            isOnField: true,
          };
        });
        setSelections(initialSelections);
        setGoalieId(activeGoalie ?? '');
        setHasExistingRoster(activePlayers.length > 0);
      } else {
        const [team, matchResponse] = await Promise.all([
          footballTeamService.getById(teamId),
          footballMatchService.getById(matchId),
        ]);
        const match = matchResponse.data;
        const isHome = match.homeTeamId === teamId;
        const lineup = (isHome ? match.homeLineup : match.awayLineup) ?? [];

        setMatchInfo({
          homeTeamName: match.homeTeamName,
          awayTeamName: match.awayTeamName,
          scheduledDateTime: match.scheduledDateTime,
          venue: match.venue,
          status: match.status,
        });

        const activeRoster = team.roster.filter((p) => p.isActive);
        setPlayers(activeRoster.map((p) => ({
          playerId: p.playerId,
          playerName: p.playerName,
          jerseyNumber: p.jerseyNumber ?? null,
          defaultPosition: p.position,
        })));

        const initialSelections: Record<string, SelectionState> = {};
        activeRoster.forEach((p) => {
          const existing = lineup.find((lp) => lp.playerId === p.playerId);
          initialSelections[p.playerId] = {
            selected: existing !== undefined,
            position: existing?.position ?? defaultFootballPosition(p.position),
            isOnField: existing?.isOnField ?? false,
          };
        });
        setSelections(initialSelections);
        setHasExistingRoster(lineup.length > 0);
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : t('clubAdmin.loadMatchError', 'Failed to load the match'));
    } finally {
      setIsLoading(false);
    }
  }, [sport, teamId, matchId, isFloorball, isHockey, t]);

  useEffect(() => {
    void load();
  }, [load]);

  const toggleSelected = (playerId: string) => {
    setSelections((prev) => ({
      ...prev,
      [playerId]: { ...prev[playerId], selected: !prev[playerId].selected },
    }));
  };

  const setPosition = (playerId: string, position: string) => {
    setSelections((prev) => ({
      ...prev,
      [playerId]: { ...prev[playerId], position },
    }));
  };

  const toggleOnField = (playerId: string) => {
    setSelections((prev) => ({
      ...prev,
      [playerId]: { ...prev[playerId], isOnField: !prev[playerId].isOnField },
    }));
  };

  const handleSave = async () => {
    if (!sport || !teamId || !matchId) return;
    setIsSaving(true);
    setError(null);
    setSuccessMessage(null);

    try {
      if (isHockey) {
        const teamPlayerIds = players
          .filter((p) => selections[p.playerId]?.selected)
          .map((p) => p.playerId);
        await clubAdminService.announceHockeyRoster(matchId, teamId, teamPlayerIds);
      } else if (isFloorball) {
        const selectedPlayers = players
          .filter((p) => selections[p.playerId]?.selected && p.playerId !== goalieId)
          .map((p) => ({
            playerId: p.playerId,
            position: selections[p.playerId].position as FloorballPosition,
          }));

        await clubAdminService.announceFloorballRoster(matchId, teamId, {
          players: selectedPlayers,
          goalieId: goalieId || null,
        });
      } else {
        const selectedPlayers = players
          .filter((p) => selections[p.playerId]?.selected)
          .map((p) => ({
            playerId: p.playerId,
            position: selections[p.playerId].position as FootballPosition,
            isOnField: selections[p.playerId].isOnField,
          }));

        await clubAdminService.announceFootballLineup(matchId, teamId, selectedPlayers);
      }

      setHasExistingRoster(true);
      setSuccessMessage(t('clubAdmin.rosterSaved', 'The match roster has been announced. It is now visible in the match view.'));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : t('clubAdmin.rosterSaveError', 'Failed to announce the match roster'));
    } finally {
      setIsSaving(false);
    }
  };

  const formatDate = (iso: string): string => {
    const date = new Date(iso);
    return date.toLocaleString(i18n.language === 'fi' ? 'fi-FI' : 'en-GB', {
      day: 'numeric', month: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit',
    });
  };

  const selectedCount = isHockey
    ? players.filter((p) => selections[p.playerId]?.selected).length
    : players.filter((p) => selections[p.playerId]?.selected && p.playerId !== goalieId).length;
  const isScheduled = matchInfo?.status === 'Scheduled';

  return (
    <ClubAdminPageTemplate title={t('clubAdmin.announceRosterTitle', 'Announce match roster')}>
      <Link to="/club-admin" className="club-admin-back-link">
        ← {t('clubAdmin.backToClubs', 'Back to my clubs')}
      </Link>

      {isLoading && <div className="club-admin-loading">{t('common.loading', 'Loading...')}</div>}
      {error && <div className="club-admin-error">{error}</div>}
      {successMessage && <div className="club-admin-success">{successMessage}</div>}

      {!isLoading && matchInfo && (
        <>
          <div className="club-admin-match-summary">
            <span className="club-admin-match-teams">
              {matchInfo.homeTeamName ?? 'TBD'} – {matchInfo.awayTeamName ?? 'TBD'}
            </span>
            <span className="club-admin-match-meta">
              {formatDate(matchInfo.scheduledDateTime)}
              {matchInfo.venue ? ` · ${matchInfo.venue}` : ''}
            </span>
            {hasExistingRoster && (
              <span className="club-admin-announced-badge">
                {t('clubAdmin.rosterAnnounced', 'Roster announced')}
              </span>
            )}
          </div>

          {!isScheduled && (
            <div className="club-admin-error">
              {t('clubAdmin.matchNotScheduled', 'The roster can no longer be changed because the match is not in the scheduled state.')}
            </div>
          )}

          {isScheduled && (
            <div className="club-admin-roster-card">
              {isFloorball && !isHockey && (
                <div className="club-admin-goalie-row">
                  <label htmlFor="goalie-select" className="club-admin-goalie-label">
                    {t('clubAdmin.goalie', 'Goalie')}
                  </label>
                  <select
                    id="goalie-select"
                    className="club-admin-position-select"
                    value={goalieId}
                    onChange={(e) => setGoalieId(e.target.value)}
                    disabled={isSaving}
                  >
                    <option value="">{t('clubAdmin.noGoalie', 'No goalie selected')}</option>
                    {players.map((p) => (
                      <option key={p.playerId} value={p.playerId}>
                        {p.jerseyNumber != null ? `#${p.jerseyNumber} ` : ''}{p.playerName}
                      </option>
                    ))}
                  </select>
                </div>
              )}

              <p className="club-admin-roster-hint">
                {isHockey
                  ? t('clubAdmin.hockeyRosterHint', 'Select the dressed players for this match. A goalie is required and at least 15 players must be selected.')
                  : isFloorball
                    ? t('clubAdmin.floorballRosterHint', 'Select the field players for this match and set their role. The goalie is selected separately above.')
                    : t('clubAdmin.footballRosterHint', 'Select the players for this match, set their position, and mark the starting lineup.')}
              </p>

              <table className="club-admin-roster-table">
                <thead>
                  <tr>
                    <th className="club-admin-select-col">{t('clubAdmin.inRoster', 'In roster')}</th>
                    <th>{t('clubAdmin.playerName', 'Player')}</th>
                    {!isHockey && <th>{t('clubAdmin.matchPosition', 'Match position')}</th>}
                    {!isFloorball && !isHockey && <th>{t('clubAdmin.starting', 'Starting')}</th>}
                  </tr>
                </thead>
                <tbody>
                  {players.map((player) => {
                    const selection = selections[player.playerId];
                    const isGoalie = isFloorball && goalieId === player.playerId;
                    return (
                      <tr key={player.playerId} className={isGoalie ? 'club-admin-roster-row--goalie' : ''}>
                        <td>
                          {isGoalie ? (
                            <span className="club-admin-goalie-tag">{t('clubAdmin.goalie', 'Goalie')}</span>
                          ) : (
                            <input
                              type="checkbox"
                              checked={selection?.selected ?? false}
                              onChange={() => toggleSelected(player.playerId)}
                              disabled={isSaving}
                            />
                          )}
                        </td>
                        <td>
                          {player.jerseyNumber != null && (
                            <span className="club-admin-jersey-badge">#{player.jerseyNumber}</span>
                          )}
                          {player.playerName}
                          {isHockey && player.defaultPosition && (
                            <span className="club-admin-upcoming-meta">
                              {' '}
                              {t(`hockey.positions.${player.defaultPosition}`, player.defaultPosition)}
                            </span>
                          )}
                        </td>
                        {!isHockey && (
                        <td>
                          {!isGoalie && (
                            <select
                              className="club-admin-position-select"
                              value={selection?.position ?? ''}
                              onChange={(e) => setPosition(player.playerId, e.target.value)}
                              disabled={isSaving || !selection?.selected}
                            >
                              {(isFloorball ? FLOORBALL_FIELD_POSITIONS : FOOTBALL_POSITIONS).map((pos) => (
                                <option key={pos} value={pos}>{t(`positions.${pos}`, pos)}</option>
                              ))}
                            </select>
                          )}
                        </td>
                        )}
                        {!isFloorball && !isHockey && (
                          <td>
                            <input
                              type="checkbox"
                              checked={selection?.isOnField ?? false}
                              onChange={() => toggleOnField(player.playerId)}
                              disabled={isSaving || !selection?.selected}
                            />
                          </td>
                        )}
                      </tr>
                    );
                  })}
                  {players.length === 0 && (
                    <tr>
                      <td colSpan={isHockey ? 2 : isFloorball ? 3 : 4} className="club-admin-roster-empty">
                        {t('clubAdmin.emptyRoster', 'This team has no players on its roster yet.')}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>

              <div className="club-admin-save-row">
                <span className="club-admin-selected-count">
                  {t('clubAdmin.selectedPlayers', 'Selected players')}: {selectedCount}
                </span>
                <button
                  type="button"
                  className="club-admin-button club-admin-button--primary"
                  onClick={() => { void handleSave(); }}
                  disabled={isSaving}
                >
                  {isSaving
                    ? t('clubAdmin.saving', 'Saving...')
                    : hasExistingRoster
                      ? t('clubAdmin.updateRoster', 'Update announced roster')
                      : t('clubAdmin.announceRoster', 'Announce roster')}
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </ClubAdminPageTemplate>
  );
}

export default ClubAdminMatchRosterPage;
