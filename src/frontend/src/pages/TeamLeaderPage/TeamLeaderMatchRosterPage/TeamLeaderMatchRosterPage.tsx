import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import TeamLeaderPageTemplate from '../components/TeamLeaderPageTemplate';
import { teamLeaderService } from '../../../api/teamLeader/teamLeaderService';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { footballTeamService } from '../../../api/football/footballTeamService';
import { floorballMatchService } from '../../../api/floorball/floorballMatchService';
import { footballMatchService } from '../../../api/football/footballMatchService';
import { FloorballPosition } from '../../../types/floorball/floorballTypes';
import { FootballPosition } from '../../../types/football/footballTypes';
import type { TeamLeaderSport } from '../../../types/teamLeader/teamLeaderTypes';
import './TeamLeaderMatchRosterPage.scss';

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

function TeamLeaderMatchRosterPage() {
  const { t, i18n } = useTranslation();
  const { sport, teamId, matchId } = useParams<{ sport: TeamLeaderSport; teamId: string; matchId: string }>();
  const isFloorball = sport === 'floorball';

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
      if (isFloorball) {
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
      setError(err instanceof Error ? err.message : t('teamLeader.loadMatchError', 'Failed to load the match'));
    } finally {
      setIsLoading(false);
    }
  }, [sport, teamId, matchId, isFloorball, t]);

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
      if (isFloorball) {
        const selectedPlayers = players
          .filter((p) => selections[p.playerId]?.selected && p.playerId !== goalieId)
          .map((p) => ({
            playerId: p.playerId,
            position: selections[p.playerId].position as FloorballPosition,
          }));

        await teamLeaderService.announceFloorballRoster(matchId, teamId, {
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

        await teamLeaderService.announceFootballLineup(matchId, teamId, selectedPlayers);
      }

      setHasExistingRoster(true);
      setSuccessMessage(t('teamLeader.rosterSaved', 'The match roster has been announced. It is now visible in the match view.'));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : t('teamLeader.rosterSaveError', 'Failed to announce the match roster'));
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

  const selectedCount = players.filter((p) => selections[p.playerId]?.selected && p.playerId !== goalieId).length;
  const isScheduled = matchInfo?.status === 'Scheduled';

  return (
    <TeamLeaderPageTemplate title={t('teamLeader.announceRosterTitle', 'Announce match roster')}>
      <Link to="/team-leader" className="team-leader-back-link">
        ← {t('teamLeader.backToTeams', 'Back to my teams')}
      </Link>

      {isLoading && <div className="team-leader-loading">{t('common.loading', 'Loading...')}</div>}
      {error && <div className="team-leader-error">{error}</div>}
      {successMessage && <div className="team-leader-success">{successMessage}</div>}

      {!isLoading && matchInfo && (
        <>
          <div className="team-leader-match-summary">
            <span className="team-leader-match-teams">
              {matchInfo.homeTeamName ?? 'TBD'} – {matchInfo.awayTeamName ?? 'TBD'}
            </span>
            <span className="team-leader-match-meta">
              {formatDate(matchInfo.scheduledDateTime)}
              {matchInfo.venue ? ` · ${matchInfo.venue}` : ''}
            </span>
            {hasExistingRoster && (
              <span className="team-leader-announced-badge">
                {t('teamLeader.rosterAnnounced', 'Roster announced')}
              </span>
            )}
          </div>

          {!isScheduled && (
            <div className="team-leader-error">
              {t('teamLeader.matchNotScheduled', 'The roster can no longer be changed because the match is not in the scheduled state.')}
            </div>
          )}

          {isScheduled && (
            <div className="team-leader-roster-card">
              {isFloorball && (
                <div className="team-leader-goalie-row">
                  <label htmlFor="goalie-select" className="team-leader-goalie-label">
                    {t('teamLeader.goalie', 'Goalie')}
                  </label>
                  <select
                    id="goalie-select"
                    className="team-leader-position-select"
                    value={goalieId}
                    onChange={(e) => setGoalieId(e.target.value)}
                    disabled={isSaving}
                  >
                    <option value="">{t('teamLeader.noGoalie', 'No goalie selected')}</option>
                    {players.map((p) => (
                      <option key={p.playerId} value={p.playerId}>
                        {p.jerseyNumber != null ? `#${p.jerseyNumber} ` : ''}{p.playerName}
                      </option>
                    ))}
                  </select>
                </div>
              )}

              <p className="team-leader-roster-hint">
                {isFloorball
                  ? t('teamLeader.floorballRosterHint', 'Select the field players for this match and set their role. The goalie is selected separately above.')
                  : t('teamLeader.footballRosterHint', 'Select the players for this match, set their position, and mark the starting lineup.')}
              </p>

              <table className="team-leader-roster-table">
                <thead>
                  <tr>
                    <th className="team-leader-select-col">{t('teamLeader.inRoster', 'In roster')}</th>
                    <th>{t('teamLeader.playerName', 'Player')}</th>
                    <th>{t('teamLeader.matchPosition', 'Match position')}</th>
                    {!isFloorball && <th>{t('teamLeader.starting', 'Starting')}</th>}
                  </tr>
                </thead>
                <tbody>
                  {players.map((player) => {
                    const selection = selections[player.playerId];
                    const isGoalie = isFloorball && goalieId === player.playerId;
                    return (
                      <tr key={player.playerId} className={isGoalie ? 'team-leader-roster-row--goalie' : ''}>
                        <td>
                          {isGoalie ? (
                            <span className="team-leader-goalie-tag">{t('teamLeader.goalie', 'Goalie')}</span>
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
                            <span className="team-leader-jersey-badge">#{player.jerseyNumber}</span>
                          )}
                          {player.playerName}
                        </td>
                        <td>
                          {!isGoalie && (
                            <select
                              className="team-leader-position-select"
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
                        {!isFloorball && (
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
                      <td colSpan={isFloorball ? 3 : 4} className="team-leader-roster-empty">
                        {t('teamLeader.emptyRoster', 'This team has no players on its roster yet.')}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>

              <div className="team-leader-save-row">
                <span className="team-leader-selected-count">
                  {t('teamLeader.selectedPlayers', 'Selected players')}: {selectedCount}
                </span>
                <button
                  type="button"
                  className="team-leader-button team-leader-button--primary"
                  onClick={() => { void handleSave(); }}
                  disabled={isSaving}
                >
                  {isSaving
                    ? t('teamLeader.saving', 'Saving...')
                    : hasExistingRoster
                      ? t('teamLeader.updateRoster', 'Update announced roster')
                      : t('teamLeader.announceRoster', 'Announce roster')}
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </TeamLeaderPageTemplate>
  );
}

export default TeamLeaderMatchRosterPage;
