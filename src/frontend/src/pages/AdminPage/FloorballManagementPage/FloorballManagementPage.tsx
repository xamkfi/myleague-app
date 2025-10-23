import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import './FloorballManagementPage.scss';
import TeamsIcon from '../../../assets/adminIcons/Teams.svg';
import PlayersIcon from '../../../assets/adminIcons/Persons.svg';
import SeasonsIcon from '../../../assets/adminIcons/Seasons.svg';
import MatchesIcon from '../../../assets/adminIcons/Matches.svg';
import RefereesIcon from '../../../assets/adminIcons/Referees.svg';

const FloorballManagementPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('floorball.management.title', 'Floorball Management')}>
      <div className="floorball-management-container">
        {/* Back button */}
        {/* <BackButton 
          to="/admin" 
          text={t('common.back', 'Back to Admin')} 
        /> */}

        <h2>{t('floorball.management.title', 'Floorball management')}</h2>

        <div className="floorball-actions">
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/teams')}
          >
            <div className="button-text">
              <span className="button-title">{t('floorball.management.actions.teams', 'Teams')}</span>
              <span className="button-subtitle">{t('floorball.management.actions.manageTeams', 'Manage teams')}</span>
            </div>
            <img src={TeamsIcon} alt="Teams" className="button-icon" />
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/players')}
          >
            <div className="button-text">
              <span className="button-title">{t('floorball.management.actions.players', 'Players')}</span>
              <span className="button-subtitle">{t('floorball.management.actions.managePlayers', 'Manage players')}</span>
            </div>
            <img src={PlayersIcon} alt="Players" className="button-icon" />
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/seasons')}
          >
            <div className="button-text">
              <span className="button-title">{t('floorball.management.actions.seasons', 'Seasons')}</span>
              <span className="button-subtitle">{t('floorball.management.actions.manageSeasons', 'Manage seasons')}</span>
            </div>
            <img src={SeasonsIcon} alt="Seasons" className="button-icon" />
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/matches')}
          >
            <div className="button-text">
              <span className="button-title">{t('floorball.management.actions.matches', 'Matches')}</span>
              <span className="button-subtitle">{t('floorball.management.actions.manageMatches', 'Manage matches')}</span>
            </div>
            <img src={MatchesIcon} alt="Matches" className="button-icon" />
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/referees')}
          >
            <div className="button-text">
              <span className="button-title">{t('floorball.management.actions.referees', 'Referees')}</span>
              <span className="button-subtitle">{t('floorball.management.actions.manageReferees', 'Manage referees')}</span>
            </div>
            <img src={RefereesIcon} alt="Referees" className="button-icon" />
          </button>
        </div>
      
      </div>
    </PageTemplate>
  );
};

export default FloorballManagementPage; 