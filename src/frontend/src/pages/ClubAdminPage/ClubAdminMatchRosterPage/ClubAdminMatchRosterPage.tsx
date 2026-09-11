import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ClubAdminPageTemplate from '../components/ClubAdminPageTemplate';
import SearchField from '../../../components/SearchField';
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
import GoalieSearchSelect from './components/GoalieSearchSelect';
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

type RosterListFilter = 'all' | 'selected' | 'available';

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

function matchesPlayerSearch(player: RosterPlayerOption, query: string): boolean {
  const needle = query.trim().toLowerCase();
  if (!needle) return true;
  const jersey = player.jerseyNumber != null ? String(player.jerseyNumber) : '';
  const haystack = `${player.playerName} ${jersey} #${jersey}`.toLowerCase();
  return haystack.includes(needle);
}

function isInMatchRoster(
  player: RosterPlayerOption,
  selections: Record<string, SelectionState>,
  goalieId: string,
  isFloorball: boolean,
): boolean {
  if (isFloorball && player.playerId === goalieId) return true;
  return Boolean(selections[player.playerId]?.selected);
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
  const [searchQuery, setSearchQuery] = useState('');
  const [listFilter, setListFilter] = useState<RosterListFilter>('all');
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

  const filteredPlayers = useMemo(() => {
    return players
      .filter((player) => {
        if (!matchesPlayerSearch(player, searchQuery)) return false;
        const inRoster = isInMatchRoster(player, selections, goalieId, isFloorball);
        if (listFilter === 'selected') return inRoster;
        if (listFilter === 'available') return !inRoster;
        return true;
      })
      .sort((a, b) => {
        const aGoalie = isFloorball && a.playerId === goalieId;
        const bGoalie = isFloorball && b.playerId === goalieId;
        if (aGoalie !== bGoalie) return aGoalie ? -1 : 1;
        const aNum = a.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
        const bNum = b.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
        if (aNum !== bNum) return aNum - bNum;
        return a.playerName.localeCompare(b.playerName, undefined, { sensitivity: 'base' });
      });
  }, [players, searchQuery, listFilter, selections, goalieId, isFloorball]);

  const toggleSelected = (playerId: string) => {
    setSelections((prev) => ({
      ...prev,
      [playerId]: { ...prev[playerId], selected: !prev[playerId].selected },
    }));
  };

  const setVisibleSelected = (selected: boolean) => {
    const visibleIds = new Set(
      filteredPlayers
        .filter((player) => !(isFloorball && player.playerId === goalieId))
        .map((player) => player.playerId),
    );
    setSelections((prev) => {
      const next: Record<string, SelectionState> = { ...prev };
      visibleIds.forEach((playerId) => {
        next[playerId] = { ...next[playerId], selected };
      });
      return next;
    });
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
  const columnCount = isHockey ? 2 : isFloorball ? 3 : 4;
  const hasActiveFilters = searchQuery.trim() !== '' || listFilter !== 'all';

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
            <div className="club-admin-match-summary__main">
              <span className="club-admin-match-teams">
                {matchInfo.homeTeamName ?? 'TBD'} – {matchInfo.awayTeamName ?? 'TBD'}
              </span>
              <span className="club-admin-match-meta">
                {formatDate(matchInfo.scheduledDateTime)}
                {matchInfo.venue ? ` · ${matchInfo.venue}` : ''}
              </span>
            </div>
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
                    {t('clubAdmin.goalie')}
                  </label>
                  <GoalieSearchSelect
                    players={players}
                    value={goalieId}
                    onChange={setGoalieId}
                    disabled={isSaving}
                  />
                </div>
              )}

              <p className="club-admin-roster-hint">
                {isHockey
                  ? t('clubAdmin.hockeyRosterHint', 'Select the dressed players for this match. A goalie is required and at least 15 players must be selected.')
                  : isFloorball
                    ? t('clubAdmin.floorballRosterHint', 'Select the field players for this match and set their role. The goalie is selected separately above.')
                    : t('clubAdmin.footballRosterHint', 'Select the players for this match, set their position, and mark the starting lineup.')}
              </p>

              <div className="club-admin-roster-toolbar">
                <SearchField
                  value={searchQuery}
                  onChange={setSearchQuery}
                  placeholder={t('clubAdmin.searchPlayers', 'Search by name or jersey number...')}
                  rounded="md"
                  size="sm"
                />
                <div className="club-admin-roster-filters" role="group" aria-label={t('clubAdmin.filterAll', 'All')}>
                  {([
                    ['all', t('clubAdmin.filterAll', 'All')],
                    ['selected', t('clubAdmin.filterSelected', 'In roster')],
                    ['available', t('clubAdmin.filterAvailable', 'Not in roster')],
                  ] as const).map(([value, label]) => (
                    <button
                      key={value}
                      type="button"
                      className={`club-admin-filter-chip${listFilter === value ? ' club-admin-filter-chip--active' : ''}`}
                      aria-pressed={listFilter === value}
                      onClick={() => setListFilter(value)}
                    >
                      {label}
                    </button>
                  ))}
                </div>
              </div>

              <div className="club-admin-roster-toolbar-meta">
                <span className="club-admin-roster-count">
                  {t('clubAdmin.showingPlayers', '{{shown}} / {{total}} players', {
                    shown: filteredPlayers.length,
                    total: players.length,
                  })}
                </span>
                {filteredPlayers.length > 0 && (
                  <div className="club-admin-roster-bulk">
                    <button
                      type="button"
                      className="club-admin-text-button"
                      onClick={() => setVisibleSelected(true)}
                      disabled={isSaving}
                    >
                      {t('clubAdmin.selectVisible', 'Select visible')}
                    </button>
                    <button
                      type="button"
                      className="club-admin-text-button"
                      onClick={() => setVisibleSelected(false)}
                      disabled={isSaving}
                    >
                      {t('clubAdmin.clearVisible', 'Clear visible')}
                    </button>
                  </div>
                )}
              </div>

              <div className="club-admin-roster-table-wrap">
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
                    {filteredPlayers.map((player) => {
                      const selection = selections[player.playerId];
                      const isGoalie = isFloorball && goalieId === player.playerId;
                      const isSelected = isInMatchRoster(player, selections, goalieId, isFloorball);
                      const rowClass = [
                        isGoalie ? 'club-admin-roster-row--goalie' : '',
                        isSelected && !isGoalie ? 'club-admin-roster-row--selected' : '',
                      ].filter(Boolean).join(' ');
                      return (
                        <tr key={player.playerId} className={rowClass || undefined}>
                          <td>
                            {isGoalie ? (
                              <span className="club-admin-goalie-tag">{t('clubAdmin.goalie', 'Goalie')}</span>
                            ) : (
                              <label className="club-admin-roster-check">
                                <input
                                  type="checkbox"
                                  checked={selection?.selected ?? false}
                                  onChange={() => toggleSelected(player.playerId)}
                                  disabled={isSaving}
                                />
                                <span className="club-admin-visually-hidden">
                                  {t('clubAdmin.inRoster', 'In roster')}
                                </span>
                              </label>
                            )}
                          </td>
                          <td>
                            <div className="club-admin-player-cell">
                              <span className="club-admin-jersey-badge">
                                {player.jerseyNumber != null ? `#${player.jerseyNumber}` : '—'}
                              </span>
                              <span className="club-admin-player-name">{player.playerName}</span>
                              {isHockey && player.defaultPosition && (
                                <span className="club-admin-upcoming-meta">
                                  {t(`hockey.positions.${player.defaultPosition}`, player.defaultPosition)}
                                </span>
                              )}
                            </div>
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
                        <td colSpan={columnCount} className="club-admin-roster-empty">
                          {t('clubAdmin.emptyRoster', 'This team has no players on its roster yet.')}
                        </td>
                      </tr>
                    )}
                    {players.length > 0 && filteredPlayers.length === 0 && (
                      <tr>
                        <td colSpan={columnCount} className="club-admin-roster-empty">
                          {t('clubAdmin.searchNoResults', 'No players match the current search.')}
                          {hasActiveFilters && (
                            <>
                              {' '}
                              <button
                                type="button"
                                className="club-admin-text-button"
                                onClick={() => {
                                  setSearchQuery('');
                                  setListFilter('all');
                                }}
                              >
                                {t('common.clear', 'Clear')}
                              </button>
                            </>
                          )}
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>

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
