import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ClubAdminPageTemplate from '../components/ClubAdminPageTemplate';
import SearchField from '../../../components/SearchField';
import { clubAdminService } from '../../../api/clubAdmin/clubAdminService';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { footballTeamService } from '../../../api/football/footballTeamService';
import { hockeyTeamService } from '../../../api/hockey/hockeyTeamService';
import type { ClubAdminSport } from '../../../types/clubAdmin/clubAdminTypes';
import { loadHockeyRosterNameMaps } from '../../../utils/hockeyLookups';
import './ClubAdminRosterPage.scss';

interface RosterRow {
  playerId: string;
  playerName: string;
  position: string;
  isActive: boolean;
  jerseyNumber: number | null;
}

const JERSEY_OPTIONS: number[] = Array.from({ length: 99 }, (_, i) => i + 1);

function ClubAdminRosterPage() {
  const { t } = useTranslation();
  const { sport, teamId } = useParams<{ sport: ClubAdminSport; teamId: string }>();

  const [teamName, setTeamName] = useState('');
  const [roster, setRoster] = useState<RosterRow[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [savingPlayerId, setSavingPlayerId] = useState<string | null>(null);
  const [rowErrors, setRowErrors] = useState<Record<string, string>>({});
  const [savedPlayerId, setSavedPlayerId] = useState<string | null>(null);

  const loadTeam = useCallback(async () => {
    if (!sport || !teamId) return;
    try {
      if (sport === 'hockey') {
        const team = await hockeyTeamService.getById(teamId);
        const names = await loadHockeyRosterNameMaps([team]);
        setTeamName(team.name);
        setRoster(team.roster.map((player) => ({
          playerId: player.playerId,
          playerName: names.byPlayerId.get(player.playerId) ?? player.playerId.slice(0, 8),
          position: player.position,
          isActive: player.isActive,
          jerseyNumber: player.jerseyNumber ?? null,
        })));
        return;
      }

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
      setError(err instanceof Error ? err.message : t('clubAdmin.loadRosterError', 'Failed to load the roster'));
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
      await clubAdminService.updateJerseyNumber(sport, teamId, playerId, newNumber);
      setRoster((prev) => prev.map((row) => (
        row.playerId === playerId ? { ...row, jerseyNumber: newNumber } : row
      )));
      setSavedPlayerId(playerId);
    } catch (err: unknown) {
      setRowErrors((prev) => ({
        ...prev,
        [playerId]: err instanceof Error ? err.message : t('clubAdmin.jerseyUpdateError', 'Failed to update the jersey number'),
      }));
    } finally {
      setSavingPlayerId(null);
    }
  };

  const filteredRoster = useMemo(() => {
    const needle = searchQuery.trim().toLowerCase();
    if (!needle) return roster;
    return roster.filter((row) => {
      const jersey = row.jerseyNumber != null ? String(row.jerseyNumber) : '';
      const haystack = `${row.playerName} ${jersey} #${jersey}`.toLowerCase();
      return haystack.includes(needle);
    });
  }, [roster, searchQuery]);

  return (
    <ClubAdminPageTemplate title={teamName ? `${teamName} – ${t('clubAdmin.rosterTitle', 'Roster')}` : t('clubAdmin.rosterTitle', 'Roster')}>
      <Link to="/club-admin" className="club-admin-back-link">
        ← {t('clubAdmin.backToClubs', 'Back to my clubs')}
      </Link>

      {isLoading && <div className="club-admin-loading">{t('common.loading', 'Loading...')}</div>}
      {error && <div className="club-admin-error">{error}</div>}

      {!isLoading && !error && (
        <div className="club-admin-roster-card">
          <p className="club-admin-roster-hint">
            {t('clubAdmin.rosterHint', 'You can change the jersey numbers of your players. Each number can only be used once per team.')}
          </p>
          <div className="club-admin-roster-toolbar">
            <SearchField
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder={t('clubAdmin.searchPlayers', 'Search by name or jersey number...')}
              rounded="md"
              size="sm"
            />
            <span className="club-admin-roster-count">
              {t('clubAdmin.showingPlayers', '{{shown}} / {{total}} players', {
                shown: filteredRoster.length,
                total: roster.length,
              })}
            </span>
          </div>
          <div className="club-admin-roster-table-wrap">
            <table className="club-admin-roster-table">
              <thead>
                <tr>
                  <th className="club-admin-roster-jersey-col">{t('clubAdmin.jerseyNumber', 'Jersey #')}</th>
                  <th>{t('clubAdmin.playerName', 'Player')}</th>
                  <th>{t('clubAdmin.position', 'Position')}</th>
                  <th>{t('clubAdmin.status', 'Status')}</th>
                </tr>
              </thead>
              <tbody>
                {filteredRoster.map((row) => (
                  <tr key={row.playerId} className={row.isActive ? '' : 'club-admin-roster-row--inactive'}>
                    <td>
                      <select
                        className="club-admin-jersey-select"
                        value={row.jerseyNumber ?? ''}
                        disabled={savingPlayerId !== null}
                        onChange={(e) => { void handleJerseyChange(row.playerId, e.target.value); }}
                      >
                        <option value="">{t('clubAdmin.noNumber', '—')}</option>
                        {JERSEY_OPTIONS.map((num) => (
                          <option key={num} value={num}>{num}</option>
                        ))}
                      </select>
                      {savingPlayerId === row.playerId && (
                        <span className="club-admin-jersey-status">{t('clubAdmin.saving', 'Saving...')}</span>
                      )}
                      {savedPlayerId === row.playerId && (
                        <span className="club-admin-jersey-status club-admin-jersey-status--saved">
                          {t('clubAdmin.saved', 'Saved')}
                        </span>
                      )}
                      {rowErrors[row.playerId] && (
                        <div className="club-admin-jersey-row-error">{rowErrors[row.playerId]}</div>
                      )}
                    </td>
                    <td>{row.playerName}</td>
                    <td>
                      {sport === 'hockey'
                        ? t(`hockey.positions.${row.position}`, row.position)
                        : t(`positions.${row.position}`, row.position)}
                    </td>
                    <td>
                      {row.isActive
                        ? t('clubAdmin.active', 'Active')
                        : t('clubAdmin.inactive', 'Inactive')}
                    </td>
                  </tr>
                ))}
                {roster.length === 0 && (
                  <tr>
                    <td colSpan={4} className="club-admin-roster-empty">
                      {t('clubAdmin.emptyRoster', 'This team has no players on its roster yet.')}
                    </td>
                  </tr>
                )}
                {roster.length > 0 && filteredRoster.length === 0 && (
                  <tr>
                    <td colSpan={4} className="club-admin-roster-empty">
                      {t('clubAdmin.searchNoResults', 'No players match the current search.')}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </ClubAdminPageTemplate>
  );
}

export default ClubAdminRosterPage;
