import { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import '../../../../styles/AdminTable.scss';
import './HockeySeasonsPage.scss';
import { SeasonsPageHeader } from './components/SeasonsPageHeader';
import { SeasonsFilters } from './components/SeasonsFilters';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { LoadingState } from './components/LoadingState';
import { SeasonsContent } from './components/SeasonsContent';
import { ConfirmCompleteSeasonModal } from './components/ConfirmCompleteSeasonModal';
import { ConfirmDeleteModal } from './components/ConfirmDeleteModal';
import { SeasonImportModal } from './components/SeasonImportModal';
import { hockeySeasonService } from '../../../../api/hockey/hockeySeasonService';
import type { HockeySeasonDto } from '../../../../types/hockey/hockeyTypes';

function HockeySeasonsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [seasons, setSeasons] = useState<HockeySeasonDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showActiveOnly, setShowActiveOnly] = useState(false);
  const [divisionFilter, setDivisionFilter] = useState('all');
  const [categoryFilter, setCategoryFilter] = useState<string[]>([]);
  const [operationLoading, setOperationLoading] = useState<string | null>(null);
  const [seasonToComplete, setSeasonToComplete] = useState<HockeySeasonDto | null>(null);
  const [seasonToDelete, setSeasonToDelete] = useState<HockeySeasonDto | null>(null);
  const [showImportModal, setShowImportModal] = useState<boolean>(false);

  const loadSeasons = useCallback(async (options?: { silent?: boolean }): Promise<void> => {
    try {
      if (!options?.silent) {
        setLoading(true);
      }
      setSeasons(await hockeySeasonService.getAll(undefined, true));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.seasons.errors.loadFailed', 'Failed to load seasons'));
    } finally {
      if (!options?.silent) {
        setLoading(false);
      }
    }
  }, [t]);

  useEffect(() => {
    void loadSeasons();
  }, [loadSeasons]);

  const uniqueDivisions = useMemo(() => {
    const names = new Set<string>();
    for (const season of seasons) {
      for (const division of season.divisions ?? []) {
        names.add(division.name);
      }
    }
    return [...names].sort((a, b) => a.localeCompare(b));
  }, [seasons]);

  const filtered = useMemo(() => {
    return seasons.filter((season) => {
      if (showActiveOnly && !season.isActive) {
        return false;
      }
      if (divisionFilter !== 'all' && !(season.divisions ?? []).some((division) => division.name === divisionFilter)) {
        return false;
      }
      if (categoryFilter.length > 0 && !categoryFilter.includes(season.teamCategory ?? '')) {
        return false;
      }
      return true;
    });
  }, [seasons, showActiveOnly, divisionFilter, categoryFilter]);

  const handleActivateToggle = async (season: HockeySeasonDto): Promise<void> => {
    setOperationLoading(season.id);
    try {
      const updated = season.isActive
        ? await hockeySeasonService.deactivate(season.id)
        : await hockeySeasonService.activate(season.id);
      setSeasons((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.seasons.errors.updateFailed', 'Failed to update season'));
    } finally {
      setOperationLoading(null);
    }
  };

  const confirmDeleteSeason = async (): Promise<void> => {
    if (!seasonToDelete) {
      return;
    }
    setOperationLoading(seasonToDelete.id);
    try {
      await hockeySeasonService.deleteSeason(seasonToDelete.id);
      setSeasons((prev) => prev.filter((item) => item.id !== seasonToDelete.id));
      setSeasonToDelete(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : t('hockey.seasons.errors.deleteFailed', 'Failed to delete season');
      if (message.includes('Cannot delete a season that has a match that has started')) {
        setError(t(
          'hockey.seasons.errors.hasMatches',
          'Cannot delete a season that has a match that has started, finished, or been cancelled.',
        ));
      } else {
        setError(message);
      }
    } finally {
      setOperationLoading(null);
    }
  };

  const confirmCompleteSeason = async (): Promise<void> => {
    if (!seasonToComplete) {
      return;
    }
    setOperationLoading(seasonToComplete.id);
    try {
      const updated = await hockeySeasonService.complete(seasonToComplete.id);
      setSeasons((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
      setSeasonToComplete(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.seasons.errors.completeFailed', 'Failed to complete season'));
    } finally {
      setOperationLoading(null);
    }
  };

  if (loading && !showImportModal) {
    return (
      <PageTemplate title={t('hockey.seasons.title', 'Manage Seasons')}>
        <LoadingState />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('hockey.seasons.title', 'Manage Seasons')}>
      <div className="floorball-seasons-container">
        <SeasonsPageHeader
          seasonsCount={seasons.length}
          onCreateSeason={() => navigate('/admin/hockey/seasons/create')}
          onManageMatches={() => navigate('/admin/hockey/seasons/matches')}
          onImportSeason={() => setShowImportModal(true)}
        />
        <ErrorPopup message={error} />
        <SeasonsFilters
          showActiveOnly={showActiveOnly}
          onShowActiveOnlyChange={setShowActiveOnly}
          divisionFilter={divisionFilter}
          onDivisionFilterChange={setDivisionFilter}
          uniqueDivisions={uniqueDivisions}
          categoryFilter={categoryFilter}
          onCategoryFilterChange={setCategoryFilter}
        />
        <div className="admin-table__wrapper">
          <SeasonsContent
            seasons={filtered}
            onEdit={(season) => navigate(`/admin/hockey/seasons/${season.id}/edit`)}
            onActivateToggle={(season) => void handleActivateToggle(season)}
            onComplete={(season) => {
              if (!season.isCompleted && season.isActive) {
                setSeasonToComplete(season);
              }
            }}
            onDelete={setSeasonToDelete}
            operationLoading={operationLoading}
          />
        </div>
        {seasonToDelete && (
          <ConfirmDeleteModal
            season={seasonToDelete}
            loading={operationLoading === seasonToDelete.id}
            onConfirm={confirmDeleteSeason}
            onCancel={() => {
              if (operationLoading !== seasonToDelete.id) {
                setSeasonToDelete(null);
              }
            }}
          />
        )}
        {seasonToComplete && (
          <ConfirmCompleteSeasonModal
            season={seasonToComplete}
            loading={operationLoading === seasonToComplete.id}
            onConfirm={confirmCompleteSeason}
            onCancel={() => {
              if (operationLoading !== seasonToComplete.id) {
                setSeasonToComplete(null);
              }
            }}
          />
        )}
        {showImportModal && (
          <SeasonImportModal
            onClose={() => setShowImportModal(false)}
            onImported={() => {
              void loadSeasons({ silent: true });
            }}
          />
        )}
      </div>
    </PageTemplate>
  );
}

export default HockeySeasonsPage;
