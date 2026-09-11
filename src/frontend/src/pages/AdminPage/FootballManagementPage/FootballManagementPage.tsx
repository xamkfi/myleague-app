import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import './FootballManagementPage.scss';
import TeamsIcon from '../../../assets/adminIcons/Teams.svg';
import PlayersIcon from '../../../assets/adminIcons/Persons.svg';
import SeasonsIcon from '../../../assets/adminIcons/Seasons.svg';
import MatchesIcon from '../../../assets/adminIcons/Matches.svg';
import RefereesIcon from '../../../assets/adminIcons/Referees.svg';

const FootballManagementPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('football.management.title', 'Football Management')}>
      <div className="football-management-container">

        <h2>{t('football.management.title', 'Football management')}</h2>

        <div className="football-actions">
          <button
            className="football-action-button"
            onClick={() => navigate('/admin/football/teams')}
          >
            <div className="button-text">
              <span className="button-title">{t('football.management.actions.teams', 'Teams')}</span>
              <span className="button-subtitle">{t('football.management.actions.manageTeams', 'Manage teams')}</span>
            </div>
            <img src={TeamsIcon} alt="Teams" className="button-icon" />
          </button>
          
          <button
            className="football-action-button"
            onClick={() => navigate('/admin/football/players')}
          >
            <div className="button-text">
              <span className="button-title">{t('football.management.actions.players', 'Players')}</span>
              <span className="button-subtitle">{t('football.management.actions.managePlayers', 'Manage players')}</span>
            </div>
            <img src={PlayersIcon} alt="Players" className="button-icon" />
          </button>
          
          <button
            className="football-action-button"
            onClick={() => navigate('/admin/football/seasons')}
          >
            <div className="button-text">
              <span className="button-title">{t('football.management.actions.seasons', 'Seasons')}</span>
              <span className="button-subtitle">{t('football.management.actions.manageSeasons', 'Manage seasons')}</span>
            </div>
            <img src={SeasonsIcon} alt="Seasons" className="button-icon" />
          </button>

          <button
            className="football-action-button"
            onClick={() => navigate('/admin/football/tournaments')}
          >
            <div className="button-text">
              <span className="button-title">{t('football.management.actions.tournaments', 'Tournaments')}</span>
              <span className="button-subtitle">{t('football.management.actions.manageTournaments', 'Manage tournaments')}</span>
            </div>
            <img src={SeasonsIcon} alt="Tournaments" className="button-icon" />
          </button>

          <button
            className="football-action-button"
            onClick={() => navigate('/admin/football/matches')}
          >
            <div className="button-text">
              <span className="button-title">{t('football.management.actions.matches', 'Matches')}</span>
              <span className="button-subtitle">{t('football.management.actions.manageMatches', 'Manage matches')}</span>
            </div>
            <img src={MatchesIcon} alt="Matches" className="button-icon" />
          </button>
          
          <button
            className="football-action-button"
            onClick={() => navigate('/admin/football/referees')}
          >
            <div className="button-text">
              <span className="button-title">{t('football.management.actions.referees', 'Referees')}</span>
              <span className="button-subtitle">{t('football.management.actions.manageReferees', 'Manage referees')}</span>
            </div>
            <img src={RefereesIcon} alt="Referees" className="button-icon" />
          </button>
        </div>
      
      </div>
    </PageTemplate>
  );
};

export default FootballManagementPage; 