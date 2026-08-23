import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { hockeyPlayerService } from '../../../../api/hockey/hockeyPlayerService';
import {
  HOCKEY_LINE_TYPES,
  type HockeyLineType,
  type HockeyTeamDto,
  type HockeyTeamPlayerDto,
} from '../../../../types/hockey/hockeyTypes';
import { loadPersonNameMap } from '../../../../utils/hockeyLookups';
import './EditRosterPage.scss';

function EditHockeyLinesPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  const [team, setTeam] = useState<HockeyTeamDto | null>(null);
  const [playerNames, setPlayerNames] = useState<Map<string, string>>(new Map());
  const [lineName, setLineName] = useState('');
  const [lineNumber, setLineNumber] = useState(1);
  const [lineType, setLineType] = useState<HockeyLineType>('ForwardLine');
  const [selectedLineId, setSelectedLineId] = useState('');
  const [teamPlayerId, setTeamPlayerId] = useState('');
  const [slot, setSlot] = useState('');
  const [order, setOrder] = useState(1);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async (): Promise<void> => {
    if (!teamId) {
      return;
    }
    try {
      setLoading(true);
      const loaded = await hockeyTeamService.getById(teamId);
      setTeam(loaded);
      if (!selectedLineId && loaded.lines.length > 0) {
        setSelectedLineId(loaded.lines[0].id);
      }
      const people = await Promise.all(
        loaded.roster.map(async (row) => {
          try {
            const player = await hockeyPlayerService.getById(row.playerId);
            const names = await loadPersonNameMap([player.personId]);
            return [row.playerId, names.get(player.personId) ?? row.playerId.slice(0, 8)] as const;
          } catch {
            return [row.playerId, row.playerId.slice(0, 8)] as const;
          }
        }),
      );
      setPlayerNames(new Map(people));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load lines');
    } finally {
      setLoading(false);
    }
  }, [teamId, selectedLineId]);

  useEffect(() => {
    void load();
  }, [load]);

  const activeRoster = useMemo(
    () => (team?.roster ?? []).filter((row) => row.isActive),
    [team],
  );
  const selectedLine = team?.lines.find((line) => line.id === selectedLineId);

  const playerLabel = (row: HockeyTeamPlayerDto): string => {
    const name = playerNames.get(row.playerId) ?? row.playerId.slice(0, 8);
    return `#${row.jerseyNumber ?? '—'} ${name}`;
  };

  const run = async (operation: () => Promise<unknown>): Promise<void> => {
    setSaving(true);
    setError(null);
    try {
      await operation();
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Operation failed');
    } finally {
      setSaving(false);
    }
  };

  return (
    <PageTemplate title={t('hockey.teams.lines', 'Edit lines')}>
      <div className="edit-roster-container">
        <h2 className="edit-roster-title">{t('hockey.teams.lines', 'Edit lines')}</h2>
        <ErrorPopup message={error} />
        {loading || !team ? (
          <p>{t('common.loading', 'Loading...')}</p>
        ) : (
          <>
            <div className="create-team-form">
              <h3>{t('hockey.teams.createLine', 'Create line')}</h3>
              <div className="form-row">
                <input
                  placeholder={t('hockey.teams.lineName', 'Line name')}
                  value={lineName}
                  onChange={(event) => setLineName(event.target.value)}
                />
                <input
                  type="number"
                  min={1}
                  value={lineNumber}
                  onChange={(event) => setLineNumber(Number(event.target.value))}
                />
                <select value={lineType} onChange={(event) => setLineType(event.target.value as HockeyLineType)}>
                  {HOCKEY_LINE_TYPES.map((item) => (
                    <option key={item} value={item}>{item}</option>
                  ))}
                </select>
                <button
                  type="button"
                  disabled={saving || !lineName.trim()}
                  onClick={() => void run(() => hockeyTeamService.addLine(team.id, {
                    name: lineName.trim(),
                    lineNumber,
                    lineType,
                  }).then(() => {
                    setLineName('');
                    setLineNumber((prev) => prev + 1);
                  }))}
                >
                  {t('common.add', 'Add')}
                </button>
              </div>
            </div>

            <div className="create-team-form" style={{ marginTop: '1rem' }}>
              <h3>{t('hockey.teams.manageLines', 'Manage lines')}</h3>
              {team.lines.length === 0 ? (
                <p>{t('hockey.teams.noLines', 'No lines created yet')}</p>
              ) : (
                <>
                  <ul>
                    {team.lines.map((line) => (
                      <li key={line.id}>
                        <button type="button" onClick={() => setSelectedLineId(line.id)}>
                          {line.name} · {line.lineType} · #{line.lineNumber}
                        </button>
                        <button
                          type="button"
                          disabled={saving}
                          onClick={() => void run(() => hockeyTeamService.removeLine(team.id, line.id).then(() => {
                            if (selectedLineId === line.id) {
                              setSelectedLineId('');
                            }
                          }))}
                        >
                          {t('common.delete', 'Delete')}
                        </button>
                      </li>
                    ))}
                  </ul>
                  {selectedLine && (
                    <div style={{ marginTop: '1rem' }}>
                      <h4>{selectedLine.name}</h4>
                      <ul>
                        {selectedLine.players.map((linePlayer) => {
                          const rosterRow = team.roster.find((row) => row.id === linePlayer.teamPlayerId);
                          return (
                            <li key={linePlayer.id}>
                              {rosterRow ? playerLabel(rosterRow) : linePlayer.teamPlayerId.slice(0, 8)} · {linePlayer.slot}
                              <button
                                type="button"
                                disabled={saving}
                                onClick={() => void run(() => hockeyTeamService.removePlayerFromLine(team.id, selectedLine.id, linePlayer.teamPlayerId))}
                              >
                                {t('common.remove', 'Remove')}
                              </button>
                            </li>
                          );
                        })}
                      </ul>
                      <div className="form-row">
                        <select value={teamPlayerId} onChange={(event) => setTeamPlayerId(event.target.value)}>
                          <option value="">{t('hockey.roster.player', 'Player')}</option>
                          {activeRoster
                            .filter((row) => !selectedLine.players.some((linePlayer) => linePlayer.teamPlayerId === row.id))
                            .map((row) => (
                              <option key={row.id} value={row.id}>{playerLabel(row)}</option>
                            ))}
                        </select>
                        <input
                          placeholder={t('hockey.teams.slot', 'Slot')}
                          value={slot}
                          onChange={(event) => setSlot(event.target.value)}
                        />
                        <input
                          type="number"
                          min={1}
                          value={order}
                          onChange={(event) => setOrder(Number(event.target.value))}
                        />
                        <button
                          type="button"
                          disabled={saving || !teamPlayerId || !slot.trim()}
                          onClick={() => void run(() => hockeyTeamService.addPlayerToLine(team.id, selectedLine.id, {
                            teamPlayerId,
                            slot: slot.trim(),
                            order,
                          }).then(() => {
                            setTeamPlayerId('');
                            setSlot('');
                            setOrder((prev) => prev + 1);
                          }))}
                        >
                          {t('common.add', 'Add')}
                        </button>
                      </div>
                    </div>
                  )}
                </>
              )}
            </div>

            <div className="form-actions" style={{ marginTop: '1rem' }}>
              <button type="button" className="cancel-button" onClick={() => navigate(`/admin/hockey/teams/${team.id}/roster`)}>
                {t('common.back', 'Back')}
              </button>
            </div>
          </>
        )}
      </div>
    </PageTemplate>
  );
}

export default EditHockeyLinesPage;
