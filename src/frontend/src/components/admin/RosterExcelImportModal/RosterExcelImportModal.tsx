import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
  type DragEvent,
  type RefObject,
} from 'react';
import { useTranslation } from 'react-i18next';
import type {
  SeasonImportCallbacks,
  SeasonImportCreatedRecord,
  SeasonImportError,
  SeasonImportStep,
  SeasonImportSummary,
} from '../../../types/common/seasonImportTypes';
import {
  matchRosterImport,
  parseRosterWorkbook,
  type ParsedRosterPlayer,
  type ParsedRosterWorkbook,
  type RosterAssignment,
  type RosterImportMode,
  type RosterImportPreview,
  type RosterTeamOption,
} from '../../../api/common/rosterExcelParser';
import '../SeasonJsonImportModal/SeasonJsonImportModal.scss';
import './RosterExcelImportModal.scss';

const I18N = 'common.rosterImport';

export interface RosterImportLock {
  competitionId: string;
  competitionName: string;
  teamId: string;
  teamName: string;
}

export interface RosterExcelImportModalProps {
  onClose: () => void;
  onImported: () => void;
  loadSeasons: () => Promise<RosterTeamOption[]>;
  loadSeasonTeams: (seasonId: string) => Promise<RosterTeamOption[]>;
  importRosters: (
    competitionId: string,
    assignments: readonly RosterAssignment[],
    callbacks: SeasonImportCallbacks,
  ) => Promise<SeasonImportSummary>;
  revertRosters: (
    competitionId: string,
    records: SeasonImportCreatedRecord[],
    callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
  ) => Promise<{ deleted: number; failed: number }>;
  lockedSelection?: RosterImportLock | null;
  preferredTeamId?: string;
  preferredTeamName?: string;
  presetSeasonId?: string;
}

type RunState =
  | { kind: 'idle' }
  | { kind: 'running' }
  | { kind: 'success'; summary: SeasonImportSummary }
  | { kind: 'failed'; summary: SeasonImportSummary; fatalMessage: string }
  | { kind: 'reverting' }
  | { kind: 'reverted'; deleted: number; failed: number };

interface LogLine {
  text: string;
  status: 'created' | 'existing' | 'skipped' | 'info' | 'error';
}

