import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import './HockeyManagementPage.scss';
import TeamsIcon from '../../../assets/adminIcons/Teams.svg';
import PlayersIcon from '../../../assets/adminIcons/Persons.svg';
import SeasonsIcon from '../../../assets/adminIcons/Seasons.svg';
import MatchesIcon from '../../../assets/adminIcons/Matches.svg';
import RefereesIcon from '../../../assets/adminIcons/Referees.svg';

function HockeyManagementPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('hockey.management.title', 'Hockey Management')}>
      <div className="hockey-management-container">
        <h2>{t('hockey.management.title', 'Hockey management')}</h2>
        <div className="hockey-actions">
          <button className="hockey-action-button" onClick={() => navigate('/admin/hockey/teams')}>
            <div className="button-text">
              <span className="button-title">{t('hockey.management.actions.teams', 'Teams')}</span>
              <span className="button-subtitle">{t('hockey.management.actions.manageTeams', 'Manage teams')}</span>
            </div>
            <img src={TeamsIcon} alt="" className="button-icon" />
          </button>
          <button className="hockey-action-button" onClick={() => navigate('/admin/hockey/players')}>
            <div className="button-text">
              <span className="button-title">{t('hockey.management.actions.players', 'Players')}</span>
              <span className="button-subtitle">{t('hockey.management.actions.managePlayers', 'Manage players')}</span>
            </div>
            <img src={PlayersIcon} alt="" className="button-icon" />
          </button>
          <button className="hockey-action-button" onClick={() => navigate('/admin/hockey/seasons')}>
            <div className="button-text">
              <span className="button-title">{t('hockey.management.actions.seasons', 'Seasons')}</span>
              <span className="button-subtitle">{t('hockey.management.actions.manageSeasons', 'Manage seasons')}</span>
            </div>
            <img src={SeasonsIcon} alt="" className="button-icon" />
          </button>
          <button className="hockey-action-button" onClick={() => navigate('/admin/hockey/tournaments')}>
            <div className="button-text">
              <span className="button-title">{t('hockey.management.actions.tournaments', 'Tournaments')}</span>
              <span className="button-subtitle">{t('hockey.management.actions.manageTournaments', 'Manage tournaments')}</span>
            </div>
            <img src={SeasonsIcon} alt="" className="button-icon" />
          </button>
          <button className="hockey-action-button" onClick={() => navigate('/admin/hockey/matches')}>
            <div className="button-text">
              <span className="button-title">{t('hockey.management.actions.matches', 'Matches')}</span>
              <span className="button-subtitle">{t('hockey.management.actions.manageMatches', 'Manage matches')}</span>
            </div>
            <img src={MatchesIcon} alt="" className="button-icon" />
          </button>
          <button className="hockey-action-button" onClick={() => navigate('/admin/hockey/officials')}>
            <div className="button-text">
              <span className="button-title">{t('hockey.management.actions.officials', 'Officials')}</span>
              <span className="button-subtitle">{t('hockey.management.actions.manageOfficials', 'Manage officials')}</span>
            </div>
            <img src={RefereesIcon} alt="" className="button-icon" />
          </button>
        </div>
      </div>
    </PageTemplate>
  );
}

export default HockeyManagementPage;
