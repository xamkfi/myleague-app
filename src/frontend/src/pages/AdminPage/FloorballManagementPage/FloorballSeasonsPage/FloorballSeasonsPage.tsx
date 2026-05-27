import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import '../../../../styles/AdminTable.scss';
import './FloorballSeasonsPage.scss';
import { useSeasonsManagement } from './hooks/useSeasonsManagement';
import { SeasonsPageHeader } from './components/SeasonsPageHeader';
import { SeasonsFilters } from './components/SeasonsFilters';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { LoadingState } from './components/LoadingState';
import { SeasonsContent } from './components/SeasonsContent';
import { ConfirmDeleteModal } from './components/ConfirmDeleteModal';

const FloorballSeasonsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const {
    // Data
    seasons,
    loading,
    error,
    operationLoading,
    selectedSeason,
    uniqueDivisions,

    // Filter states
    showActiveOnly,
    divisionFilter,

    // Modal states
    showDeleteModal,

    // Actions
    setDivisionFilter,
    handleShowActiveOnlyChange,
    handleDeleteSeason,
    handleActivateToggle,
    handleCompleteSeason,
    openDeleteModal,
    closeModals,
  } = useSeasonsManagement();

  /**
   * Row-click navigointi keskitetään tähän funktioon.
   * Näin SeasonsTable saa vain selkeän onEdit-callbackin, eikä tiedä reittipoluista liikaa.
   */
  const handleEditSeason = (seasonId: string): void => {
    navigate(`/admin/floorball/seasons/${seasonId}/edit`);
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.seasons.title', 'Manage Seasons')}>
        <LoadingState />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.seasons.title', 'Manage Seasons')}>
      <div className="floorball-seasons-container">
        <SeasonsPageHeader
          seasonsCount={seasons.length}
          onCreateSeason={() => navigate('/admin/floorball/seasons/create')}
          onManageMatches={() => navigate('/admin/floorball/seasons/matches')}
        />

        <ErrorPopup message={error} />

        <SeasonsFilters
          showActiveOnly={showActiveOnly}
          onShowActiveOnlyChange={handleShowActiveOnlyChange}
          divisionFilter={divisionFilter}
          onDivisionFilterChange={setDivisionFilter}
          uniqueDivisions={uniqueDivisions}
        />

        {/*
          BulkActionsBar poistettu, koska season-listauksessa ei enää käytetä multiselectiä.
          Samalla poistettiin selectedIds-state sekä bulk activate/deactivate/delete -funktiot.
        */}
        <div className="admin-table__wrapper">
          <SeasonsContent
            seasons={seasons}
            onEdit={(season) => handleEditSeason(season.id)}
            onDelete={openDeleteModal}
            onActivateToggle={handleActivateToggle}
            onComplete={handleCompleteSeason}
            operationLoading={operationLoading}
          />
        </div>

        {showDeleteModal && selectedSeason && (
          <ConfirmDeleteModal
            season={selectedSeason}
            onConfirm={handleDeleteSeason}
            onCancel={closeModals}
          />
        )}
      </div>
    </PageTemplate>
  );
};

export default FloorballSeasonsPage;