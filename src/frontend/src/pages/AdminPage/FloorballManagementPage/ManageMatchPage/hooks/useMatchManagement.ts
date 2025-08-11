import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import type {
  CreateFloorballMatchRequest,
  ChangeMatchSeasonRequest,
  ChangeMatchTeamsRequest,
  ChangeMatchVenueRequest,
  ChangeMatchDateTimeRequest,
  FloorballMatchDto
} from '../../../../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../../../../types/floorball/floorballTypes';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import { floorballMatchEventService } from '../../../../../api/floorball/floorballMatchEventService';

interface UseMatchManagementParams {
  setMatches: React.Dispatch<React.SetStateAction<FloorballMatchDto[]>>;
}

export function useMatchManagement({ setMatches }: UseMatchManagementParams) {
  const navigate = useNavigate();
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const [showForm, setShowForm] = useState(false);
  const [formMode, setFormMode] = useState<'create' | 'edit'>('create');
  const [editMatch, setEditMatch] = useState<FloorballMatchDto | undefined>(undefined);

  const handleLiveMatch = useCallback((match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  }, [navigate]);

  const handleEditMatch = useCallback((match: FloorballMatchDto) => {
    setEditMatch(match);
    setFormMode('edit');
    setShowForm(true);
  }, []);

  const handleCloseForm = useCallback(() => {
    setShowForm(false);
    setEditMatch(undefined);
    setFormMode('create');
  }, []);

  const handleFormSubmit = useCallback(async (
    matchData: CreateFloorballMatchRequest |
                ChangeMatchSeasonRequest |
                ChangeMatchTeamsRequest |
                ChangeMatchVenueRequest |
                ChangeMatchDateTimeRequest
  ) => {
    if (!editMatch) return;
    setActionLoading('edit');
    try {
      let response;
      if ('seasonId' in matchData && !('homeTeamId' in matchData)) {
        response = await floorballMatchService.changeSeason(editMatch.id, matchData.seasonId);
      } else if ('homeTeamId' in matchData && 'awayTeamId' in matchData) {
        response = await floorballMatchService.changeTeams(editMatch.id, matchData.homeTeamId, matchData.awayTeamId);
      } else if ('venue' in matchData) {
        response = await floorballMatchService.changeVenue(editMatch.id, matchData.venue);
      } else if ('scheduledDateTime' in matchData) {
        response = await floorballMatchService.changeDateTime(editMatch.id, matchData.scheduledDateTime);
      } else {
        throw new Error('Invalid update data');
      }
      if (response.success && response.data) {
        setMatches(prev => prev.map(m => m.id === editMatch.id ? response.data! : m));
        handleCloseForm();
      }
    } catch (error) {
      console.error('Error updating match:', error);
    } finally {
      setActionLoading(null);
    }
  }, [editMatch, handleCloseForm, setMatches]);

  const handleCancelMatch = useCallback(async (matchId: string) => {
    setActionLoading('cancelling');
    try {
      await floorballMatchEventService.cancelMatch(matchId);
      setMatches(prev => prev.map(m => m.id === matchId ? { ...m, status: FloorballMatchStatus.Cancelled } : m));
    } catch (error) {
      console.error('Error canceling match:', error);
    } finally {
      setActionLoading(null);
    }
  }, [setMatches]);

  return {
    actionLoading,
    showForm,
    formMode,
    editMatch,
    handleLiveMatch,
    handleEditMatch,
    handleCloseForm,
    handleFormSubmit,
    handleCancelMatch
  };
}