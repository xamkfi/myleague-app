import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import TeamLeaderPageTemplate from '../components/TeamLeaderPageTemplate';
import { teamLeaderService } from '../../../api/teamLeader/teamLeaderService';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { footballTeamService } from '../../../api/football/footballTeamService';
import type { TeamLeaderSport } from '../../../types/teamLeader/teamLeaderTypes';
import './TeamLeaderRosterPage.scss';

interface RosterRow {
  playerId: string;
  playerName: string;
  position: string;
  isActive: boolean;
  jerseyNumber: number | null;
}

const JERSEY_OPTIONS: number[] = Array.from({ length: 99 }, (_, i) => i + 1);

function TeamLeaderRosterPage() {
  const { t } = useTranslation();
  const { sport, teamId } = useParams<{ sport: TeamLeaderSport; teamId: string }>();

  const [teamName, setTeamName] = useState('');
  const [roster, setRoster] = useState<RosterRow[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [savingPlayerId, setSavingPlayerId] = useState<string | null>(null);
  const [rowErrors, setRowErrors] = useState<Record<string, string>>({});
  const [savedPlayerId, setSavedPlayerId] = useState<string | null>(null);

  const loadTeam = useCallback(async () => {
    if (!sport || !teamId) return;
    try {
      const team = sport === 'floorball'
        ? await floorballTeamService.getById(teamId)
        : await footballTeamService.getById(teamId);
      setTeamName(team.name);
      setRoster(team.roster.map((player) => ({
        playerId: player.playerId,
        playerName: player.playerName,
        position: player.position,
        isActive: player.isActive,
        jerseyNumber: player.jerseyNumber ?? null,
      })));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : t('teamLeader.loadRosterError', 'Failed to load the roster'));
    } finally {
      setIsLoading(false);
    }
  }, [sport, teamId, t]);

  useEffect(() => {
    void loadTeam();
  }, [loadTeam]);

  const handleJerseyChange = async (playerId: string, value: string) => {
    if (!sport || !teamId) return;
    const newNumber = value === '' ? null : Number(value);

    setSavingPlayerId(playerId);
    setSavedPlayerId(null);
    setRowErrors((prev) => {
      const next = { ...prev };
      delete next[playerId];
      return next;
    });

    try {
      await teamLeaderService.updateJerseyNumber(sport, teamId, playerId, newNumber);
      setRoster((prev) => prev.map((row) => (
        row.playerId === playerId ? { ...row, jerseyNumber: newNumber } : row
      )));
      setSavedPlayerId(playerId);
    } catch (err: unknown) {
      setRowErrors((prev) => ({
        ...prev,
        [playerId]: err instanceof Error ? err.message : t('teamLeader.jerseyUpdateError', 'Failed to update the jersey number'),
      }));
    } finally {
      setSavingPlayerId(null);
    }
  };

  return (
    <TeamLeaderPageTemplate title={teamName ? `${teamName} – ${t('teamLeader.rosterTitle', 'Roster')}` : t('teamLeader.rosterTitle', 'Roster')}>
      <Link to="/team-leader" className="team-leader-back-link">
        ← {t('teamLeader.backToTeams', 'Back to my teams')}
      </Link>

      {isLoading && <div className="team-leader-loading">{t('common.loading', 'Loading...')}</div>}
      {error && <div className="team-leader-error">{error}</div>}

      {!isLoading && !error && (
        <div className="team-leader-roster-card">
          <p className="team-leader-roster-hint">
            {t('teamLeader.rosterHint', 'You can change the jersey numbers of your players. Each number can only be used once per team.')}
          </p>
          <table className="team-leader-roster-table">
            <thead>
              <tr>
                <th className="team-leader-roster-jersey-col">{t('teamLeader.jerseyNumber', 'Jersey #')}</th>
                <th>{t('teamLeader.playerName', 'Player')}</th>
                <th>{t('teamLeader.position', 'Position')}</th>
                <th>{t('teamLeader.status', 'Status')}</th>
              </tr>
            </thead>
            <tbody>
              {roster.map((row) => (
                <tr key={row.playerId} className={row.isActive ? '' : 'team-leader-roster-row--inactive'}>
                  <td>
                    <select
                      className="team-leader-jersey-select"
                      value={row.jerseyNumber ?? ''}
                      disabled={savingPlayerId !== null}
                      onChange={(e) => { void handleJerseyChange(row.playerId, e.target.value); }}
                    >
                      <option value="">{t('teamLeader.noNumber', '—')}</option>
                      {JERSEY_OPTIONS.map((num) => (
                        <option key={num} value={num}>{num}</option>
                      ))}
                    </select>
                    {savingPlayerId === row.playerId && (
                      <span className="team-leader-jersey-status">{t('teamLeader.saving', 'Saving...')}</span>
                    )}
                    {savedPlayerId === row.playerId && (
                      <span className="team-leader-jersey-status team-leader-jersey-status--saved">
                        {t('teamLeader.saved', 'Saved')}
                      </span>
                    )}
                    {rowErrors[row.playerId] && (
                      <div className="team-leader-jersey-row-error">{rowErrors[row.playerId]}</div>
                    )}
                  </td>
                  <td>{row.playerName}</td>
                  <td>{t(`positions.${row.position}`, row.position)}</td>
                  <td>
                    {row.isActive
                      ? t('teamLeader.active', 'Active')
                      : t('teamLeader.inactive', 'Inactive')}
                  </td>
                </tr>
              ))}
              {roster.length === 0 && (
                <tr>
                  <td colSpan={4} className="team-leader-roster-empty">
                    {t('teamLeader.emptyRoster', 'This team has no players on its roster yet.')}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </TeamLeaderPageTemplate>
  );
}

export default TeamLeaderRosterPage;
