import { useCallback, useMemo, useRef, useState, type ChangeEvent, type DragEvent, type ReactElement } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type {
  SeasonImportCallbacks,
  SeasonImportCreatedRecord,
  SeasonImportDryRunCounts,
  SeasonImportError,
  SeasonImportPreview,
  SeasonImportStep,
  SeasonImportSummary,
  SeasonTeamCategory,
} from '../../../types/common/seasonImportTypes';
import './SeasonJsonImportModal.scss';

type PromptCopyState = null | 'copied' | 'downloaded' | 'error';

export interface SeasonJsonImportModalProps<TPayload> {
  onClose: () => void;
  onImported: () => void;
  i18nPrefix: string;
  prompt: string;
  buildPromptFileName: () => string;
  sampleHref: string;
  sampleDownloadName: string;
  editPath: (seasonId: string) => string;
  validatePayload: (parsed: unknown) => { valid: true; payload: TPayload } | { valid: false; errors: string[] };
  getDryRunCounts: (payload: TPayload) => SeasonImportDryRunCounts;
  getPreview: (payload: TPayload) => SeasonImportPreview;
  inferTeamCategory: (payload: TPayload) => SeasonTeamCategory;
  getSeasonName: (payload: TPayload) => string;
  getDefaultVenue: (payload: TPayload) => string;
  applyOverrides: (
    payload: TPayload,
    overrides: { name: string; venue: string; teamCategory: SeasonTeamCategory },
  ) => TPayload;
  importSeason: (
    payload: TPayload,
    callbacks: SeasonImportCallbacks,
    options: { defaultTeamCategory: SeasonTeamCategory },
  ) => Promise<SeasonImportSummary>;
  revertImport: (
    records: SeasonImportCreatedRecord[],
    callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
  ) => Promise<{ deleted: number; failed: number }>;
}

type ModalState<TPayload> =
  | { kind: 'idle' }
  | { kind: 'invalid'; errors: string[] }
  | { kind: 'preview'; payload: TPayload; counts: SeasonImportDryRunCounts; fileName: string }
  | { kind: 'running'; payload: TPayload; counts: SeasonImportDryRunCounts; fileName: string }
  | { kind: 'success'; summary: SeasonImportSummary }
  | { kind: 'failed'; summary: SeasonImportSummary; fatalMessage: string }
  | { kind: 'reverting'; records: SeasonImportCreatedRecord[] }
  | { kind: 'reverted'; deleted: number; failed: number };

interface LogLine {
  text: string;
  status: 'created' | 'existing' | 'skipped' | 'info' | 'error';
}

