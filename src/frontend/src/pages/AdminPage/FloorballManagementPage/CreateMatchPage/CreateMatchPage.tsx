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
import './CreateMatchPage.scss';
import '../MatchOverviewPage/MatchOverviewPage.scss';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

const CreateMatchPage = () => {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleCreateMatch = async (matchData: CreateFloorballMatchRequest) => {
    try {
      setLoading(true);

      const response = await floorballMatchService.create(matchData);
      
      if (response.success && response.data) {
        navigate('/admin/floorball/matches');
      }

    } catch (error) {
      console.error('Error creating match:', error);
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
    <PageTemplate title={'Create match'}>
    <div className="match-management">
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
    </PageTemplate>
  );
};

export default CreateMatchPage;
