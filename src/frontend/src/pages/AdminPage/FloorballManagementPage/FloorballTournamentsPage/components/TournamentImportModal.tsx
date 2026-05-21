import { useCallback, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  getDryRunCounts,
  importTournament,
  revertImport,
  validatePayload,
} from '../../../../../api/floorball/tournamentImportService';
import type {
  CreatedRecord,
  ImportDryRunCounts,
  ImportError,
  ImportStep,
  ImportSummary,
  TournamentImportPayload,
} from '../../../../../types/floorball/tournamentImportTypes';
import { TeamCategory } from '../../../../../types/floorball/floorballTypes';
import './TournamentImportModal.scss';

interface TournamentImportModalProps {
  onClose: () => void;
  /** Called after a successful import (so the parent can refresh the list). */
  onImported: () => void;
}

type ModalState =
  | { kind: 'idle' }
  | { kind: 'invalid'; errors: string[] }
  | { kind: 'preview'; payload: TournamentImportPayload; counts: ImportDryRunCounts; fileName: string }
  | { kind: 'running'; payload: TournamentImportPayload; counts: ImportDryRunCounts; fileName: string }
  | { kind: 'success'; summary: ImportSummary }
  | { kind: 'failed'; summary: ImportSummary; fatalMessage: string }
  | { kind: 'reverting'; records: CreatedRecord[] }
  | { kind: 'reverted'; deleted: number; failed: number };

interface LogLine {
  text: string;
  status: 'created' | 'existing' | 'skipped' | 'info' | 'error';
}

const SAMPLE_HREF = new URL('../../../../../types/floorball/tournamentImport.sample.json', import.meta.url).href;