export function SeasonJsonImportModal<TPayload>({
  onClose,
  onImported,
  i18nPrefix,
  prompt,
  buildPromptFileName,
  sampleHref,
  sampleDownloadName,
  editPath,
  validatePayload,
  getDryRunCounts,
  getPreview,
  inferTeamCategory,
  getSeasonName,
  getDefaultVenue,
  applyOverrides,
  importSeason,
  revertImport,
}: SeasonJsonImportModalProps<TPayload>) {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();

  const [state, setState] = useState<ModalState<TPayload>>({ kind: 'idle' });
  const [log, setLog] = useState<LogLine[]>([]);
  const [progress, setProgress] = useState<{ done: number; total: number; phase: string }>({
    done: 0,
    total: 0,
    phase: '',
  });
  const [seasonNameOverride, setSeasonNameOverride] = useState<string>('');
  const [venueOverride, setVenueOverride] = useState<string>('');
  const [defaultTeamCategory, setDefaultTeamCategory] = useState<SeasonTeamCategory>('Adult');
  const abortRef = useRef(false);
  const autoRevertRef = useRef(false);
  const [autoRevert, setAutoRevert] = useState(false);
  const [promptCopyState, setPromptCopyState] = useState<PromptCopyState>(null);
  const promptCopyTimerRef = useRef<number | null>(null);

  const downloadPromptFile = useCallback((): boolean => {
    try {
      const blob: Blob = new Blob([prompt], { type: 'text/plain;charset=utf-8' });
      const url: string = URL.createObjectURL(blob);
      const link: HTMLAnchorElement = document.createElement('a');
      link.href = url;
      link.download = buildPromptFileName();
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.setTimeout(() => URL.revokeObjectURL(url), 1000);
      return true;
    } catch (err) {
      console.error('Failed to download AI prompt', err);
      return false;
    }
  }, [buildPromptFileName, prompt]);

  const handleCopyPrompt = useCallback(async (): Promise<void> => {
    if (promptCopyTimerRef.current !== null) {
      window.clearTimeout(promptCopyTimerRef.current);
      promptCopyTimerRef.current = null;
    }

    let copied = false;
    if (typeof navigator !== 'undefined' && navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
      try {
        await navigator.clipboard.writeText(prompt);
        copied = true;
      } catch (err) {
        console.warn('Clipboard write was rejected, falling back to download.', err);
      }
    }

    setPromptCopyState(copied ? 'copied' : downloadPromptFile() ? 'downloaded' : 'error');
    promptCopyTimerRef.current = window.setTimeout(() => {
      setPromptCopyState(null);
      promptCopyTimerRef.current = null;
    }, 4000);
  }, [downloadPromptFile, prompt]);

  const handleDownloadPrompt = useCallback((): void => {
    if (promptCopyTimerRef.current !== null) {
      window.clearTimeout(promptCopyTimerRef.current);
      promptCopyTimerRef.current = null;
    }
    setPromptCopyState(downloadPromptFile() ? 'downloaded' : 'error');
    promptCopyTimerRef.current = window.setTimeout(() => {
      setPromptCopyState(null);
      promptCopyTimerRef.current = null;
    }, 4000);
  }, [downloadPromptFile]);

  const handleFileSelected = useCallback(
    async (file: File) => {
      setLog([]);
      setProgress({ done: 0, total: 0, phase: '' });
      let parsed: unknown;
      try {
        parsed = JSON.parse(await file.text());
      } catch (err) {
        const msg = err instanceof Error ? err.message : String(err);
        setState({
          kind: 'invalid',
          errors: [t(`${i18nPrefix}.invalidJson`, 'File is not valid JSON: {{msg}}', { msg })],
        });
        return;
      }
      const result = validatePayload(parsed);
      if (!result.valid) {
        setState({ kind: 'invalid', errors: result.errors });
        return;
      }
      setSeasonNameOverride(getSeasonName(result.payload));
      setVenueOverride(getDefaultVenue(result.payload));
      setDefaultTeamCategory(inferTeamCategory(result.payload));
      setState({
        kind: 'preview',
        payload: result.payload,
        counts: getDryRunCounts(result.payload),
        fileName: file.name,
      });
    },
    [getDefaultVenue, getDryRunCounts, getSeasonName, i18nPrefix, inferTeamCategory, t, validatePayload],
  );

  const appendLog = useCallback((line: LogLine) => {
    setLog((prev) => [...prev, line]);
  }, []);

  const runRevert = useCallback(
    async (records: SeasonImportCreatedRecord[]) => {
      setState({ kind: 'reverting', records });
      appendLog({ text: t(`${i18nPrefix}.revertStart`, 'Reverting created entities...'), status: 'info' });
      const { deleted, failed } = await revertImport(records, {
        onStep: (step) => {
          appendLog({ text: step.label, status: step.status });
          setProgress({ done: step.index + 1, total: step.total, phase: step.phase });
        },
        onError: (err) => {
          appendLog({ text: `${err.label}: ${err.message}`, status: 'error' });
        },
      });
      setState({ kind: 'reverted', deleted, failed });
      onImported();
    },
    [appendLog, i18nPrefix, onImported, revertImport, t],
  );

  const startImport = useCallback(
    async (payload: TPayload, counts: SeasonImportDryRunCounts, fileName: string) => {
      abortRef.current = false;
      autoRevertRef.current = autoRevert;
      setLog([]);
      setProgress({ done: 0, total: 0, phase: '' });

      const effectivePayload = applyOverrides(payload, {
        name: seasonNameOverride.trim() || getSeasonName(payload),
        venue: venueOverride.trim(),
        teamCategory: defaultTeamCategory,
      });
      setState({ kind: 'running', payload: effectivePayload, counts, fileName });

      const summary = await importSeason(
        effectivePayload,
        {
          onStep: (step: SeasonImportStep) => {
            appendLog({ text: step.label, status: step.status });
            setProgress({ done: step.index + 1, total: step.total, phase: step.phase });
          },
          onError: (err: SeasonImportError) => {
            appendLog({ text: `${err.label}: ${err.message}`, status: 'error' });
          },
          shouldAbort: () => abortRef.current,
        },
        { defaultTeamCategory },
      );

      if (summary.fatal || summary.aborted) {
        const fatalMessage = summary.aborted
          ? t(`${i18nPrefix}.aborted`, 'Import was cancelled by the user.')
          : summary.errors.find((item) => item.fatal)?.message ??
            t(`${i18nPrefix}.unknownError`, 'Import failed for an unknown reason.');
        setState({ kind: 'failed', summary, fatalMessage });
        if (autoRevertRef.current && summary.created.length > 0) {
          await runRevert(summary.created);
        }
      } else {
        setState({ kind: 'success', summary });
        onImported();
      }
    },
    [
      appendLog,
      applyOverrides,
      autoRevert,
      defaultTeamCategory,
      getSeasonName,
      i18nPrefix,
      importSeason,
      onImported,
      runRevert,
      seasonNameOverride,
      t,
      venueOverride,
    ],
  );

  const onPickFile = (event: ChangeEvent<HTMLInputElement>): void => {
    const file = event.target.files?.[0];
    if (file) void handleFileSelected(file);
    event.target.value = '';
  };

  const onDrop = (event: DragEvent<HTMLElement>): void => {
    event.preventDefault();
    const file = event.dataTransfer.files?.[0];
    if (file) void handleFileSelected(file);
  };

  const onDragOver = (event: DragEvent<HTMLElement>): void => {
    event.preventDefault();
  };

  const renderLog = (): ReactElement => (
    <div className="import-modal__log">
      {log.map((line, index) => (
        <div key={index} className={`import-modal__log-line import-modal__log-line--${line.status}`}>
          <span className="import-modal__log-glyph">{glyph(line.status)}</span>
          {line.text}
        </div>
      ))}
    </div>
  );

  const renderProgress = (): ReactElement => {
    const pct = progress.total > 0 ? Math.round((progress.done / progress.total) * 100) : 0;
    return (
      <div className="import-modal__running">
        <div className="import-modal__progress">
          <div className="import-modal__progress-bar" style={{ width: `${pct}%` }} />
        </div>
        <p className="import-modal__progress-label">
          {progress.phase
            ? `${progress.phase}: ${progress.done}/${progress.total} (${pct}%)`
            : t(`${i18nPrefix}.starting`, 'Starting...')}
        </p>
        {renderLog()}
      </div>
    );
  };

  const renderBody = (): ReactElement | undefined => {
    switch (state.kind) {
      case 'idle':
        return (
          <div className="import-modal__pick">
            <label className="import-modal__dropzone" onDrop={onDrop} onDragOver={onDragOver}>
              <i className="fas fa-file-upload"></i>
              <p>{t(`${i18nPrefix}.dropzone`, 'Drop a JSON file here, or click to choose.')}</p>
              <span className="import-modal__choose-btn">
                <i className="fas fa-folder-open"></i> {t(`${i18nPrefix}.chooseFile`, 'Choose file')}
              </span>
              <input
                type="file"
                accept="application/json,.json"
                onChange={onPickFile}
                className="import-modal__file-input"
              />
            </label>

            <div className="import-modal__ai-help">
              <h4 className="import-modal__ai-help-title">
                <i className="fas fa-robot" aria-hidden="true"></i>{' '}
                {t(`${i18nPrefix}.aiHelp.title`, "Don't have a JSON file yet? Generate one with AI")}
              </h4>
              <ol className="import-modal__ai-help-steps">
                <li>
                  {t(
                    `${i18nPrefix}.aiHelp.step1`,
                    'Copy the prompt below with the "Copy prompt" button (or download it if copying is blocked).',
                  )}
                </li>
                <li>
                  {t(
                    `${i18nPrefix}.aiHelp.step2`,
                    'Open ChatGPT, Claude, Gemini or another vision-capable AI and paste the prompt (Ctrl/Cmd + V).',
                  )}
                </li>
                <li>
                  {t(
                    `${i18nPrefix}.aiHelp.step3`,
                    'Attach a screenshot / photo of the season schedule (and roster sheets if you have them) and send the message.',
                  )}
                </li>
                <li>
                  {t(
                    `${i18nPrefix}.aiHelp.step4`,
                    "Save the AI's response as a .json file and drop it into the area above.",
                  )}
                </li>
              </ol>

              <div className="import-modal__ai-help-actions">
                <button
                  type="button"
                  className="import-modal__ai-help-btn import-modal__ai-help-btn--primary"
                  onClick={() => void handleCopyPrompt()}
                >
                  <i className="fas fa-copy" aria-hidden="true"></i>{' '}
                  {t(`${i18nPrefix}.aiHelp.copyPrompt`, 'Copy AI prompt to clipboard')}
                </button>
                <button
                  type="button"
                  className="import-modal__ai-help-btn import-modal__ai-help-btn--ghost"
                  onClick={handleDownloadPrompt}
                >
                  <i className="fas fa-download" aria-hidden="true"></i>{' '}
                  {t(`${i18nPrefix}.aiHelp.downloadPrompt`, 'Download AI prompt as .txt')}
                </button>
              </div>

              {promptCopyState === 'copied' && (
                <p className="import-modal__ai-help-feedback import-modal__ai-help-feedback--ok" role="status">
                  <i className="fas fa-check-circle" aria-hidden="true"></i>{' '}
                  {t(
                    `${i18nPrefix}.aiHelp.copiedToast`,
                    'Prompt copied to clipboard. Paste it into your AI chat (Ctrl/Cmd + V) and attach the schedule image.',
                  )}
                </p>
              )}
              {promptCopyState === 'downloaded' && (
                <p className="import-modal__ai-help-feedback import-modal__ai-help-feedback--info" role="status">
                  <i className="fas fa-info-circle" aria-hidden="true"></i>{' '}
                  {t(
                    `${i18nPrefix}.aiHelp.downloadedToast`,
                    'Prompt downloaded as a .txt file. Open it, copy the contents, and paste them into your AI chat.',
                  )}
                </p>
              )}
              {promptCopyState === 'error' && (
                <p className="import-modal__ai-help-feedback import-modal__ai-help-feedback--error" role="alert">
                  <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>{' '}
                  {t(
                    `${i18nPrefix}.aiHelp.errorToast`,
                    'Could not copy or download the prompt. Please try the "Download" button again.',
                  )}
                </p>
              )}

              <a className="import-modal__sample-link" href={sampleHref} download={sampleDownloadName}>
                <i className="fas fa-file-download" aria-hidden="true"></i>{' '}
                {t(`${i18nPrefix}.downloadSample`, 'Download sample JSON')}
              </a>
            </div>
          </div>
        );
      case 'invalid':
        return (
          <div className="import-modal__errors">
            <div className="warning-icon">
              <i className="fas fa-exclamation-circle"></i>
            </div>
            <h4>{t(`${i18nPrefix}.invalidTitle`, 'JSON validation failed')}</h4>
            <ul>
              {state.errors.map((error, index) => (
                <li key={index}>{error}</li>
              ))}
            </ul>
          </div>
        );
      case 'preview':
        return (
          <div className="import-modal__preview">
            <p className="import-modal__filename">
              <i className="fas fa-file-code"></i> {state.fileName}
            </p>
            <label className="import-modal__field">
              <span className="import-modal__field-label">{t(`${i18nPrefix}.seasonName`, 'Season name')}</span>
              <input
                type="text"
                className="import-modal__field-input"
                value={seasonNameOverride}
                onChange={(event) => setSeasonNameOverride(event.target.value)}
                placeholder={getSeasonName(state.payload)}
              />
            </label>
            <label className="import-modal__field">
              <span className="import-modal__field-label">{t(`${i18nPrefix}.seasonVenue`, 'Default venue')}</span>
              <input
                type="text"
                className="import-modal__field-input"
                value={venueOverride}
                onChange={(event) => setVenueOverride(event.target.value)}
                placeholder={t(`${i18nPrefix}.seasonVenuePlaceholder`, 'e.g. Kampparit Areena')}
              />
              <span className="import-modal__field-hint">
                {t(
                  `${i18nPrefix}.seasonVenueHint`,
                  'Pre-filled from the JSON when available. Used as the default location for matches that omit a venue.',
                )}
              </span>
            </label>
            <label className="import-modal__field">
              <span className="import-modal__field-label">{t(`${i18nPrefix}.seasonCategory`, 'Season type')}</span>
              <select
                className="import-modal__field-input"
                value={defaultTeamCategory}
                onChange={(event) => setDefaultTeamCategory(event.target.value as SeasonTeamCategory)}
              >
                <option value="Adult">{t('audience.adult', 'Adult')}</option>
                <option value="Youth">{t('audience.youth', 'Youth')}</option>
                <option value="Women">{t('audience.women', 'Women')}</option>
              </select>
              <span className="import-modal__field-hint">
                {t(
                  `${i18nPrefix}.seasonCategoryHint`,
                  'Applied to the season and to newly created teams (existing teams keep their category).',
                )}
              </span>
            </label>
            <table className="import-modal__counts">
              <tbody>
                <tr>
                  <th>{t(`${i18nPrefix}.counts.clubs`, 'Clubs')}</th>
                  <td>{state.counts.clubs}</td>
                </tr>
                <tr>
                  <th>{t(`${i18nPrefix}.counts.divisions`, 'Divisions')}</th>
                  <td>{state.counts.divisions}</td>
                </tr>
                <tr>
                  <th>{t(`${i18nPrefix}.counts.teams`, 'Teams')}</th>
                  <td>{state.counts.teams}</td>
                </tr>
                <tr>
                  <th>{t(`${i18nPrefix}.counts.players`, 'Players')}</th>
                  <td>{state.counts.players}</td>
                </tr>
                <tr>
                  <th>{t(`${i18nPrefix}.counts.assignments`, 'Season assignments')}</th>
                  <td>{state.counts.assignments}</td>
                </tr>
                <tr>
                  <th>{t(`${i18nPrefix}.counts.matches`, 'Matches')}</th>
                  <td>{state.counts.matches}</td>
                </tr>
              </tbody>
            </table>
            <SeasonImportPreviewPanel
              preview={getPreview(state.payload)}
              seasonName={seasonNameOverride.trim() || getSeasonName(state.payload)}
              hasDefaultVenue={venueOverride.trim().length > 0}
              language={i18n.language}
              t={t}
              i18nPrefix={i18nPrefix}
            />
            <p className="import-modal__note">
              {t(
                `${i18nPrefix}.previewNote`,
                'Existing clubs/teams (matched by name) are reused. Only missing ones are created.',
              )}
            </p>
            {state.counts.players > 0 && (
              <p className="import-modal__note">
                {t(
                  `${i18nPrefix}.playerRosterNote`,
                  'Player rosters (if included in the JSON) are created and added to teams automatically. Existing players (matched by name) are reused.',
                )}
              </p>
            )}
            <label className="import-modal__checkbox">
              <input type="checkbox" checked={autoRevert} onChange={(event) => setAutoRevert(event.target.checked)} />
              {t(`${i18nPrefix}.autoRevert`, 'Automatically revert if any step fails')}
            </label>
          </div>
        );
      case 'running':
      case 'reverting':
        return renderProgress();
      case 'success':
        return (
          <div className="import-modal__success">
            <div className="warning-icon import-modal__success-icon">
              <i className="fas fa-check-circle"></i>
            </div>
            <h4>{t(`${i18nPrefix}.successTitle`, 'Import complete')}</h4>
            <ImportSummaryView summary={state.summary} t={t} i18nPrefix={i18nPrefix} />
            {renderLog()}
          </div>
        );
      case 'failed':
        return (
          <div className="import-modal__failed">
            <div className="warning-icon">
              <i className="fas fa-exclamation-triangle"></i>
            </div>
            <h4>{t(`${i18nPrefix}.failedTitle`, 'Import failed')}</h4>
            <p className="warning-text">{state.fatalMessage}</p>
            <ImportSummaryView summary={state.summary} t={t} i18nPrefix={i18nPrefix} />
            {state.summary.created.length > 0 && (
              <p className="import-modal__note">
                {t(
                  `${i18nPrefix}.failedNote`,
                  '{{count}} entities were created before the failure. You can revert them.',
                  { count: state.summary.created.length },
                )}
              </p>
            )}
            {renderLog()}
          </div>
        );
      case 'reverted':
        return (
          <div className="import-modal__success">
            <div className="warning-icon">
              <i className="fas fa-undo"></i>
            </div>
            <h4>{t(`${i18nPrefix}.revertedTitle`, 'Revert complete')}</h4>
            <p>
              {t(
                `${i18nPrefix}.revertedSummary`,
                'Removed {{deleted}} item(s). {{failed}} failure(s) — check the log.',
                { deleted: state.deleted, failed: state.failed },
              )}
            </p>
            {renderLog()}
          </div>
        );
    }
  };

  const footer = useMemo(() => {
    switch (state.kind) {
      case 'idle':
      case 'invalid':
        return (
          <button className="btn btn-secondary" onClick={onClose}>
            {t('common.close', 'Close')}
          </button>
        );
      case 'preview':
        return (
          <>
            <button className="btn btn-secondary" onClick={onClose}>
              {t('common.cancel', 'Cancel')}
            </button>
            <button
              className="btn btn-primary"
              onClick={() => void startImport(state.payload, state.counts, state.fileName)}
            >
              <i className="fas fa-play"></i> {t(`${i18nPrefix}.startImport`, 'Start import')}
            </button>
          </>
        );
      case 'running':
        return (
          <button
            className="btn btn-danger"
            onClick={() => {
              abortRef.current = true;
            }}
          >
            <i className="fas fa-stop"></i> {t('common.cancel', 'Cancel')}
          </button>
        );
      case 'reverting':
        return (
          <button className="btn btn-secondary" disabled>
            {t(`${i18nPrefix}.reverting`, 'Reverting...')}
          </button>
        );
      case 'success':
        return (
          <>
            <button className="btn btn-secondary" onClick={onClose}>
              {t('common.close', 'Close')}
            </button>
            {state.summary.seasonId && (
              <button
                className="btn btn-primary"
                onClick={() => {
                  navigate(editPath(state.summary.seasonId as string));
                  onClose();
                }}
              >
                <i className="fas fa-arrow-right"></i> {t(`${i18nPrefix}.openSeason`, 'Open season')}
              </button>
            )}
          </>
        );
      case 'failed':
        return (
          <>
            <button className="btn btn-secondary" onClick={onClose}>
              {t('common.close', 'Close')}
            </button>
            {state.summary.created.length > 0 && (
              <button className="btn btn-danger" onClick={() => void runRevert(state.summary.created)}>
                <i className="fas fa-undo"></i> {t(`${i18nPrefix}.revertAll`, 'Revert all changes')}
              </button>
            )}
          </>
        );
      case 'reverted':
        return (
          <button className="btn btn-secondary" onClick={onClose}>
            {t('common.close', 'Close')}
          </button>
        );
    }
  }, [editPath, i18nPrefix, navigate, onClose, runRevert, startImport, state, t]);

  return (
    <div className="modal-overlay">
      <div className="modal-content import-modal">
        <div className="modal-header">
          <h3>{t(`${i18nPrefix}.title`, 'Import season from JSON')}</h3>
          <button
            className="modal-close-btn"
            onClick={state.kind === 'running' || state.kind === 'reverting' ? undefined : onClose}
            disabled={state.kind === 'running' || state.kind === 'reverting'}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>
        <div className="modal-body import-modal__body">{renderBody()}</div>
        <div className="modal-footer">{footer}</div>
      </div>
    </div>
  );
}

