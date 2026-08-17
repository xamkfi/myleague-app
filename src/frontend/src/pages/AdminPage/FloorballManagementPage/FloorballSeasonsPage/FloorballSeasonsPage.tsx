import { useState } from 'react';
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
import { ConfirmCompleteSeasonModal } from './components/ConfirmCompleteSeasonModal';
import type { FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';

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
    categoryFilter,

    // Modal states
    showDeleteModal,

    // Actions
    setDivisionFilter,
    setCategoryFilter,
    handleShowActiveOnlyChange,
    handleDeleteSeason,
    handleActivateToggle,
    handleCompleteSeason,
    openDeleteModal,
    closeModals,
  } = useSeasonsManagement();

  /**
   * Tämä state pitää muistissa kauden, jota käyttäjä yrittää päättää.
   * Jos arvo on null, complete-vahvistusmodaalia ei näytetä.
   */
  const [seasonToComplete, setSeasonToComplete] =
    useState<FloorballSeasonDto | null>(null);

  /**
   * Keskitetty edit-navigointi.
   * Näin SeasonsContent ja SeasonsTable eivät tiedä reittirakenteesta liikaa.
   */
  const handleEditSeason = (seasonId: string): void => {
    navigate(`/admin/floorball/seasons/${seasonId}/edit`);
  };

  /**
   * Avaa vahvistusmodaalin seasonin päättämistä varten.
   * Tässä tehdään myös kevyt tarkistus, ettei päättynyttä tai epäaktiivista kautta
   * yritetä päättää uudestaan käyttöliittymästä käsin.
   */
  const openCompleteSeasonModal = (season: FloorballSeasonDto): void => {
    if (season.isCompleted || !season.isActive) return;

    setSeasonToComplete(season);
  };

  /**
   * Sulkee complete-vahvistusmodaalin.
   * Jos operaatio on juuri käynnissä, sulkeminen estetään,
   * jotta käyttäjä ei katkaise käyttöliittymän tilaa kesken tallennuksen.
   */
  const closeCompleteSeasonModal = (): void => {
    if (operationLoading === seasonToComplete?.id) return;

    setSeasonToComplete(null);
  };

  /**
   * Suorittaa varsinaisen kauden päättämisen vasta käyttäjän vahvistuksen jälkeen.
   * Vanhaa handleCompleteSeason-funktiota ei poisteta, vaan sitä käytetään turvallisemmin
   * modaalin confirm-painikkeen takana.
   */
  const confirmCompleteSeason = async (): Promise<void> => {
    if (!seasonToComplete) return;

    try {
      await Promise.resolve(handleCompleteSeason(seasonToComplete));
      setSeasonToComplete(null);
    } catch (err) {
      /**
       * Jos hookin sisäinen complete-toiminto joskus heittää virheen,
       * modaali jätetään auki, jotta käyttäjä voi yrittää uudelleen tai peruuttaa.
       */
      console.error('Complete season failed:', err);
    }
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
          categoryFilter={categoryFilter}
          onCategoryFilterChange={setCategoryFilter}
        />

        {/*
          BulkActionsBar on poistettu, koska season-listauksessa ei enää käytetä multiselectiä.
          Complete-toiminto puolestaan ohjataan nyt vahvistusmodaalin kautta.
        */}
        <div className="admin-table__wrapper">
          <SeasonsContent
            seasons={seasons}
            onEdit={(season) => handleEditSeason(season.id)}
            onDelete={openDeleteModal}
            onActivateToggle={handleActivateToggle}
            onComplete={openCompleteSeasonModal}
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

        {seasonToComplete && (
          <ConfirmCompleteSeasonModal
            season={seasonToComplete}
            loading={operationLoading === seasonToComplete.id}
            onConfirm={confirmCompleteSeason}
            onCancel={closeCompleteSeasonModal}
          />
        )}
      </div>
    </PageTemplate>
  );
};

export default FloorballSeasonsPage;