import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { 
  floorballSeasonService, 
  type FloorballSeasonDto 
} from '../../../../../api/floorball/floorballSeasonService';

export const useSeasonsManagement = () => {
  const { t } = useTranslation();
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [operationLoading, setOperationLoading] = useState<string | null>(null);

  // Modal states
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  
  // Selected season for edit/delete operations
  const [selectedSeason, setSelectedSeason] = useState<FloorballSeasonDto | null>(null);

  // Filter states
  const [showActiveOnly, setShowActiveOnly] = useState(false);
  const [divisionFilter, setDivisionFilter] = useState<string>('all');

  const parseApiError = (error: any): string => {
    // Handle network errors
    if (error.message?.includes('Failed to fetch') || error.message?.includes('NetworkError')) {
      return t('floorball.seasons.errors.networkError', 'Network error. Please check your connection and try again.');
    }

    // Handle HTTP errors with specific status codes
    if (error.message?.includes('HTTP 400')) {
      return t('floorball.seasons.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    
    if (error.message?.includes('HTTP 404')) {
      return t('floorball.seasons.errors.notFound', 'Season not found. It may have been deleted.');
    }
    
    if (error.message?.includes('HTTP 409')) {
      return t('floorball.seasons.errors.conflictError', 'Operation conflicts with current data.');
    }
    
    if (error.message?.includes('HTTP 500')) {
      return t('floorball.seasons.errors.serverError', 'Server error. Please try again later.');
    }

    // Handle specific business logic errors
    const errorMessage = error.message || '';
    
    if (errorMessage.includes('Cannot activate a completed season')) {
      return t('floorball.seasons.errors.cannotActivateCompleted', 'Cannot activate a completed season.');
    }
    
    if (errorMessage.includes('Cannot update a completed season')) {
      return t('floorball.seasons.errors.cannotUpdateCompleted', 'Cannot update a completed season.');
    }
    
    if (errorMessage.includes('overlapping dates') || errorMessage.includes('overlaps with')) {
      return t('floorball.seasons.errors.overlappingDates', 'A season already exists that overlaps with the specified dates.');
    }

    // Default error message
    return error.message || t('floorball.seasons.errors.operationFailed', 'Operation failed. Please try again.');
  };

  const loadSeasons = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const result = showActiveOnly 
        ? await floorballSeasonService.getActive()
        : await floorballSeasonService.getAll();
      
      setSeasons(result.data || []);
    } catch (err) {
      setError(parseApiError(err));
      console.error('Error loading seasons:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateSeason = async (seasonData: any) => {
    try {
      await floorballSeasonService.create(seasonData);
      setShowCreateModal(false);
      await loadSeasons();
    } catch (err) {
      console.error('Error creating season:', err);
      throw err;
    }
  };

  const handleEditSeason = async (seasonData: any) => {
    if (!selectedSeason) return;
    
    try {
      await floorballSeasonService.update(selectedSeason.id, seasonData);
      setShowEditModal(false);
      setSelectedSeason(null);
      await loadSeasons();
    } catch (err) {
      console.error('Error updating season:', err);
      throw err;
    }
  };

  const handleDeleteSeason = async () => {
    if (!selectedSeason) return;
    
    try {
      await floorballSeasonService.delete(selectedSeason.id);
      setShowDeleteModal(false);
      setSelectedSeason(null);
      await loadSeasons();
    } catch (err) {
      console.error('Error deleting season:', err);
      throw err;
    }
  };

  const handleActivateToggle = async (season: FloorballSeasonDto) => {
    try {
      setOperationLoading(season.id);
      setError(null);
      
      if (season.isActive) {
        await floorballSeasonService.deactivate(season.id);
      } else {
        await floorballSeasonService.activate(season.id);
      }
      await loadSeasons();
    } catch (err) {
      console.error('Error toggling season activation:', err);
      setError(parseApiError(err));
    } finally {
      setOperationLoading(null);
    }
  };

  const handleCompleteSeason = async (season: FloorballSeasonDto) => {
    try {
      setOperationLoading(season.id);
      setError(null);
      
      await floorballSeasonService.complete(season.id);
      await loadSeasons();
    } catch (err) {
      console.error('Error completing season:', err);
      setError(parseApiError(err));
    } finally {
      setOperationLoading(null);
    }
  };

  const openEditModal = (season: FloorballSeasonDto) => {
    setSelectedSeason(season);
    setShowEditModal(true);
  };

  const openDeleteModal = (season: FloorballSeasonDto) => {
    setSelectedSeason(season);
    setShowDeleteModal(true);
  };

  const closeModals = () => {
    setShowCreateModal(false);
    setShowEditModal(false);
    setShowDeleteModal(false);
    setSelectedSeason(null);
  };

  const handleShowActiveOnlyChange = (value: boolean) => {
    setShowActiveOnly(value);
    // Trigger reload when filter changes
    if (value !== showActiveOnly) {
      setTimeout(loadSeasons, 0);
    }
  };

  // Filter seasons based on current filters
  const filteredSeasons = seasons.filter(season => {
    if (divisionFilter !== 'all' && season.division !== divisionFilter) {
      return false;
    }
    return true;
  });

  const uniqueDivisions = [...new Set(seasons.map(s => s.division))].sort();

  useEffect(() => {
    loadSeasons();
  }, []);

  return {
    // Data
    seasons: filteredSeasons,
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
  };
}; 