import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { 
  CreateFloorballMatchRequest,
  ChangeMatchSeasonRequest,
  ChangeMatchTeamsRequest,
  ChangeMatchVenueRequest,
  ChangeMatchDateTimeRequest
} from '../../../../types/floorball/floorballTypes';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import MatchForm from '../MatchOverviewPage/Components/MatchForm/MatchForm';
import BackButton from '../../../../components/BackButton/BackButton';
import Navbar from '../../../../components/Navigation/Navbar';
import './CreateMatchPage.scss';
import '../MatchOverviewPage/MatchOverviewPage.scss';

const CreateMatchPage = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleCreateMatch = async (matchData: CreateFloorballMatchRequest) => {
    try {
      setLoading(true);
      setError(null);

      const response = await floorballMatchService.create(matchData);
      
      if (response.success && response.data) {
        navigate('/admin/floorball/matches');
      }

    } catch (error) {
      console.error('Error creating match:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to create match';
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const handleFormSubmit = (
    matchData: CreateFloorballMatchRequest | ChangeMatchSeasonRequest | ChangeMatchTeamsRequest | ChangeMatchVenueRequest | ChangeMatchDateTimeRequest
  ) => {
    return handleCreateMatch(matchData as CreateFloorballMatchRequest);
  }

  const handleCancel = () => {
    navigate('/admin/floorball/matches');
  };

  return (
    <div className="match-management">
      <Navbar />
      <div className="match-management__content create-match-page">
        <div className="page-header">
          <div className="header-left">
            <BackButton to="/admin/floorball/matches" text="Back" />
          </div>
          <div className="header-center">
            <h1>Create New Match</h1>
          </div>
          <div className="header-right"></div>
        </div>

        {error && (
          <div className="error-alert page-error">
            <span className="error-icon">⚠️</span>
            <span className="error-text">{error}</span>
            <button onClick={() => setError(null)} className="error-close">×</button>
          </div>
        )}

        <div className="form-container">
          <MatchForm
            mode="create"
            onSubmit={handleFormSubmit}
            onCancel={handleCancel}
            loading={loading}
          />
        </div>
      </div>
    </div>
  );
};

export default CreateMatchPage;
