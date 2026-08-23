import { useEffect, useMemo, useState } from 'react';
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
  const [operationLoading, setOperationLoading] = useState<string | null>(null);
  const [seasonToComplete, setSeasonToComplete] = useState<HockeySeasonDto | null>(null);

  useEffect(() => {
    const load = async (): Promise<void> => {
      try {
        setLoading(true);
        setSeasons(await hockeySeasonService.getAll());
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : t('hockey.seasons.errors.loadFailed', 'Failed to load seasons'));
      } finally {
        setLoading(false);
      }
    };
    void load();
  }, [t]);

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
      return true;
    });
  }, [seasons, showActiveOnly, divisionFilter]);

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

  if (loading) {
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
        />
        <ErrorPopup message={error} />
        <SeasonsFilters
          showActiveOnly={showActiveOnly}
          onShowActiveOnlyChange={setShowActiveOnly}
          divisionFilter={divisionFilter}
          onDivisionFilterChange={setDivisionFilter}
          uniqueDivisions={uniqueDivisions}
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
            operationLoading={operationLoading}
          />
        </div>
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
      </div>
    </PageTemplate>
  );
}

export default HockeySeasonsPage;
