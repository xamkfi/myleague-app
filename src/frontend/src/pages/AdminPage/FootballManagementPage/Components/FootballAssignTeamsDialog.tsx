import { useCallback, useEffect, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { footballMatchService } from '../../../../api/football/footballMatchService';
import { footballTeamNameSearchService } from '../../../../api/football/footballTeamNameSearchService';
import type { FootballMatchDto } from '../../../../types/football/footballTypes';
import SearchableInfiniteDropdown from '../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import '../../../../components/AssignTeamsDialog/AssignTeamsDialog.scss';

interface FootballAssignTeamsDialogProps {
  isOpen: boolean;
  match: FootballMatchDto;
  onClose: () => void;
  onSaved: (updatedMatch: FootballMatchDto) => void;
}

const FootballAssignTeamsDialog = ({
  isOpen,
  match,
  onClose,
  onSaved,
}: FootballAssignTeamsDialogProps): ReactElement | null => {
  const { t } = useTranslation();
  const [homeTeamId, setHomeTeamId] = useState<string>(match.homeTeamId ?? '');
  const [awayTeamId, setAwayTeamId] = useState<string>(match.awayTeamId ?? '');
  const [initialHomeOptions, setInitialHomeOptions] = useState<{ id: string; name: string }[]>([]);
  const [initialAwayOptions, setInitialAwayOptions] = useState<{ id: string; name: string }[]>([]);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    setHomeTeamId(match.homeTeamId ?? '');
    setAwayTeamId(match.awayTeamId ?? '');
    setError(null);

    if (match.homeTeamId && match.homeTeamName) {
      setInitialHomeOptions([{ id: match.homeTeamId, name: match.homeTeamName }]);
    } else {
      setInitialHomeOptions([]);
    }
    if (match.awayTeamId && match.awayTeamName) {
      setInitialAwayOptions([{ id: match.awayTeamId, name: match.awayTeamName }]);
    } else {
      setInitialAwayOptions([]);
    }
  }, [isOpen, match.homeTeamId, match.homeTeamName, match.awayTeamId, match.awayTeamName]);

  const searchTeamsWith = useCallback(
    async (initialOptions: { id: string; name: string }[], query: string, page: number) => {
      const result = await footballTeamNameSearchService.searchTeams(query, page);
      if (page !== 1 || initialOptions.length === 0) {
        return result;
      }
      const trimmed: string = query.trim().toLowerCase();
      const filteredInitial = trimmed
        ? initialOptions.filter((opt) => opt.name.toLowerCase().includes(trimmed))
        : initialOptions;
      const seenIds = new Set<string>(filteredInitial.map((o) => o.id));
      const merged = [
        ...filteredInitial,
        ...result.data.filter((opt) => !seenIds.has(opt.id)),
      ];
      return { data: merged, pagination: result.pagination };
    },
    []
  );

  const searchHome = useCallback(
    (query: string, page: number) => searchTeamsWith(initialHomeOptions, query, page),
    [initialHomeOptions, searchTeamsWith]
  );

  const searchAway = useCallback(
    (query: string, page: number) => searchTeamsWith(initialAwayOptions, query, page),
    [initialAwayOptions, searchTeamsWith]
  );

  const canEdit: boolean = match.status === 'Scheduled' || match.status === 'Postponed';
  const homeChanged: boolean = (homeTeamId || null) !== (match.homeTeamId ?? null);
  const awayChanged: boolean = (awayTeamId || null) !== (match.awayTeamId ?? null);
  const sameTeamSelected: boolean = Boolean(homeTeamId) && homeTeamId === awayTeamId;
  const canSave: boolean = canEdit && !saving && !sameTeamSelected && (homeChanged || awayChanged);

  const handleSave = useCallback(async (): Promise<void> => {
    if (!canSave) return;
    try {
      setSaving(true);
      setError(null);
      const response = await footballMatchService.assignTeams(match.id, {
        homeTeamId: homeTeamId ? homeTeamId : null,
        awayTeamId: awayTeamId ? awayTeamId : null,
      });
      if (response.data) {
        onSaved(response.data);
        onClose();
      }
    } catch (err: unknown) {
      const message: string = err instanceof Error
        ? err.message
        : t('football.matches.assignTeams.genericError', 'Failed to save teams.');
      setError(message);
    } finally {
      setSaving(false);
    }
  }, [canSave, homeTeamId, awayTeamId, match.id, onSaved, onClose, t]);

  if (!isOpen) return null;

  return (
    <div className="assign-teams-dialog__overlay" onClick={onClose}>
      <div
        className="assign-teams-dialog"
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="assign-teams-dialog-title"
      >
        <header className="assign-teams-dialog__header">
          <h2 id="assign-teams-dialog-title" className="assign-teams-dialog__title">
            {t('football.matches.assignTeams.title', 'Assign teams')}
          </h2>
          <button
            type="button"
            className="assign-teams-dialog__close"
            onClick={onClose}
            aria-label={t('common.close', 'Close')}
            disabled={saving}
          >
            <i className="fas fa-times" aria-hidden="true"></i>
          </button>
        </header>

        <div className="assign-teams-dialog__body">
          {!canEdit && (
            <div className="assign-teams-dialog__warning" role="status">
              <i className="fas fa-info-circle" aria-hidden="true"></i>
              {t(
                'football.matches.assignTeams.notEditable',
                'Teams can only be changed when the match is Scheduled or Postponed.'
              )}
            </div>
          )}

          <p className="assign-teams-dialog__hint">
            {t(
              'football.matches.assignTeams.hint',
              'Leave a team unselected to reset it to TBD. Playoff changes propagate to the next match when its feeder match has not finished yet.'
            )}
          </p>

          {error && (
            <div className="assign-teams-dialog__error" role="alert">
              <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
              {error}
            </div>
          )}

          {sameTeamSelected && (
            <div className="assign-teams-dialog__error" role="alert">
              <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
              {t('football.matches.assignTeams.sameTeam', 'Home and away teams cannot be the same.')}
            </div>
          )}

          <div className="assign-teams-dialog__field">
            <label htmlFor="assign-teams-home" className="assign-teams-dialog__label">
              {t('football.matches.homeTeamLabel', 'Home Team')}
            </label>
            <SearchableInfiniteDropdown
              placeholder={t('football.matches.assignTeams.homePlaceholder', 'Select home team (optional)')}
              value={homeTeamId}
              onChange={(value: string) => setHomeTeamId(value)}
              onSearch={searchHome}
              searchPlaceholder={t('common.search', 'Search...')}
              emptyMessage={t('football.matches.assignTeams.noTeams', 'No teams found')}
              disabled={!canEdit || saving}
              loadInitialDataOnMount={true}
            />
            {homeTeamId && (
              <button
                type="button"
                className="assign-teams-dialog__clear"
                onClick={() => setHomeTeamId('')}
                disabled={!canEdit || saving}
              >
                <i className="fas fa-times-circle" aria-hidden="true"></i>
                {t('football.matches.assignTeams.clearHome', 'Clear home team')}
              </button>
            )}
          </div>

          <div className="assign-teams-dialog__field">
            <label htmlFor="assign-teams-away" className="assign-teams-dialog__label">
              {t('football.matches.awayTeamLabel', 'Away Team')}
            </label>
            <SearchableInfiniteDropdown
              placeholder={t('football.matches.assignTeams.awayPlaceholder', 'Select away team (optional)')}
              value={awayTeamId}
              onChange={(value: string) => setAwayTeamId(value)}
              onSearch={searchAway}
              searchPlaceholder={t('common.search', 'Search...')}
              emptyMessage={t('football.matches.assignTeams.noTeams', 'No teams found')}
              disabled={!canEdit || saving}
              loadInitialDataOnMount={true}
            />
            {awayTeamId && (
              <button
                type="button"
                className="assign-teams-dialog__clear"
                onClick={() => setAwayTeamId('')}
                disabled={!canEdit || saving}
              >
                <i className="fas fa-times-circle" aria-hidden="true"></i>
                {t('football.matches.assignTeams.clearAway', 'Clear away team')}
              </button>
            )}
          </div>
        </div>

        <footer className="assign-teams-dialog__footer">
          <button
            type="button"
            className="assign-teams-dialog__btn assign-teams-dialog__btn--ghost"
            onClick={onClose}
            disabled={saving}
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            className="assign-teams-dialog__btn assign-teams-dialog__btn--primary"
            onClick={handleSave}
            disabled={!canSave}
          >
            {saving ? (
              <>
                <i className="fas fa-spinner fa-spin" aria-hidden="true"></i>
                {t('common.saving', 'Saving...')}
              </>
            ) : (
              <>
                <i className="fas fa-check" aria-hidden="true"></i>
                {t('football.matches.assignTeams.save', 'Save teams')}
              </>
            )}
          </button>
        </footer>
      </div>
    </div>
  );
};

export default FootballAssignTeamsDialog;
