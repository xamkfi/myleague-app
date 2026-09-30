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
  assignRosterDestinations,
  normalizeTeamName,
  suggestRosterDestinations,
  type ParsedRosterPlayer,
  type ParsedRosterWorkbook,
  type RosterAssignment,
  type RosterImportPreview,
  type RosterTeamOption,
} from '../../../api/common/rosterExcelParser';
import { parseRosterJson } from '../../../api/common/rosterJsonParser';
import '../SeasonJsonImportModal/SeasonJsonImportModal.scss';
import './RosterExcelImportModal.scss';

const I18N = 'common.rosterImport';

export interface RosterImportLock {
  competitionId: string;
  competitionName: string;
  teamId: string;
  teamName: string;
}

export interface RosterJsonGuide {
  prompt: string;
  buildPromptFileName: () => string;
  sampleHref: string;
  sampleDownloadName: string;
  allowedPositions: readonly string[];
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
  guide: RosterJsonGuide;
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
  presetSeasonId,
  guide,
}: RosterExcelImportModalProps) {
  const { t } = useTranslation();
  const suggestedTeamId = lockedSelection?.teamId ?? preferredTeamId ?? null;
  const [seasons, setSeasons] = useState<RosterTeamOption[]>([]);
  const [teams, setTeams] = useState<RosterTeamOption[]>([]);
  const [seasonsLoading, setSeasonsLoading] = useState(true);
  const [teamsLoading, setTeamsLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [seasonId, setSeasonId] = useState(lockedSelection?.competitionId ?? '');
  const [destinations, setDestinations] = useState<Record<number, string>>({});
  const [destinationsTouched, setDestinationsTouched] = useState(false);
  const [parsed, setParsed] = useState<ParsedRosterWorkbook | null>(null);
  const [fileName, setFileName] = useState<string | null>(null);
  const [fileErrors, setFileErrors] = useState<string[]>([]);
  const [promptCopyState, setPromptCopyState] = useState<null | 'copied' | 'downloaded' | 'error'>(null);
  const [runState, setRunState] = useState<RunState>({ kind: 'idle' });
  const [log, setLog] = useState<LogLine[]>([]);
  const [progress, setProgress] = useState({ done: 0, total: 0 });
  const [autoRevert, setAutoRevert] = useState(false);
  const abortRef = useRef(false);
  const autoRevertRef = useRef(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    const previous = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = previous;
    };
  }, []);

  useEffect(() => {
    let cancelled = false;
    setSeasonsLoading(true);
    void loadSeasons()
      .then((loaded) => {
        if (cancelled) return;
        setSeasons(loaded);
        const preferredSeasonId = lockedSelection?.competitionId ?? presetSeasonId;
        if (preferredSeasonId && loaded.some((season) => season.id === preferredSeasonId)) {
          setSeasonId(preferredSeasonId);
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
  }, [loadSeasons, lockedSelection, presetSeasonId, t]);

  useEffect(() => {
    if (seasonId.length === 0) {
      setTeams([]);
      return;
    }
    let cancelled = false;
    setTeamsLoading(true);
    void loadSeasonTeams(seasonId)
      .then((loaded) => {
        if (cancelled) return;
        setTeams(loaded);
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
  }, [loadSeasonTeams, seasonId, t]);

  useEffect(() => {
    if (!parsed || destinationsTouched) return;
    setDestinations(suggestRosterDestinations(parsed, teams, suggestedTeamId));
  }, [destinationsTouched, parsed, suggestedTeamId, teams]);

  const preview = useMemo((): RosterImportPreview | null => {
    if (!parsed) return null;
    return assignRosterDestinations(parsed, teams, destinations);
  }, [destinations, parsed, teams]);

  const competitionId = seasonId;

  const appendLog = useCallback((line: LogLine) => {
    setLog((prev) => [...prev, line]);
  }, []);

  const readFile = useCallback(
    async (file: File) => {
      setFileErrors([]);
      if (!file.name.toLowerCase().endsWith('.json')) {
        setParsed(null);
        setFileName(null);
        setDestinations({});
        setDestinationsTouched(false);
        setFileErrors([t(`${I18N}.jsonOnly`, 'Choose a .json file.')]);
        return;
      }
      try {
        const text = await file.text();
        const result = parseRosterJson(text, guide.allowedPositions);
        if (!result.ok) {
          setParsed(null);
          setFileName(file.name);
          setDestinations({});
          setDestinationsTouched(false);
          setFileErrors(result.errors);
          return;
        }
        setParsed(result.workbook);
        setFileName(file.name);
        setDestinationsTouched(false);
        setDestinations({});
      } catch (err) {
        const message = err instanceof Error ? err.message : String(err);
        setParsed(null);
        setFileName(null);
        setDestinations({});
        setDestinationsTouched(false);
        setFileErrors([t(`${I18N}.fileFailed`, 'Could not read the JSON file: {{msg}}', { msg: message })]);
      }
    },
    [guide.allowedPositions, t],
  );

  const downloadPrompt = useCallback((): boolean => {
    try {
      const blob = new Blob([guide.prompt], { type: 'text/plain;charset=utf-8' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = guide.buildPromptFileName();
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.setTimeout(() => URL.revokeObjectURL(url), 1000);
      return true;
    } catch {
      return false;
    }
  }, [guide]);

  const downloadPromptFile = useCallback(() => {
    setPromptCopyState(downloadPrompt() ? 'downloaded' : 'error');
  }, [downloadPrompt]);

  const copyPrompt = useCallback(async () => {
    try {
      await navigator.clipboard.writeText(guide.prompt);
      setPromptCopyState('copied');
    } catch {
      setPromptCopyState(downloadPrompt() ? 'downloaded' : 'error');
    }
  }, [downloadPrompt, guide.prompt]);

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

  const onDestinationChange = useCallback((index: number, teamId: string) => {
    setDestinationsTouched(true);
    setDestinations((current) => {
      const next = { ...current };
      if (teamId.length === 0) {
        delete next[index];
      } else {
        next[index] = teamId;
      }
      return next;
    });
  }, []);

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
      const nothingAdded = summary.teamPlayerAssignments === 0 && summary.errors.length > 0;
      if (summary.fatal || summary.aborted || nothingAdded) {
        const fatal = summary.errors.find((err) => err.fatal);
        const reasons = summary.errors.map((err) => `${err.label}: ${err.message}`).join(' ');
        setRunState({
          kind: 'failed',
          summary,
          fatalMessage: nothingAdded
            ? t(`${I18N}.nothingAdded`, 'No players were added. {{reasons}}', { reasons })
            : fatal?.message ?? t(`${I18N}.failed`, 'Import failed'),
        });
        if (summary.fatal && autoRevertRef.current && summary.created.length > 0 && !summary.aborted) {
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
    <div className="modal-overlay roster-import-overlay">
      <div className="modal-content import-modal roster-import-modal">
        <div className="modal-header">
          <h3>{t(`${I18N}.title`, 'Import roster from JSON')}</h3>
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
              parsed={parsed}
              seasons={seasons}
              teams={teams}
              seasonsLoading={seasonsLoading}
              teamsLoading={teamsLoading}
              loadError={loadError}
              seasonId={seasonId}
              destinations={destinations}
              fileName={fileName}
              fileErrors={fileErrors}
              preview={preview}
              guide={guide}
              promptCopyState={promptCopyState}
              onCopyPrompt={() => void copyPrompt()}
              onDownloadPrompt={downloadPromptFile}
              fileInputRef={fileInputRef}
              onSeasonChange={(value) => {
                setSeasonId(value);
                setDestinationsTouched(false);
                setDestinations({});
              }}
              onDestinationChange={onDestinationChange}
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
  parsed: ParsedRosterWorkbook | null;
  seasons: RosterTeamOption[];
  teams: RosterTeamOption[];
  seasonsLoading: boolean;
  teamsLoading: boolean;
  loadError: string | null;
  seasonId: string;
  destinations: Record<number, string>;
  fileName: string | null;
  fileErrors: string[];
  preview: RosterImportPreview | null;
  guide: RosterJsonGuide;
  promptCopyState: null | 'copied' | 'downloaded' | 'error';
  onCopyPrompt: () => void;
  onDownloadPrompt: () => void;
  fileInputRef: RefObject<HTMLInputElement | null>;
  onSeasonChange: (seasonId: string) => void;
  onDestinationChange: (index: number, teamId: string) => void;
  onFileChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onDrop: (event: DragEvent<HTMLLabelElement>) => void;
}

function SetupView({
  parsed,
  seasons,
  teams,
  seasonsLoading,
  teamsLoading,
  loadError,
  seasonId,
  destinations,
  fileName,
  fileErrors,
  preview,
  guide,
  promptCopyState,
  onCopyPrompt,
  onDownloadPrompt,
  fileInputRef,
  onSeasonChange,
  onDestinationChange,
  onFileChange,
  onDrop,
}: SetupViewProps) {
  const { t } = useTranslation();
  const fileReady = parsed !== null && fileErrors.length === 0;

  return (
    <div className="roster-import-modal__setup">
      <label
        className="import-modal__dropzone"
        onDragOver={(event) => event.preventDefault()}
        onDrop={onDrop}
      >
        <i className="fas fa-file-upload" aria-hidden="true"></i>
        <p>{fileName ?? t(`${I18N}.dropzone`, 'Drop a JSON file here or choose a file.')}</p>
        <span className="import-modal__choose-btn">
          <i className="fas fa-folder-open" aria-hidden="true"></i> {t(`${I18N}.chooseFile`, 'Choose file')}
        </span>
        <input
          ref={fileInputRef}
          type="file"
          accept="application/json,.json"
          onChange={onFileChange}
          className="import-modal__file-input"
        />
      </label>
      {fileErrors.map((error) => (
        <p key={error} className="import-modal__note">{error}</p>
      ))}
      {!fileReady && <p className="import-modal__note">{t(`${I18N}.fileShape`)}</p>}
      {fileReady && parsed && (
        <FileSummary parsed={parsed} />
      )}
      {fileReady && (
        <section className="roster-import-modal__destination">
          <h4>{t(`${I18N}.destinationHeading`, 'Where to import')}</h4>
          <p className="import-modal__note">{t(`${I18N}.destinationHint`)}</p>
          {loadError && <p className="import-modal__note">{loadError}</p>}
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
          {teamsLoading && <p>{t('common.loading', 'Loading...')}</p>}
          {!teamsLoading && seasonId.length > 0 && teams.length === 0 && (
            <p className="import-modal__note">{t(`${I18N}.noTeams`, 'This season has no teams.')}</p>
          )}
          {parsed && seasonId.length > 0 && teams.length > 0 && (
            <div className="roster-import-modal__map">
              <div className="roster-import-modal__map-head">
                <span>{t(`${I18N}.fileTeamColumn`, 'Team in the file')}</span>
                <span>{t(`${I18N}.seasonTeamColumn`, 'Import into')}</span>
              </div>
              {parsed.teams.map((block, index) => {
                const selectedId = destinations[index] ?? '';
                const selectedTeam = teams.find((team) => team.id === selectedId);
                const nameMatches = Boolean(
                  block.teamName
                  && selectedTeam
                  && normalizeTeamName(block.teamName) === normalizeTeamName(selectedTeam.name),
                );
                const sourceName = block.teamName
                  ?? t(`${I18N}.unnamedTeamShort`, 'Team name not in the file');
                return (
                  <div key={`${block.teamName ?? 'team'}-${index}`} className="roster-import-modal__map-row">
                    <div className="roster-import-modal__map-source">
                      <strong>{sourceName}</strong>
                      <span>
                        {t(`${I18N}.playerCountShort`, '{{count}} players', { count: block.players.length })}
                      </span>
                    </div>
                    <div className="roster-import-modal__map-target">
                      <select
                        className={selectedId.length === 0 ? 'is-skipped' : undefined}
                        aria-label={t(`${I18N}.importIntoNamed`, 'Import {{name}} into', { name: sourceName })}
                        value={selectedId}
                        onChange={(event) => onDestinationChange(index, event.target.value)}
                      >
                        <option value="">{t(`${I18N}.skipTeam`, 'Do not import')}</option>
                        {teams.map((team) => (
                          <option key={team.id} value={team.id}>{team.name}</option>
                        ))}
                      </select>
                      {nameMatches && (
                        <span className="roster-import-modal__match">{t(`${I18N}.nameMatches`)}</span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
          {preview && <PreviewView preview={preview} />}
        </section>
      )}
      <div className="import-modal__ai-help">
        <h4 className="import-modal__ai-help-title">
          {t(`${I18N}.aiHelp.title`, "Don't have a JSON file yet? Generate one with AI")}
        </h4>
        <ol className="import-modal__ai-help-steps">
          <li>{t(`${I18N}.aiHelp.step1`)}</li>
          <li>{t(`${I18N}.aiHelp.step2`)}</li>
          <li>{t(`${I18N}.aiHelp.step3`)}</li>
          <li>{t(`${I18N}.aiHelp.step4`)}</li>
        </ol>
        <div className="import-modal__ai-help-actions">
          <button type="button" className="import-modal__ai-help-btn import-modal__ai-help-btn--primary" onClick={onCopyPrompt}>
            {t(`${I18N}.aiHelp.copyPrompt`, 'Copy AI prompt to clipboard')}
          </button>
          <button type="button" className="import-modal__ai-help-btn import-modal__ai-help-btn--ghost" onClick={onDownloadPrompt}>
            {t(`${I18N}.aiHelp.downloadPrompt`, 'Download AI prompt as .txt')}
          </button>
        </div>
        {promptCopyState === 'copied' && (
          <p className="import-modal__ai-help-feedback import-modal__ai-help-feedback--ok" role="status">
            {t(`${I18N}.aiHelp.copiedToast`)}
          </p>
        )}
        {promptCopyState === 'downloaded' && (
          <p className="import-modal__ai-help-feedback import-modal__ai-help-feedback--info" role="status">
            {t(`${I18N}.aiHelp.downloadedToast`)}
          </p>
        )}
        {promptCopyState === 'error' && (
          <p className="import-modal__ai-help-feedback import-modal__ai-help-feedback--error" role="alert">
            {t(`${I18N}.aiHelp.errorToast`)}
          </p>
        )}
        <a className="import-modal__sample-link" href={guide.sampleHref} download={guide.sampleDownloadName}>
          {t(`${I18N}.downloadSample`, 'Download sample JSON')}
        </a>
      </div>
    </div>
  );
}

function FileSummary({ parsed }: { parsed: ParsedRosterWorkbook }) {
  const { t } = useTranslation();
  return (
    <section className="roster-import-modal__summary">
      <h4>{t(`${I18N}.fileSummary`, 'In the file')}</h4>
      {parsed.teams.length === 0 && (
        <p className="import-modal__note">{t(`${I18N}.noPlayersInFile`)}</p>
      )}
      {parsed.teams.map((block, index) => (
        <details key={`${block.teamName ?? 'team'}-${index}`} className="roster-import-modal__team">
          <summary>
            {block.teamName
              ? t(`${I18N}.teamLine`, '{{name}} · {{count}} players', {
                name: block.teamName,
                count: block.players.length,
              })
              : t(`${I18N}.unnamedTeam`, 'Team name could not be read · {{count}} players', {
                count: block.players.length,
              })}
          </summary>
          <ul className="roster-import-modal__players">
            {block.players.map((player) => (
              <li key={`${index}-${player.rowNumber}-${player.rawName}`}>
                <PlayerLine player={player} />
              </li>
            ))}
          </ul>
        </details>
      ))}
    </section>
  );
}

function PreviewView({ preview }: { preview: RosterImportPreview }) {
  const { t } = useTranslation();
  const blocked = preview.invalidNames.length > 0
    || preview.duplicateDestinations.length > 0
    || preview.parsedPlayerCount === 0;
  return (
    <section className="import-modal__preview">
      {blocked && <p className="import-modal__note">{t(`${I18N}.cannotImport`)}</p>}
      {preview.parsedPlayerCount === 0 && (
        <p className="import-modal__note">{t(`${I18N}.noPlayersInFile`)}</p>
      )}
      {preview.assignments.length === 0 && preview.parsedPlayerCount > 0 && (
        <p className="import-modal__note">{t(`${I18N}.pickDestination`)}</p>
      )}
      {preview.duplicateDestinations.length > 0 && (
        <p className="import-modal__note">
          {t(`${I18N}.duplicateDestination`, 'The same season team is selected for more than one file team: {{names}}', {
            names: preview.duplicateDestinations.join(', '),
          })}
        </p>
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
  if (player.position) marks.push(player.position);
  else if (player.isGoalkeeper) marks.push(t(`${I18N}.goalkeeper`, 'goalkeeper'));
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
