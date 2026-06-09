import { useCallback, useEffect, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { floorballTeamNameSearchService } from '../../api/floorball/floorballTeamNameSearchService';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import SearchableInfiniteDropdown from '../SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import './AssignTeamsDialog.scss';

interface AssignTeamsDialogProps {
  isOpen: boolean;
  match: FloorballMatchDto;
  onClose: () => void;
  /**
   * Invoked with the updated match after a successful save. Parents typically use this to
   * refresh local state (e.g. the manage-match page, the tournament bracket data).
   */
  onSaved: (updatedMatch: FloorballMatchDto) => void;
}

/**
 * Lightweight dialog that lets an admin assign or change the participating teams on a
 * `Scheduled` / `Postponed` match. Submitting routes through the backend's AssignMatchTeams
 * endpoint, which also handles playoff propagation (i.e. updating any downstream "winner of"
 * placeholder slot in the bracket when the source match is still unplayed).
 *
 * The dialog is read-only when the match is in any other status — the backend will reject
 * the update anyway and the user should not be encouraged to try.
 */
const AssignTeamsDialog = ({
  isOpen,
  match,
  onClose,
  onSaved,
}: AssignTeamsDialogProps): ReactElement | null => {
  const { t } = useTranslation();
  const [homeTeamId, setHomeTeamId] = useState<string>(match.homeTeamId ?? '');
  const [awayTeamId, setAwayTeamId] = useState<string>(match.awayTeamId ?? '');
  const [initialHomeOptions, setInitialHomeOptions] = useState<{ id: string; name: string }[]>([]);
  const [initialAwayOptions, setInitialAwayOptions] = useState<{ id: string; name: string }[]>([]);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Reset local state every time the dialog is opened so navigating between different
  // matches doesn't leak the previous selection. We also re-prime the cached initial
  // options so the dropdown shows the currently-saved team name immediately (without
  // requiring the operator to type a search).
  useEffect(() => {
    if (!isOpen) return;
    setHomeTeamId(match.homeTeamId ?? '');
    setAwayTeamId(match.awayTeamId ?? '');
    setError(null);

    const loadInitialOptions = async (): Promise<void> => {
      try {
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
      } catch (err: unknown) {
        // Pre-seeding the chips is purely cosmetic — never block opening the dialog.
        console.warn('Failed to seed AssignTeamsDialog initial options', err);
      }
    };
    void loadInitialOptions();
  }, [isOpen, match.homeTeamId, match.homeTeamName, match.awayTeamId, match.awayTeamName]);

  const searchTeamsWith = useCallback(
    async (initialOptions: { id: string; name: string }[], query: string, page: number) => {
      const result = await floorballTeamNameSearchService.searchTeams(query, page);
      if (page !== 1 || initialOptions.length === 0) {
        return result;
      }
      const trimmed: string = query.trim().toLowerCase();
      const filteredInitial = trimmed
        ? initialOptions.filter((opt) => opt.name.toLowerCase().includes(trimmed))
        : initialOptions;
      // Avoid duplicates: if the same team came back in `result.data`, prefer the entry
      // already pre-seeded by the parent (no perceived flicker).
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
      // The service throws on non-2xx and on `success: false` responses, joining any
      // backend-provided `errors` strings into the thrown message — so the catch below
      // is the canonical error path. The happy path here always has `data` populated.
      const response = await floorballMatchService.assignTeams(match.id, {
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
        : t('floorball.matches.assignTeams.genericError', 'Joukkueiden tallennus epäonnistui.');
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
            {t('floorball.matches.assignTeams.title', 'Aseta joukkueet')}
          </h2>
          <button
            type="button"
            className="assign-teams-dialog__close"
            onClick={onClose}
            aria-label={t('common.close', 'Sulje')}
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
                'floorball.matches.assignTeams.notEditable',
                'Joukkueita voi muuttaa vain kun ottelu on Aikataulutettu- tai Siirretty-tilassa.'
              )}
            </div>
          )}

          <p className="assign-teams-dialog__hint">
            {t(
              'floorball.matches.assignTeams.hint',
              'Jätä joukkue valitsematta jos haluat palauttaa sen TBD-tilaan. Playoff-otteluiden muutokset välittyvät automaattisesti seuraavaan otteluun mikäli sen syöttävä ottelu ei ole vielä päättynyt.'
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
              {t('floorball.matches.assignTeams.sameTeam', 'Koti- ja vierasjoukkue eivät voi olla samat.')}
            </div>
          )}

          <div className="assign-teams-dialog__field">
            <label htmlFor="assign-teams-home" className="assign-teams-dialog__label">
              {t('floorball.matches.homeTeamLabel', 'Home Team')}
            </label>
            <SearchableInfiniteDropdown
              placeholder={t('floorball.matches.assignTeams.homePlaceholder', 'Valitse kotijoukkue (valinnainen)')}
              value={homeTeamId}
              onChange={(value: string) => setHomeTeamId(value)}
              onSearch={searchHome}
              searchPlaceholder={t('common.search', 'Hae...')}
              emptyMessage={t('floorball.matches.assignTeams.noTeams', 'Joukkueita ei löytynyt')}
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
                {t('floorball.matches.assignTeams.clearHome', 'Tyhjennä kotijoukkue')}
              </button>
            )}
          </div>

          <div className="assign-teams-dialog__field">
            <label htmlFor="assign-teams-away" className="assign-teams-dialog__label">
              {t('floorball.matches.awayTeamLabel', 'Away Team')}
            </label>
            <SearchableInfiniteDropdown
              placeholder={t('floorball.matches.assignTeams.awayPlaceholder', 'Valitse vierasjoukkue (valinnainen)')}
              value={awayTeamId}
              onChange={(value: string) => setAwayTeamId(value)}
              onSearch={searchAway}
              searchPlaceholder={t('common.search', 'Hae...')}
              emptyMessage={t('floorball.matches.assignTeams.noTeams', 'Joukkueita ei löytynyt')}
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
                {t('floorball.matches.assignTeams.clearAway', 'Tyhjennä vierasjoukkue')}
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
            {t('common.cancel', 'Peruuta')}
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
                {t('common.saving', 'Tallennetaan...')}
              </>
            ) : (
              <>
                <i className="fas fa-check" aria-hidden="true"></i>
                {t('floorball.matches.assignTeams.save', 'Tallenna joukkueet')}
              </>
            )}
          </button>
        </footer>
      </div>
    </div>
  );
};

export default AssignTeamsDialog;