export const TournamentImportModal = ({ onClose, onImported }: TournamentImportModalProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [state, setState] = useState<ModalState>({ kind: 'idle' });
  const [log, setLog] = useState<LogLine[]>([]);
  const [progress, setProgress] = useState<{ done: number; total: number; phase: string }>({ done: 0, total: 0, phase: '' });
  const [tournamentNameOverride, setTournamentNameOverride] = useState<string>('');
  const [defaultTeamCategory, setDefaultTeamCategory] = useState<TeamCategory>(TeamCategory.Adult);
  const abortRef = useRef(false);
  const autoRevertRef = useRef(false);
  const [autoRevert, setAutoRevert] = useState(false);

  const handleFileSelected = useCallback(async (file: File) => {
    setLog([]);
    setProgress({ done: 0, total: 0, phase: '' });
    let parsed: unknown;
    try {
      const text = await file.text();
      parsed = JSON.parse(text);
    } catch (err) {
      const msg = err instanceof Error ? err.message : String(err);
      setState({ kind: 'invalid', errors: [t('floorball.tournaments.import.invalidJson', 'File is not valid JSON: {{msg}}', { msg })] });
      return;
    }
    const result = validatePayload(parsed);
    if (!result.valid) {
      setState({ kind: 'invalid', errors: result.errors });
      return;
    }
    setTournamentNameOverride(result.payload.tournament.name);
    setState({
      kind: 'preview',
      payload: result.payload,
      counts: getDryRunCounts(result.payload),
      fileName: file.name,
    });
  }, [t]);

  const appendLog = useCallback((line: LogLine) => {
    setLog((prev) => [...prev, line]);
  }, []);

  const runRevert = useCallback(async (records: CreatedRecord[]) => {
    setState({ kind: 'reverting', records });
    appendLog({ text: t('floorball.tournaments.import.revertStart', 'Reverting created entities...'), status: 'info' });
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
  }, [appendLog, onImported, t]);

  const startImport = useCallback(async (payload: TournamentImportPayload, counts: ImportDryRunCounts, fileName: string) => {
    abortRef.current = false;
    autoRevertRef.current = autoRevert;
    setLog([]);
    setProgress({ done: 0, total: 0, phase: '' });

    // Apply the user's edits before kicking off the import.
    const effectiveName = tournamentNameOverride.trim() || payload.tournament.name;
    const effectivePayload: TournamentImportPayload = {
      ...payload,
      tournament: { ...payload.tournament, name: effectiveName },
    };
    setState({ kind: 'running', payload: effectivePayload, counts, fileName });

    const summary = await importTournament(
      effectivePayload,
      {
        onStep: (step: ImportStep) => {
          appendLog({ text: step.label, status: step.status });
          setProgress({ done: step.index + 1, total: step.total, phase: step.phase });
        },
        onError: (err: ImportError) => {
          appendLog({ text: `${err.label}: ${err.message}`, status: 'error' });
        },
        shouldAbort: () => abortRef.current,
      },
      { defaultTeamCategory },
    );

    if (summary.fatal || summary.aborted) {
      const fatalMessage = summary.aborted
        ? t('floorball.tournaments.import.aborted', 'Import was cancelled by the user.')
        : summary.errors.find((e) => e.fatal)?.message ?? t('floorball.tournaments.import.unknownError', 'Import failed for an unknown reason.');
      setState({ kind: 'failed', summary, fatalMessage });
      if (autoRevertRef.current && summary.created.length > 0) {
        await runRevert(summary.created);
      }
    } else {
      setState({ kind: 'success', summary });
      onImported();
    }
  }, [appendLog, autoRevert, defaultTeamCategory, onImported, runRevert, t, tournamentNameOverride]);

  const onPickFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) handleFileSelected(file);
    e.target.value = '';
  };

  const onDrop = (e: React.DragEvent<HTMLElement>) => {
    e.preventDefault();
    const file = e.dataTransfer.files?.[0];
    if (file) handleFileSelected(file);
  };

  const onDragOver = (e: React.DragEvent<HTMLElement>) => {
    e.preventDefault();
  };

  const renderBody = () => {
    switch (state.kind) {
      case 'idle':
        return (
          <div className="import-modal__pick">
            <label
              className="import-modal__dropzone"
              onDrop={onDrop}
              onDragOver={onDragOver}
            >
              <i className="fas fa-file-upload"></i>
              <p>
                {t('floorball.tournaments.import.dropzone', 'Drop a JSON file here, or click to choose.')}
              </p>
              <span className="import-modal__choose-btn">
                <i className="fas fa-folder-open"></i>
                {' '}
                {t('floorball.tournaments.import.chooseFile', 'Choose file')}
              </span>
              <input
                type="file"
                accept="application/json,.json"
                onChange={onPickFile}
                className="import-modal__file-input"
              />
            </label>
            <a
              className="import-modal__sample-link"
              href={SAMPLE_HREF}
              download="tournament-import-sample.json"
            >
              <i className="fas fa-download"></i>
              {t('floorball.tournaments.import.downloadSample', 'Download sample JSON')}
            </a>
          </div>
        );
      case 'invalid':
        return (
          <div className="import-modal__errors">
            <div className="warning-icon"><i className="fas fa-exclamation-circle"></i></div>
            <h4>{t('floorball.tournaments.import.invalidTitle', 'JSON validation failed')}</h4>
            <ul>
              {state.errors.map((e, i) => <li key={i}>{e}</li>)}
            </ul>
          </div>
        );
      case 'preview':
        return (
          <div className="import-modal__preview">
            <p className="import-modal__filename"><i className="fas fa-file-code"></i> {state.fileName}</p>
            <label className="import-modal__field">
              <span className="import-modal__field-label">
                {t('floorball.tournaments.import.tournamentName', 'Tournament name')}
              </span>
              <input
                type="text"
                className="import-modal__field-input"
                value={tournamentNameOverride}
                onChange={(e) => setTournamentNameOverride(e.target.value)}
                placeholder={state.payload.tournament.name}
              />
            </label>
            <label className="import-modal__field">
              <span className="import-modal__field-label">
                {t('floorball.tournaments.import.tournamentCategory', 'Tournament type')}
              </span>
              <select
                className="import-modal__field-input"
                value={defaultTeamCategory}
                onChange={(e) => setDefaultTeamCategory(e.target.value as TeamCategory)}
              >
                <option value={TeamCategory.Adult}>{t('floorball.categories.adult', 'Adult')}</option>
                <option value={TeamCategory.Youth}>{t('floorball.categories.youth', 'Youth')}</option>
                <option value={TeamCategory.Women}>{t('floorball.categories.women', 'Women')}</option>
              </select>
              <span className="import-modal__field-hint">
                {t('floorball.tournaments.import.tournamentCategoryHint', 'Applied to newly created teams (existing teams keep their category).')}
              </span>
            </label>
            <table className="import-modal__counts">
              <tbody>
                <tr><th>{t('floorball.tournaments.import.counts.clubs', 'Clubs')}</th><td>{state.counts.clubs}</td></tr>
                <tr><th>{t('floorball.tournaments.import.counts.teams', 'Teams')}</th><td>{state.counts.teams}</td></tr>
                <tr><th>{t('floorball.tournaments.import.counts.players', 'Players')}</th><td>{state.counts.players}</td></tr>
                <tr><th>{t('floorball.tournaments.import.counts.groups', 'Groups')}</th><td>{state.counts.groups}</td></tr>
                <tr><th>{t('floorball.tournaments.import.counts.assignments', 'Group assignments')}</th><td>{state.counts.groupAssignments}</td></tr>
                <tr><th>{t('floorball.tournaments.import.counts.matches', 'Matches')}</th><td>{state.counts.matches}</td></tr>
                <tr><th>{t('floorball.tournaments.import.counts.playoffSlots', 'Playoff slots')}</th><td>{state.counts.playoffSlots}</td></tr>
              </tbody>
            </table>
            <p className="import-modal__note">
              {t('floorball.tournaments.import.previewNote', 'Existing clubs/teams (matched by name) are reused. Only missing ones are created.')}
            </p>
            {state.counts.players > 0 && (
              <p className="import-modal__note">
                {t('floorball.tournaments.import.playerRosterNote', 'Player rosters (if included in the JSON) are created and added to teams automatically. Existing players (matched by name) are reused.')}
              </p>
            )}
            {state.counts.playoffSlots > 0 && (
              <p className="import-modal__note">
                {t('floorball.tournaments.import.playoffSlotsNote', 'Playoff slots will appear as placeholder "TBD vs TBD" rows in the schedule. The real teams are filled in automatically when the playoff stage is started.')}
              </p>
            )}
            <label className="import-modal__checkbox">
              <input
                type="checkbox"
                checked={autoRevert}
                onChange={(e) => setAutoRevert(e.target.checked)}
              />
              {t('floorball.tournaments.import.autoRevert', 'Automatically revert if any step fails')}
            </label>
          </div>
        );
      case 'running':
      case 'reverting':
        return renderProgress();
      case 'success':
        return (
          <div className="import-modal__success">
            <div className="warning-icon import-modal__success-icon"><i className="fas fa-check-circle"></i></div>
            <h4>{t('floorball.tournaments.import.successTitle', 'Import complete')}</h4>
            <ImportSummaryView summary={state.summary} t={t} />
            {renderLog()}
          </div>
        );
      case 'failed':
        return (
          <div className="import-modal__failed">
            <div className="warning-icon"><i className="fas fa-exclamation-triangle"></i></div>
            <h4>{t('floorball.tournaments.import.failedTitle', 'Import failed')}</h4>
            <p className="warning-text">{state.fatalMessage}</p>
            <ImportSummaryView summary={state.summary} t={t} />
            {state.summary.created.length > 0 && (
              <p className="import-modal__note">
                {t('floorball.tournaments.import.failedNote', '{{count}} entities were created before the failure. You can revert them.', { count: state.summary.created.length })}
              </p>
            )}
            {renderLog()}
          </div>
        );
      case 'reverted':
        return (
          <div className="import-modal__success">
            <div className="warning-icon"><i className="fas fa-undo"></i></div>
            <h4>{t('floorball.tournaments.import.revertedTitle', 'Revert complete')}</h4>
            <p>
              {t('floorball.tournaments.import.revertedSummary', 'Removed {{deleted}} item(s). {{failed}} failure(s) — check the log.', {
                deleted: state.deleted,
                failed: state.failed,
              })}
            </p>
            {renderLog()}
          </div>
        );
    }
  };

  const renderProgress = () => {
    const pct = progress.total > 0 ? Math.round((progress.done / progress.total) * 100) : 0;
    return (
      <div className="import-modal__running">
        <div className="import-modal__progress">
          <div className="import-modal__progress-bar" style={{ width: `${pct}%` }} />
        </div>
        <p className="import-modal__progress-label">
          {progress.phase ? `${progress.phase}: ${progress.done}/${progress.total} (${pct}%)` : t('floorball.tournaments.import.starting', 'Starting...')}
        </p>
        {renderLog()}
      </div>
    );
  };

  const renderLog = () => (
    <div className="import-modal__log">
      {log.map((line, i) => (
        <div key={i} className={`import-modal__log-line import-modal__log-line--${line.status}`}>
          <span className="import-modal__log-glyph">{glyph(line.status)}</span>
          {line.text}
        </div>
      ))}
    </div>
  );

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
              onClick={() => startImport(state.payload, state.counts, state.fileName)}
            >
              <i className="fas fa-play"></i> {t('floorball.tournaments.import.startImport', 'Start import')}
            </button>
          </>
        );
      case 'running':
        return (
          <button className="btn btn-danger" onClick={() => { abortRef.current = true; }}>
            <i className="fas fa-stop"></i> {t('common.cancel', 'Cancel')}
          </button>
        );
      case 'reverting':
        return (
          <button className="btn btn-secondary" disabled>
            {t('floorball.tournaments.import.reverting', 'Reverting...')}
          </button>
        );
      case 'success':
        return (
          <>
            <button className="btn btn-secondary" onClick={onClose}>
              {t('common.close', 'Close')}
            </button>
            {state.summary.tournamentId && (
              <button
                className="btn btn-primary"
                onClick={() => {
                  navigate(`/admin/floorball/tournaments/${state.summary.tournamentId}/edit`);
                  onClose();
                }}
              >
                <i className="fas fa-arrow-right"></i> {t('floorball.tournaments.import.openTournament', 'Open tournament')}
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
              <button className="btn btn-danger" onClick={() => runRevert(state.summary.created)}>
                <i className="fas fa-undo"></i> {t('floorball.tournaments.import.revertAll', 'Revert all changes')}
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
  }, [state, t, onClose, startImport, runRevert, navigate]);

  return (
    <div className="modal-overlay">
      <div className="modal-content import-modal">
        <div className="modal-header">
          <h3>{t('floorball.tournaments.import.title', 'Import tournament from JSON')}</h3>
          <button
            className="modal-close-btn"
            onClick={state.kind === 'running' || state.kind === 'reverting' ? undefined : onClose}
            disabled={state.kind === 'running' || state.kind === 'reverting'}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>
        <div className="modal-body import-modal__body">
          {renderBody()}
        </div>
        <div className="modal-footer">{footer}</div>
      </div>
    </div>
  );
};

