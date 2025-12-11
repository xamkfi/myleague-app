import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { 
  floorballSeasonService, 
  type FloorballSeasonDto,
  type CreateFloorballSeasonRequest,
  type UpdateFloorballSeasonRequest
} from '../../../../../api/floorball/floorballSeasonService';
import { divisionService } from '../../../../../api/common/divisionService';
import type { DivisionType } from '../../../../../types/common/divisionType';

export const useSeasonsManagement = () => {
  const { t } = useTranslation();
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [divisions, setDivisions] = useState<DivisionType[]>([]);
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

  const parseApiError = useCallback((error: unknown): string => {
    const errorMessage = error instanceof Error ? error.message : String(error);
    
    // Handle network errors
    if (errorMessage.includes('Failed to fetch') || errorMessage.includes('NetworkError')) {
      return t('floorball.seasons.errors.networkError', 'Network error. Please check your connection and try again.');
    }

    // Handle HTTP errors with specific status codes
    if (errorMessage.includes('HTTP 400')) {
      return t('floorball.seasons.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    
    if (errorMessage.includes('HTTP 404')) {
      return t('floorball.seasons.errors.notFound', 'Season not found. It may have been deleted.');
    }
    
    if (errorMessage.includes('HTTP 409')) {
      return t('floorball.seasons.errors.conflictError', 'Operation conflicts with current data.');
    }
    
    if (errorMessage.includes('HTTP 500')) {
      return t('floorball.seasons.errors.serverError', 'Server error. Please try again later.');
    }

    // Handle specific business logic errors
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
    return errorMessage || t('floorball.seasons.errors.operationFailed', 'Operation failed. Please try again.');
  }, [t]);

  const loadSeasons = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Always load all seasons, we'll filter them locally
      const result = await floorballSeasonService.getAll();
      setSeasons(result.data || []);
    } catch (err) {
      setError(parseApiError(err));
      console.error('Error loading seasons:', err);
    } finally {
      setLoading(false);
    }
  }, [parseApiError]);

  const loadDivisions = useCallback(async () => {
    try {
      const result = await divisionService.getAll();
      setDivisions(result.data || []);
    } catch (err) {
      console.error('Error loading divisions:', err);
      setDivisions([]);
    }
  }, []);

  const handleCreateSeason = async (seasonData: CreateFloorballSeasonRequest, shouldActivate: boolean) => {
    try {
      const created = await floorballSeasonService.create(seasonData);
      if (shouldActivate && created?.data?.id) {
        try {
          await floorballSeasonService.activate(created.data.id);
        } catch (activateErr) {
          console.error('Error activating newly created season:', activateErr);
          // Let UI still proceed, the user can toggle later
        }
      }
      setShowCreateModal(false);
      await loadSeasons();
    } catch (err) {
      console.error('Error creating season:', err);
      throw err;
    }
  };

  const handleEditSeason = async (seasonData: UpdateFloorballSeasonRequest) => {
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

      const result = season.isActive
        ? await floorballSeasonService.deactivate(season.id)
        : await floorballSeasonService.activate(season.id);

      // Update only the toggled season locally to avoid full reload
      const updated = result?.data;
      setSeasons(prev => prev.map(s => {
        if (s.id !== season.id) return s;
        if (updated) {
          return { ...s, ...updated };
        }
        // Fallback: toggle isActive if API did not return data
        return { ...s, isActive: !s.isActive };
      }));
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
  };

  // Filter seasons based on current filters
  const filteredSeasons = seasons.filter(season => {
    // Filter by active status
    if (showActiveOnly && !season.isActive) {
      return false;
    }
    
    // Filter by division
    if (divisionFilter !== 'all') {
      // Check if any of the season's divisions match the filter
      const hasMatchingDivision = season.seasonDivisions?.some(seasonDivision => {
        const division = divisions.find(d => d.id === seasonDivision.divisionId);
        const divisionName = division?.name || seasonDivision.divisionId;
        return divisionName === divisionFilter;
      });
      if (!hasMatchingDivision) {
        return false;
      }
    }
    
    return true;
  });

  // Get unique division names from the seasons' division IDs
  const uniqueDivisions = [...new Set(
    seasons.flatMap(season => 
      (season.seasonDivisions || []).map(seasonDivision => {
        const division = divisions.find(d => d.id === seasonDivision.divisionId);
        return division?.name || seasonDivision.divisionId; // Fallback to ID if division not found
      })
    )
  )].sort();

  useEffect(() => {
    loadDivisions();
    loadSeasons();
  }, [loadDivisions, loadSeasons]);

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