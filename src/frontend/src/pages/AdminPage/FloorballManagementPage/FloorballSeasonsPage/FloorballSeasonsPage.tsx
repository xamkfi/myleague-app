import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import './FloorballSeasonsPage.scss';
import { useSeasonsManagement } from './hooks/useSeasonsManagement';
import { SeasonsPageHeader } from './components/SeasonsPageHeader';
import { SeasonsFilters } from './components/SeasonsFilters';
import { ErrorMessage } from './components/ErrorMessage';
import { LoadingState } from './components/LoadingState';
import { SeasonsContent } from './components/SeasonsContent';
import BackButton from '../../../../components/BackButton/BackButton';
import { ConfirmDeleteModal } from './components/ConfirmDeleteModal';
import { EditSeasonModal } from './components/EditSeasonModal';
import { CreateSeasonModal } from './components/CreateSeasonModal';

const FloorballSeasonsPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  
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
    showCreateModal,
    showEditModal,
    showDeleteModal,
    
    // Actions
    setShowCreateModal,
    setDivisionFilter,
    handleShowActiveOnlyChange,
    handleCreateSeason,
    handleEditSeason,
    handleDeleteSeason,
    handleActivateToggle,
    handleCompleteSeason,
    openEditModal,
    openDeleteModal,
    closeModals,
    loadSeasons
  } = useSeasonsManagement();

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

        {/* Back button */}
        <BackButton 
          to="/admin/floorball" 
          text={t('common.back', 'Back to Floorball Management')} 
        />

        <SeasonsPageHeader
          seasonsCount={seasons.length}
          onCreateSeason={() => setShowCreateModal(true)}
        />

        {error && <ErrorMessage message={error} />}

        <SeasonsFilters
          showActiveOnly={showActiveOnly}
          onShowActiveOnlyChange={handleShowActiveOnlyChange}
          divisionFilter={divisionFilter}
          onDivisionFilterChange={setDivisionFilter}
          uniqueDivisions={uniqueDivisions}
        />

        <SeasonsContent
          seasons={seasons}
          onEdit={openEditModal}
          onDelete={openDeleteModal}
          onActivateToggle={handleActivateToggle}
          onComplete={handleCompleteSeason}
          operationLoading={operationLoading}
        />

        {/* Modals */}
        {showCreateModal && (
          <CreateSeasonModal
            onSave={handleCreateSeason}
            onClose={closeModals}
          />
        )}

        {showEditModal && selectedSeason && (
          <EditSeasonModal
            season={selectedSeason}
            onSave={handleEditSeason}
            onClose={closeModals}
            onTeamsChanged={loadSeasons}
          />
        )}

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