function formatPreviewDate(value: string, language: string, withTime: boolean): string {
  const locale = language.toLowerCase().startsWith('fi') ? 'fi-FI' : 'en-GB';
  const parsed = new Date(withTime ? value : `${value}T00:00:00`);
  if (Number.isNaN(parsed.getTime())) return value;
  return new Intl.DateTimeFormat(locale, {
    day: 'numeric',
    month: 'numeric',
    year: 'numeric',
    ...(withTime ? { hour: '2-digit', minute: '2-digit' } : {}),
  }).format(parsed);
}

const SeasonImportPreviewPanel = ({
  preview,
  seasonName,
  hasDefaultVenue,
  language,
  t,
  i18nPrefix,
}: {
  preview: SeasonImportPreview;
  seasonName: string;
  hasDefaultVenue: boolean;
  language: string;
  t: ReturnType<typeof useTranslation>['t'];
  i18nPrefix: string;
}): ReactElement => {
  const checks: string[] = [];
  if (preview.teamsWithoutMatches.length > 0) {
    checks.push(
      t(`${i18nPrefix}.previewCheckTeamsWithoutMatches`, 'Teams with no matches: {{teams}}', {
        teams: preview.teamsWithoutMatches.join(', '),
      }),
    );
  }
  if (preview.matchesOutsideSeason > 0) {
    checks.push(
      t(`${i18nPrefix}.previewCheckMatchesOutsideSeason`, '{{count}} matches fall outside the season dates.', {
        count: preview.matchesOutsideSeason,
      }),
    );
  }
  if (preview.matchesWithoutOwnVenue > 0 && !hasDefaultVenue) {
    checks.push(
      t(
        `${i18nPrefix}.previewCheckMissingVenue`,
        '{{count}} matches have no venue and no default venue is set.',
        { count: preview.matchesWithoutOwnVenue },
      ),
    );
  }

  const rangeLabel =
    preview.firstMatchAt && preview.lastMatchAt
      ? t(`${i18nPrefix}.previewMatchWindow`, 'Matches {{first}} – {{last}}', {
          first: formatPreviewDate(preview.firstMatchAt, language, true),
          last: formatPreviewDate(preview.lastMatchAt, language, true),
        })
      : t(`${i18nPrefix}.previewNoMatches`, 'The file has no matches.');

  return (
    <section className="import-modal__checklist" aria-label={t(`${i18nPrefix}.previewHeading`, 'Review before import')}>
      <h4 className="import-modal__checklist-title">{t(`${i18nPrefix}.previewHeading`, 'Review before import')}</h4>
      <p className="import-modal__checklist-season">
        <strong>{seasonName}</strong>
        {' · '}
        {t(`${i18nPrefix}.previewSeasonRange`, '{{start}} – {{end}}', {
          start: formatPreviewDate(preview.startDate, language, false),
          end: formatPreviewDate(preview.endDate, language, false),
        })}
        {' · '}
        {rangeLabel}
      </p>
      {preview.venues.length > 0 ? (
        <p className="import-modal__checklist-meta">
          {t(`${i18nPrefix}.previewVenues`, 'Venues: {{venues}}', { venues: preview.venues.join(', ') })}
        </p>
      ) : (
        <p className="import-modal__checklist-meta">
          {t(`${i18nPrefix}.previewNoVenue`, 'No venue is set on the matches.')}
        </p>
      )}
      <ul className="import-modal__divisions">
        {preview.divisions.map((division) => (
          <li key={division.name} className="import-modal__division">
            <p className="import-modal__division-title">
              {t(`${i18nPrefix}.previewDivisionTeams`, '{{division}} · {{count}} teams', {
                division: division.name,
                count: division.teams.length,
              })}
            </p>
            {division.teams.length > 0 ? (
              <ul className="import-modal__teams">
                {division.teams.map((team) => {
                  const showClub = team.clubName.trim().toLowerCase() !== team.name.trim().toLowerCase();
                  const players =
                    team.playerCount > 0
                      ? ` · ${t(`${i18nPrefix}.previewPlayers`, '{{count}} players', { count: team.playerCount })}`
                      : '';
                  return (
                    <li key={team.name}>
                      {showClub
                        ? t(`${i18nPrefix}.previewTeamLineWithClub`, '{{name}} ({{club}}) · {{matches}} matches', {
                            name: team.name,
                            club: team.clubName,
                            matches: team.matchCount,
                          })
                        : t(`${i18nPrefix}.previewTeamLine`, '{{name}} · {{matches}} matches', {
                            name: team.name,
                            matches: team.matchCount,
                          })}
                      {players}
                    </li>
                  );
                })}
              </ul>
            ) : (
              <p className="import-modal__checklist-meta">
                {t(`${i18nPrefix}.previewDivisionEmpty`, 'No teams in this division.')}
              </p>
            )}
          </li>
        ))}
      </ul>
      {checks.length > 0 ? (
        <ul className="import-modal__checks">
          {checks.map((check) => (
            <li key={check}>{check}</li>
          ))}
        </ul>
      ) : (
        <p className="import-modal__note import-modal__note--ok">
          {t(`${i18nPrefix}.previewCheckOk`, 'Teams, divisions, and match times look consistent.')}
        </p>
      )}
    </section>
  );
};