export function RosterExcelImportModal({
  onClose,
  onImported,
  loadSeasons,
  loadSeasonTeams,
  importRosters,
  revertRosters,
  lockedSelection = null,
  preferredTeamId,
  preferredTeamName,
  presetSeasonId,
}: RosterExcelImportModalProps) {
  const { t } = useTranslation();
  const [externalLock, setExternalLock] = useState<RosterImportLock | null>(null);
  const [seasons, setSeasons] = useState<RosterTeamOption[]>([]);
  const [teams, setTeams] = useState<RosterTeamOption[]>([]);
  const [seasonsLoading, setSeasonsLoading] = useState(!lockedSelection);
  const [teamsLoading, setTeamsLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [seasonId, setSeasonId] = useState(lockedSelection?.competitionId ?? '');
  const [mode, setMode] = useState<RosterImportMode>('single');
  const [selectedIds, setSelectedIds] = useState<string[]>(
    lockedSelection ? [lockedSelection.teamId] : [],
  );
  const [parsed, setParsed] = useState<ParsedRosterWorkbook | null>(null);
  const [fileName, setFileName] = useState<string | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [runState, setRunState] = useState<RunState>({ kind: 'idle' });
  const [log, setLog] = useState<LogLine[]>([]);
  const [progress, setProgress] = useState({ done: 0, total: 0 });
  const [autoRevert, setAutoRevert] = useState(false);
  const abortRef = useRef(false);
  const autoRevertRef = useRef(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    if (lockedSelection) return;
    let cancelled = false;
    setSeasonsLoading(true);
    void loadSeasons()
      .then((loaded) => {
        if (cancelled) return;
        setSeasons(loaded);
        if (presetSeasonId && loaded.some((season) => season.id === presetSeasonId)) {
          setSeasonId(presetSeasonId);
        } else if (presetSeasonId && preferredTeamId && preferredTeamName) {
          setExternalLock({
            competitionId: presetSeasonId,
            competitionName: t(`${I18N}.selectedCompetition`, 'Selected competition'),
            teamId: preferredTeamId,
            teamName: preferredTeamName,
          });
        }
        setLoadError(null);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        const message = err instanceof Error ? err.message : String(err);
        setLoadError(t(`${I18N}.loadFailed`, 'Could not load seasons: {{msg}}', { msg: message }));
      })
      .finally(() => {
        if (!cancelled) setSeasonsLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [loadSeasons, lockedSelection, preferredTeamId, preferredTeamName, presetSeasonId, t]);

  useEffect(() => {
    if (lockedSelection || seasonId.length === 0) {
      if (!lockedSelection) setTeams([]);
      return;
    }
    let cancelled = false;
    setTeamsLoading(true);
    void loadSeasonTeams(seasonId)
      .then((loaded) => {
        if (cancelled) return;
        setTeams(loaded);
        setSelectedIds((current) => {
          const stillThere = current.filter((id) => loaded.some((team) => team.id === id));
          if (stillThere.length > 0) return stillThere;
          if (preferredTeamId && loaded.some((team) => team.id === preferredTeamId)) {
            return [preferredTeamId];
          }
          return [];
        });
        setLoadError(null);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        const message = err instanceof Error ? err.message : String(err);
        setLoadError(t(`${I18N}.teamsFailed`, 'Could not load teams: {{msg}}', { msg: message }));
        setTeams([]);
      })
      .finally(() => {
        if (!cancelled) setTeamsLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [loadSeasonTeams, lockedSelection, preferredTeamId, seasonId, t]);

  const activeLock = lockedSelection ?? externalLock;

  const selectedTeams = useMemo((): RosterTeamOption[] => {
    if (activeLock) {
      return [{ id: activeLock.teamId, name: activeLock.teamName }];
    }
    return teams.filter((team) => selectedIds.includes(team.id));
  }, [activeLock, selectedIds, teams]);

  const effectiveMode: RosterImportMode = activeLock ? 'single' : mode;

  const preview = useMemo((): RosterImportPreview | null => {
    if (!parsed) return null;
    return matchRosterImport(parsed, selectedTeams, effectiveMode);
  }, [effectiveMode, parsed, selectedTeams]);

  const competitionId = activeLock?.competitionId ?? seasonId;

  const appendLog = useCallback((line: LogLine) => {
    setLog((prev) => [...prev, line]);
  }, []);

  const readFile = useCallback(
    async (file: File) => {
      setFileError(null);
      if (!file.name.toLowerCase().endsWith('.xlsx')) {
        setParsed(null);
        setFileName(null);
        setFileError(t(`${I18N}.xlsxOnly`, 'Choose an .xlsx file.'));
        return;
      }
      try {
        const buffer = await file.arrayBuffer();
        const workbook = await parseRosterWorkbook(buffer);
        setParsed(workbook);
        setFileName(file.name);
      } catch (err) {
        const message = err instanceof Error ? err.message : String(err);
        setParsed(null);
        setFileName(null);
        setFileError(t(`${I18N}.fileFailed`, 'Could not read the Excel file: {{msg}}', { msg: message }));
      }
    },
    [t],
  );

  const onFileChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];
      if (file) void readFile(file);
    },
    [readFile],
  );

  const onDrop = useCallback(
    (event: DragEvent<HTMLLabelElement>) => {
      event.preventDefault();
      const file = event.dataTransfer.files[0];
      if (file) void readFile(file);
    },
    [readFile],
  );

  const toggleTeam = useCallback(
    (teamId: string) => {
      setSelectedIds((current) => {
        if (effectiveMode === 'single') return [teamId];
        return current.includes(teamId) ? current.filter((id) => id !== teamId) : [...current, teamId];
      });
    },
    [effectiveMode],
  );

  const runRevert = useCallback(
    async (records: SeasonImportCreatedRecord[]) => {
      setRunState({ kind: 'reverting' });
      appendLog({ text: t(`${I18N}.revertStart`, 'Reverting created records...'), status: 'info' });
      const { deleted, failed } = await revertRosters(competitionId, records, {
        onStep: (step) => {
          appendLog({ text: step.label, status: step.status });
          setProgress({ done: step.index + 1, total: step.total });
        },
        onError: (err) => {
          appendLog({ text: `${err.label}: ${err.message}`, status: 'error' });
        },
      });
      setRunState({ kind: 'reverted', deleted, failed });
      onImported();
    },
    [appendLog, competitionId, onImported, revertRosters, t],
  );

  const startImport = useCallback(async () => {
    if (!preview?.canImport || competitionId.length === 0) return;
    abortRef.current = false;
    autoRevertRef.current = autoRevert;
    setLog([]);
    setProgress({ done: 0, total: 0 });
    setRunState({ kind: 'running' });
    const callbacks: SeasonImportCallbacks = {
      onStep: (step: SeasonImportStep) => {
        appendLog({ text: step.label, status: step.status });
        setProgress({ done: step.index + 1, total: step.total });
      },
      onError: (err: SeasonImportError) => {
        appendLog({ text: `${err.label}: ${err.message}`, status: 'error' });
      },
      shouldAbort: () => abortRef.current,
    };
    try {
      const summary = await importRosters(competitionId, preview.assignments, callbacks);
      if (summary.fatal || summary.aborted) {
        const fatal = summary.errors.find((err) => err.fatal);
        setRunState({
          kind: 'failed',
          summary,
          fatalMessage: fatal?.message ?? t(`${I18N}.failed`, 'Import failed'),
        });
        if (autoRevertRef.current && summary.created.length > 0 && !summary.aborted) {
          await runRevert(summary.created);
        }
        return;
      }
      setRunState({ kind: 'success', summary });
      onImported();
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      setRunState({
        kind: 'failed',
        summary: {
          clubsCreated: 0,
          clubsExisting: 0,
          divisionsCreated: 0,
          divisionsExisting: 0,
          teamsCreated: 0,
          teamsExisting: 0,
          personsCreated: 0,
          personsExisting: 0,
          playersCreated: 0,
          playersExisting: 0,
          teamPlayerAssignments: 0,
          seasonId: null,
          seasonName: null,
          seasonAssignments: 0,
          matchesCreated: 0,
          errors: [],
          created: [],
          fatal: true,
          aborted: false,
        },
        fatalMessage: message,
      });
    }
  }, [appendLog, autoRevert, competitionId, importRosters, onImported, preview, runRevert, t]);

  const busy = runState.kind === 'running' || runState.kind === 'reverting';

  return (
    <div className="modal-overlay">
      <div className="modal-content import-modal roster-import-modal">
        <div className="modal-header">
          <h3>{t(`${I18N}.title`, 'Import roster from Excel')}</h3>
          <button
            type="button"
            className="modal-close-btn"
            onClick={busy ? undefined : onClose}
            disabled={busy}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>
        <div className="modal-body import-modal__body">
          {runState.kind === 'idle' && (
            <SetupView
              lockedSelection={activeLock}
              seasons={seasons}
              teams={teams}
              seasonsLoading={seasonsLoading}
              teamsLoading={teamsLoading}
              loadError={loadError}
              seasonId={seasonId}
              mode={effectiveMode}
              selectedIds={selectedIds}
              fileName={fileName}
              fileError={fileError}
              preview={preview}
              fileInputRef={fileInputRef}
              onSeasonChange={(value) => {
                setSeasonId(value);
                setSelectedIds([]);
                setParsed(null);
                setFileName(null);
              }}
              onModeChange={(value) => {
                setMode(value);
                setSelectedIds((current) => (value === 'single' ? current.slice(0, 1) : current));
              }}
              onToggleTeam={toggleTeam}
              onFileChange={onFileChange}
              onDrop={onDrop}
            />
          )}
          {runState.kind !== 'idle' && (
            <div className="import-modal__log" role="log">
              <p>
                {t(`${I18N}.progress`, '{{done}} / {{total}}', {
                  done: progress.done,
                  total: progress.total,
                })}
              </p>
              {log.map((line, index) => (
                <p key={`${index}-${line.text}`} className={`import-modal__log-line import-modal__log-line--${line.status}`}>
                  {line.text}
                </p>
              ))}
              {runState.kind === 'success' && <SummaryView summary={runState.summary} />}
              {runState.kind === 'failed' && (
                <p className="import-modal__note">{runState.fatalMessage}</p>
              )}
              {runState.kind === 'reverted' && (
                <p className="import-modal__note">
                  {t(`${I18N}.reverted`, 'Reverted {{deleted}} records, {{failed}} failed.', {
                    deleted: runState.deleted,
                    failed: runState.failed,
                  })}
                </p>
              )}
            </div>
          )}
        </div>
        <div className="modal-footer">
          {runState.kind === 'idle' && (
            <>
              <label className="roster-import-modal__auto-revert">
                <input
                  type="checkbox"
                  checked={autoRevert}
                  onChange={(event) => setAutoRevert(event.target.checked)}
                />
                {t(`${I18N}.autoRevert`, 'Revert automatically if the import stops on a fatal error')}
              </label>
              <button type="button" className="btn btn-secondary" onClick={onClose}>
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="button"
                className="btn btn-primary"
                disabled={!preview?.canImport || competitionId.length === 0}
                onClick={() => void startImport()}
              >
                {t(`${I18N}.start`, 'Import roster')}
              </button>
            </>
          )}
          {runState.kind === 'running' && (
            <button type="button" className="btn btn-secondary" onClick={() => { abortRef.current = true; }}>
              {t(`${I18N}.abort`, 'Stop')}
            </button>
          )}
          {runState.kind === 'failed' && (
            <>
              <button type="button" className="btn btn-secondary" onClick={onClose}>
                {t('common.close', 'Close')}
              </button>
              {runState.summary.created.length > 0 && (
                <button
                  type="button"
                  className="btn btn-danger"
                  onClick={() => void runRevert(runState.summary.created)}
                >
                  {t(`${I18N}.revert`, 'Revert created')}
                </button>
              )}
            </>
          )}
          {(runState.kind === 'success' || runState.kind === 'reverted') && (
            <button type="button" className="btn btn-secondary" onClick={onClose}>
              {t('common.close', 'Close')}
            </button>
          )}
          {runState.kind === 'reverting' && (
            <button type="button" className="btn btn-secondary" disabled>
              {t(`${I18N}.reverting`, 'Reverting...')}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

interface SetupViewProps {
  lockedSelection: RosterImportLock | null;
  seasons: RosterTeamOption[];
  teams: RosterTeamOption[];
  seasonsLoading: boolean;
  teamsLoading: boolean;
  loadError: string | null;
  seasonId: string;
  mode: RosterImportMode;
  selectedIds: string[];
  fileName: string | null;
  fileError: string | null;
  preview: RosterImportPreview | null;
  fileInputRef: RefObject<HTMLInputElement | null>;
  onSeasonChange: (seasonId: string) => void;
  onModeChange: (mode: RosterImportMode) => void;
  onToggleTeam: (teamId: string) => void;
  onFileChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onDrop: (event: DragEvent<HTMLLabelElement>) => void;
}

function SetupView({
  lockedSelection,
  seasons,
  teams,
  seasonsLoading,
  teamsLoading,
  loadError,
  seasonId,
  mode,
  selectedIds,
  fileName,
  fileError,
  preview,
  fileInputRef,
  onSeasonChange,
  onModeChange,
  onToggleTeam,
  onFileChange,
  onDrop,
}: SetupViewProps) {
  const { t } = useTranslation();
  const canPickFile = lockedSelection !== null || (seasonId.length > 0 && selectedIds.length > 0);

  return (
    <div className="roster-import-modal__setup">
      {lockedSelection ? (
        <p className="import-modal__note">
          {t(`${I18N}.lockedIntro`, 'Importing the roster of {{team}} into {{season}}.', {
            team: lockedSelection.teamName,
            season: lockedSelection.competitionName,
          })}
        </p>
      ) : (
        <>
          <label className="import-modal__field">
            <span>{t(`${I18N}.season`, 'Season')}</span>
            <select
              value={seasonId}
              disabled={seasonsLoading}
              onChange={(event) => onSeasonChange(event.target.value)}
            >
              <option value="">{t(`${I18N}.seasonPlaceholder`, 'Select a season')}</option>
              {seasons.map((season) => (
                <option key={season.id} value={season.id}>
                  {season.name}
                </option>
              ))}
            </select>
          </label>
          <fieldset className="roster-import-modal__mode" disabled={seasonId.length === 0}>
            <legend>{t(`${I18N}.teams`, 'Teams')}</legend>
            <label>
              <input
                type="radio"
                name="roster-import-mode"
                checked={mode === 'single'}
                onChange={() => onModeChange('single')}
              />
              {t(`${I18N}.modeSingle`, 'One team')}
            </label>
            <label>
              <input
                type="radio"
                name="roster-import-mode"
                checked={mode === 'multiple'}
                onChange={() => onModeChange('multiple')}
              />
              {t(`${I18N}.modeMultiple`, 'Several teams')}
            </label>
          </fieldset>
          {mode === 'multiple' && (
            <p className="import-modal__note">{t(`${I18N}.multiReminder`)}</p>
          )}
          {teamsLoading && <p>{t('common.loading', 'Loading...')}</p>}
          {!teamsLoading && seasonId.length > 0 && teams.length === 0 && (
            <p className="import-modal__note">{t(`${I18N}.noTeams`, 'This season has no teams.')}</p>
          )}
          {teams.length > 0 && (
            <ul className="roster-import-modal__teams">
              {teams.map((team) => (
                <li key={team.id}>
                  <label>
                    <input
                      type={mode === 'single' ? 'radio' : 'checkbox'}
                      name="roster-import-team"
                      checked={selectedIds.includes(team.id)}
                      onChange={() => onToggleTeam(team.id)}
                    />
                    {team.name}
                  </label>
                </li>
              ))}
            </ul>
          )}
        </>
      )}
      {loadError && <p className="import-modal__note">{loadError}</p>}
      <label
        className="import-modal__dropzone"
        onDragOver={(event) => event.preventDefault()}
        onDrop={(event) => {
          if (!canPickFile) {
            event.preventDefault();
            return;
          }
          onDrop(event);
        }}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
          onChange={onFileChange}
          disabled={!canPickFile}
        />
        <span>
          {fileName ?? t(`${I18N}.dropzone`, 'Drop an Excel file here or choose a file.')}
        </span>
        <span className="import-modal__choose-btn">{t(`${I18N}.chooseFile`, 'Choose file')}</span>
      </label>
      {!canPickFile && (
        <p className="import-modal__note">
          {t(`${I18N}.selectFirst`, 'Select the season and at least one team before choosing the file.')}
        </p>
      )}
      {fileError && <p className="import-modal__note">{fileError}</p>}
      {preview && <PreviewView preview={preview} />}
    </div>
  );
}

function PreviewView({ preview }: { preview: RosterImportPreview }) {
  const { t } = useTranslation();
  return (
    <section className="import-modal__preview">
      <h4>{t(`${I18N}.previewHeading`, 'Check before import')}</h4>
      <p className="import-modal__note">{t(`${I18N}.ignoredMeta`)}</p>
      {preview.assignments.map((assignment) => (
        <div key={assignment.teamId}>
          <p>
            {t(`${I18N}.teamLine`, '{{name}} · {{count}} players', {
              name: assignment.teamName,
              count: assignment.block.players.length,
            })}
          </p>
          {assignment.block.coachName && (
            <p className="import-modal__checklist-meta">
              {t(`${I18N}.coach`, 'Coach: {{name}}', { name: assignment.block.coachName })}
            </p>
          )}
          {assignment.block.jerseyColor && (
            <p className="import-modal__checklist-meta">
              {t(`${I18N}.jerseyColor`, 'Jersey colour: {{color}}', { color: assignment.block.jerseyColor })}
            </p>
          )}
          <ul>
            {assignment.block.players.map((player) => (
              <li key={`${assignment.teamId}-${player.rowNumber}-${player.rawName}`}>
                <PlayerLine player={player} />
              </li>
            ))}
          </ul>
        </div>
      ))}
      {preview.missingTeams.length > 0 && (
        <p className="import-modal__note">
          {t(`${I18N}.missingTeams`, 'Missing from the file: {{names}}', {
            names: preview.missingTeams.map((team) => team.name).join(', '),
          })}
        </p>
      )}
      {preview.extraFileTeams.length > 0 && (
        <p className="import-modal__note">
          {t(`${I18N}.extraTeams`, 'Extra teams in the file will not be imported: {{names}}', {
            names: preview.extraFileTeams.join(', '),
          })}
        </p>
      )}
      {preview.duplicateFileTeams.length > 0 && (
        <p className="import-modal__note">
          {t(`${I18N}.duplicateNames`, 'The file contains the same team more than once: {{names}}', {
            names: preview.duplicateFileTeams.join(', '),
          })}
        </p>
      )}
      {preview.unnamedPlayerCount > 0 && (
        <p className="import-modal__note">{t(`${I18N}.unnamedPlayers`)}</p>
      )}
      {preview.invalidNames.length > 0 && (
        <p className="import-modal__note">
          {t(`${I18N}.invalidNames`, 'A first and last name are required: {{names}}', {
            names: preview.invalidNames.join(', '),
          })}
        </p>
      )}
      {preview.canImport && (
        <p className="import-modal__note import-modal__note--ok">{t(`${I18N}.canImportHint`)}</p>
      )}
    </section>
  );
}

function PlayerLine({ player }: { player: ParsedRosterPlayer }) {
  const { t } = useTranslation();
  const marks: string[] = [];
  if (player.isGoalkeeper) marks.push(t(`${I18N}.goalkeeper`, 'goalkeeper'));
  if (typeof player.jerseyNumber === 'number') {
    marks.push(t(`${I18N}.jersey`, 'no. {{number}}', { number: player.jerseyNumber }));
  }
  if (player.isCaptain) marks.push(t(`${I18N}.captain`, 'captain'));
  if (player.isReferee) marks.push(t(`${I18N}.referee`, 'referee'));
  if (player.invalidName) marks.push(t(`${I18N}.invalidNameMark`, 'name incomplete'));
  const suffix = marks.length > 0 ? ` (${marks.join(', ')})` : '';
  return (
    <>
      {player.rawName}
      {suffix}
    </>
  );
}

function SummaryView({ summary }: { summary: SeasonImportSummary }) {
  const { t } = useTranslation();
  return (
    <div className="import-modal__summary">
      <p>
        {t(`${I18N}.summary`, 'Added {{added}} players, skipped {{skipped}}, new persons {{persons}}.', {
          added: summary.teamPlayerAssignments,
          skipped: summary.playersExisting,
          persons: summary.personsCreated,
        })}
      </p>
      {summary.errors.map((err, index) => (
        <p key={`${err.label}-${index}`} className="import-modal__note">
          {err.label}: {err.message}
        </p>
      ))}
    </div>
  );
}

export default RosterExcelImportModal;
