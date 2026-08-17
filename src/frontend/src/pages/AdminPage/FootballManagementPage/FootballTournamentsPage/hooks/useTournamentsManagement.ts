import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { footballTournamentService } from '../../../../../api/football/footballTournamentService';
import type { FootballTournamentDto } from '../../../../../types/football/tournamentTypes';

export type TournamentStatusFilter = 'all' | 'Draft' | 'GroupStage' | 'PlayoffStage' | 'Completed' | 'Cancelled';

export const useTournamentsManagement = () => {
  const { t } = useTranslation();
  const [tournaments, setTournaments] = useState<FootballTournamentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [showOngoingOnly, setShowOngoingOnly] = useState(false);
  const [statusFilter, setStatusFilter] = useState<TournamentStatusFilter>('all');
  const [categoryFilter, setCategoryFilter] = useState<string[]>([]);

  const parseApiError = useCallback((err: unknown): string => {
    const msg = err instanceof Error ? err.message : String(err);
    if (msg.includes('Failed to fetch') || msg.includes('NetworkError')) {
      return t('football.tournaments.errors.networkError', 'Network error. Please check your connection and try again.');
    }
    if (msg.includes('HTTP 400')) {
      return t('football.tournaments.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    if (msg.includes('HTTP 404')) {
      return t('football.tournaments.errors.notFound', 'Tournament not found. It may have been deleted.');
    }
    if (msg.includes('HTTP 500')) {
      return t('football.tournaments.errors.serverError', 'Server error. Please try again later.');
    }
    return msg || t('football.tournaments.errors.operationFailed', 'Operation failed. Please try again.');
  }, [t]);

  // `silent: true` keeps the page-level loading state untouched so the parent doesn't swap
  // its content for <LoadingState />. Background refreshes after lifecycle actions should
  // always use silent mode.
  const loadTournaments = useCallback(async (options?: { silent?: boolean }) => {
    const silent = options?.silent === true;
    try {
      if (!silent) {
        setLoading(true);
      }
      setError(null);
      const result = await footballTournamentService.getAll();
      setTournaments(result.data ?? []);
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      if (!silent) {
        setLoading(false);
      }
    }
  }, [parseApiError]);

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
    // Team category filter (multi-select; empty selection = show all)
    if (categoryFilter.length > 0 && !categoryFilter.includes(tournament.teamCategory ?? '')) {
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
    showOngoingOnly,
    statusFilter,
    categoryFilter,
    uniqueStatuses,
    setShowOngoingOnly,
    setStatusFilter,
    setCategoryFilter,
    loadTournaments,
  };
};