const ImportSummaryView = ({
  summary,
  t,
  i18nPrefix,
}: {
  summary: SeasonImportSummary;
  t: ReturnType<typeof useTranslation>['t'];
  i18nPrefix: string;
}) => (
  <div className="import-modal__summary">
    <SummaryRow
      label={t(`${i18nPrefix}.counts.clubs`, 'Clubs')}
      created={summary.clubsCreated}
      existing={summary.clubsExisting}
    />
    <SummaryRow
      label={t(`${i18nPrefix}.counts.divisions`, 'Divisions')}
      created={summary.divisionsCreated}
      existing={summary.divisionsExisting}
    />
    <SummaryRow
      label={t(`${i18nPrefix}.counts.teams`, 'Teams')}
      created={summary.teamsCreated}
      existing={summary.teamsExisting}
    />
    <SummaryRow
      label={t(`${i18nPrefix}.counts.players`, 'Players')}
      created={summary.playersCreated}
      existing={summary.playersExisting}
    />
    <SummaryRow
      label={t(`${i18nPrefix}.counts.season`, 'Season')}
      created={summary.seasonId ? 1 : 0}
      existing={0}
    />
    <SummaryRow
      label={t(`${i18nPrefix}.counts.assignments`, 'Season assignments')}
      created={summary.seasonAssignments}
      existing={0}
    />
    <SummaryRow
      label={t(`${i18nPrefix}.counts.matches`, 'Matches')}
      created={summary.matchesCreated}
      existing={0}
    />
  </div>
);

const SummaryRow = ({ label, created, existing }: { label: string; created: number; existing: number }) => (
  <div className="import-modal__summary-row">
    <span className="import-modal__summary-label">{label}</span>
    <span className="import-modal__summary-value">
      <strong>{created}</strong>
      {existing > 0 && <span className="import-modal__summary-existing"> (+{existing} existing)</span>}
    </span>
  </div>
);

function glyph(status: LogLine['status']): string {
  switch (status) {
    case 'created':
      return '✓';
    case 'existing':
      return '↻';
    case 'skipped':
      return '–';
    case 'error':
      return '✕';
    case 'info':
      return '•';
  }
}

export default SeasonJsonImportModal;