const ImportSummaryView = ({ summary, t }: { summary: ImportSummary; t: ReturnType<typeof useTranslation>['t'] }) => (
  <div className="import-modal__summary">
    <SummaryRow label={t('floorball.tournaments.import.counts.clubs', 'Clubs')} created={summary.clubsCreated} existing={summary.clubsExisting} />
    <SummaryRow label={t('floorball.tournaments.import.counts.divisions', 'Divisions')} created={summary.divisionsCreated} existing={summary.divisionsExisting} />
    <SummaryRow label={t('floorball.tournaments.import.counts.teams', 'Teams')} created={summary.teamsCreated} existing={summary.teamsExisting} />
    <SummaryRow label={t('floorball.tournaments.import.counts.players', 'Players')} created={summary.playersCreated} existing={summary.playersExisting} />
    <SummaryRow label={t('floorball.tournaments.import.counts.tournament', 'Tournament')} created={summary.tournamentId ? 1 : 0} existing={0} />
    <SummaryRow label={t('floorball.tournaments.import.counts.groups', 'Groups')} created={summary.groupsCreated} existing={0} />
    <SummaryRow label={t('floorball.tournaments.import.counts.assignments', 'Group assignments')} created={summary.groupAssignments} existing={0} />
    <SummaryRow label={t('floorball.tournaments.import.counts.matches', 'Matches')} created={summary.matchesCreated} existing={0} />
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
    case 'created': return '✓';
    case 'existing': return '↻';
    case 'skipped': return '–';
    case 'error': return '✕';
    case 'info': return '•';
  }
}

export default TournamentImportModal;
