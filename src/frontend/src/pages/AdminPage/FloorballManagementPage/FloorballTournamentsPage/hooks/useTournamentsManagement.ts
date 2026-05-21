import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballTournamentService } from '../../../../../api/floorball/floorballTournamentService';
import type { FloorballTournamentDto } from '../../../../../types/floorball/tournamentTypes';

export type TournamentStatusFilter = 'all' | 'Draft' | 'GroupStage' | 'PlayoffStage' | 'Completed' | 'Cancelled';

export const useTournamentsManagement = () => {
  const { t } = useTranslation();
  const [tournaments, setTournaments] = useState<FloorballTournamentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [operationLoading, setOperationLoading] = useState<string | null>(null);

  const [showOngoingOnly, setShowOngoingOnly] = useState(false);
  const [statusFilter, setStatusFilter] = useState<TournamentStatusFilter>('all');

  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedTournament, setSelectedTournament] = useState<FloorballTournamentDto | null>(null);

  const parseApiError = useCallback((err: unknown): string => {
    const msg = err instanceof Error ? err.message : String(err);
    if (msg.includes('Failed to fetch') || msg.includes('NetworkError')) {
      return t('floorball.tournaments.errors.networkError', 'Network error. Please check your connection and try again.');
    }
    if (msg.includes('HTTP 400')) {
      return t('floorball.tournaments.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    if (msg.includes('HTTP 404')) {
      return t('floorball.tournaments.errors.notFound', 'Tournament not found. It may have been deleted.');
    }
    if (msg.includes('HTTP 500')) {
      return t('floorball.tournaments.errors.serverError', 'Server error. Please try again later.');
    }
    return msg || t('floorball.tournaments.errors.operationFailed', 'Operation failed. Please try again.');
  }, [t]);

  // `silent: true` keeps the page-level loading state untouched so the parent doesn't swap
  // its content for <LoadingState />. That matters when a modal (e.g. TournamentImportModal)
  // is open during the refresh — flipping `loading` would unmount the whole modal mid-import
  // and the user would lose the progress log. Background refreshes after imports / lifecycle
  // actions should always use silent mode.
  const loadTournaments = useCallback(async (options?: { silent?: boolean }) => {
    const silent = options?.silent === true;
    try {
      if (!silent) {
        setLoading(true);
      }
      setError(null);
      const result = await floorballTournamentService.getAll();
      setTournaments(result.data ?? []);
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      if (!silent) {
        setLoading(false);
      }
    }
  }, [parseApiError]);

  const handleDeleteTournament = async () => {
    if (!selectedTournament) return;
    try {
      setOperationLoading(selectedTournament.id);
      await floorballTournamentService.delete(selectedTournament.id);
      setShowDeleteModal(false);
      setSelectedTournament(null);
      await loadTournaments();
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setOperationLoading(null);
    }
  };

  type LifecycleAction = 'startGroupStage' | 'startPlayoffStage' | 'complete' | 'cancel';

  const handleLifecycleAction = async (tournament: FloorballTournamentDto, action: LifecycleAction) => {
    try {
      setOperationLoading(tournament.id);
      setError(null);
      await floorballTournamentService[action](tournament.id);
      await loadTournaments();
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setOperationLoading(null);
    }
  };

  const openDeleteModal = (tournament: FloorballTournamentDto) => {
    setSelectedTournament(tournament);
    setShowDeleteModal(true);
  };

  const closeModals = () => {
    setShowDeleteModal(false);
    setSelectedTournament(null);
  };

  const filteredTournaments = tournaments.filter((tournament) => {
    if (showOngoingOnly) {
      const status = tournament.tournamentStatus;
      if (status === 'Completed' || status === 'Draft' || status === 'Cancelled') {
        return false;
      }
    }
    if (statusFilter !== 'all' && tournament.tournamentStatus !== statusFilter) {
      return false;
    }
    return true;
  });

  const uniqueStatuses = [...new Set(tournaments.map((t) => t.tournamentStatus))].sort();

  useEffect(() => {
    loadTournaments();
  }, [loadTournaments]);

  return {
    tournaments: filteredTournaments,
    allTournamentsCount: tournaments.length,
    loading,
    error,
    operationLoading,
    selectedTournament,
    showOngoingOnly,
    statusFilter,
    uniqueStatuses,
    showDeleteModal,
    setShowOngoingOnly,
    setStatusFilter,
    handleDeleteTournament,
    handleLifecycleAction,
    openDeleteModal,
    closeModals,
    loadTournaments,
  };
};
