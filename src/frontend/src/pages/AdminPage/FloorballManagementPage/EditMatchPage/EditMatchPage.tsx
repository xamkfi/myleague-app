import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import type { 
  FloorballMatchDto,
  CreateFloorballMatchRequest,
  ChangeMatchSeasonRequest,
  ChangeMatchTeamsRequest,
  ChangeMatchVenueRequest,
  ChangeMatchDateTimeRequest
} from '../../../../types/floorball/floorballTypes';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballMatchEventService } from '../../../../api/floorball/floorballMatchEventService';
import MatchForm from '../MatchOverviewPage/Components/MatchForm/MatchForm';
import BackButton from '../../../../components/BackButton/BackButton';
import Navbar from '../../../../components/Navigation/Navbar';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './EditMatchPage.scss';
import '../MatchOverviewPage/MatchOverviewPage.scss';

const EditMatchPage = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [matchData, setMatchData] = useState<FloorballMatchDto | null>(null);
  const navigate = useNavigate();
  const { matchId } = useParams<{ matchId: string }>();

  useEffect(() => {
    if (!matchId) {
      setError('Match ID is missing');
      return;
    }

    const fetchMatchData = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await floorballMatchService.getById(matchId);
        if (response.success && response.data) {
          setMatchData(response.data);
        } else {
          setError('Failed to fetch match data');
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An unknown error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchMatchData();
  }, [matchId]);

  const handleCancelMatch = async (matchId: string) => {
    try {
      setLoading(true);
      setError(null);
      await floorballMatchEventService.cancelMatch(matchId);
      navigate('/admin/floorball/matches');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred during cancellation';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateMatch = async (updatedData: CreateFloorballMatchRequest) => {
    if (!matchData) {
      setError('Original match data is not available.');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const changes: Promise<unknown>[] = [];

      if (updatedData.seasonId !== matchData.seasonId) {
        changes.push(floorballMatchService.changeSeason(matchData.id, updatedData.seasonId));
      }
      
      if (updatedData.homeTeamId !== matchData.homeTeamId || updatedData.awayTeamId !== matchData.awayTeamId) {
        changes.push(floorballMatchService.changeTeams(matchData.id, updatedData.homeTeamId, updatedData.awayTeamId));
      }
      
      if (updatedData.venue !== matchData.venue) {
        changes.push(floorballMatchService.changeVenue(matchData.id, updatedData.venue || ''));
      }
      
      if (updatedData.scheduledDateTime !== matchData.scheduledDateTime) {
        changes.push(floorballMatchService.changeDateTime(matchData.id, updatedData.scheduledDateTime));
      }

      if (changes.length === 0) {
        setError('No changes were detected.');
        return;
      }

      await Promise.all(changes);

      navigate('/admin/floorball/matches');

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred during update';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleFormSubmit = (
    formData: CreateFloorballMatchRequest | ChangeMatchSeasonRequest | ChangeMatchTeamsRequest | ChangeMatchVenueRequest | ChangeMatchDateTimeRequest
  ) => {
    return handleUpdateMatch(formData as CreateFloorballMatchRequest);
  };

  const handleCancel = () => {
    navigate('/admin/floorball/matches');
  };

  if (loading) {
    return <div>Loading match data...</div>;
  }
  
  return (
    <div className="match-management">
      <Navbar />
      <div className="match-management__content edit-match-page">
        <div className="page-header">
          <div className="header-left">
            <BackButton to="/admin/floorball/matches" text="Back" />
          </div>
          <div className="header-center">
            <h1>Edit Match</h1>
          </div>
          <div className="header-right"></div>
        </div>

        <ErrorPopup message={error} />

        <div className="form-container">
          {matchData ? (
            <MatchForm
              mode="edit"
              initialData={matchData}
              onSubmit={handleFormSubmit}
              onCancel={handleCancel}
              onCancelMatch={handleCancelMatch}
              loading={loading}
            />
          ) : (
            !loading && <p>Match data could not be loaded.</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default EditMatchPage;
