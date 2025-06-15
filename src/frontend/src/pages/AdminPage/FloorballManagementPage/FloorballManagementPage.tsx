import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../components/PageTemplate/PageTemplate';
import './FloorballManagementPage.scss';

const FloorballManagementPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('floorball.management.title', 'Floorball Management')}>
      <div className="floorball-management-container">
        <div className="floorball-actions">
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/teams')}
          >
            👥 {t('floorball.management.actions.teams', 'Manage Teams')}
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/players')}
          >
            🏃‍♂️ {t('floorball.management.actions.players', 'Manage Players')}
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/coaches')}
          >
            👨‍🏫 {t('floorball.management.actions.coaches', 'Manage Coaches')}
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/team-managers')}
          >
            👔 {t('floorball.management.actions.teamManagers', 'Manage Team Managers')}
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/referees')}
          >
            👨‍⚖️ {t('floorball.management.actions.referees', 'Manage Referees')}
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/matches')}
          >
            ⚔️ {t('floorball.management.actions.matches', 'Manage Matches')}
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/match-events')}
          >
            📊 {t('floorball.management.actions.matchEvents', 'Manage Match Events')}
          </button>
          
          <button
            className="floorball-action-button"
            onClick={() => navigate('/admin/floorball/seasons')}
          >
            📅 {t('floorball.management.actions.seasons', 'Manage Seasons')}
          </button>
        </div>
        
        <div className="back-button-container">
          <button
            className="back-button"
            onClick={() => navigate('/admin')}
          >
            {t('common.back', 'Back to Admin')}
          </button>
        </div>
      </div>
    </PageTemplate>
  );
};

export default FloorballManagementPage; 