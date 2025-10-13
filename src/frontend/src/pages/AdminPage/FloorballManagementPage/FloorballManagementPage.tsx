import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { divisionService } from '../../../api/common/divisionService';
import PageTemplate from '../../../components/PageTemplate/PageTemplate';
import './FloorballManagementPage.scss';
import BackButton from '../../../components/BackButton/BackButton';

const FloorballManagementPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const handleComingSoonClick = async (featureName: string) => {
    const division = (await divisionService.getById("b851ac37-219c-4fa9-a76f-d8620066151d")).data.name;
    console.log(division);
    alert(t('floorball.management.comingSoonAlert', '🚧 {{featureName}} is coming soon! 🚧\n\nOur development team is working hard to bring you this feature. Stay tuned for updates! 🎯', { featureName }));
  };

  return (
    <PageTemplate title={t('floorball.management.title', 'Floorball Management')}>
      <div className="floorball-management-container">
        {/* Back button */}
        <BackButton 
          to="/admin" 
          text={t('common.back', 'Back to Admin')} 
        />

        {/* Status Legend */}
        <div className="feature-status-legend">
          <div className="legend-item">
            <span className="status-indicator available"></span>
            <span>{t('floorball.management.legend.availableNow', 'Available Now')}</span>
          </div>
          <div className="legend-item">
            <span className="status-indicator coming-soon"></span>
            <span>{t('floorball.management.legend.comingSoon', 'Coming Soon')}</span>
          </div>
        </div>

        <div className="floorball-actions">
          {/* Available Features */}
          <button
            className="floorball-action-button available"
            onClick={() => navigate('/admin/floorball/teams')}
          >
            <div className="button-content">
              <span className="emoji">👥</span>
              <span className="text">{t('floorball.management.actions.teams', 'Manage Teams')}</span>
              <div className="status-badge available">{t('floorball.management.badges.ready', '✨ Ready!')}</div>
            </div>
          </button>
          
          <button
            className="floorball-action-button available"
            onClick={() => navigate('/admin/floorball/players')}
          >
            <div className="button-content">
              <span className="emoji">🏃‍♂️</span>
              <span className="text">{t('floorball.management.actions.players', 'Manage Players')}</span>
              <div className="status-badge available">{t('floorball.management.badges.ready', '✨ Ready!')}</div>
            </div>
          </button>
          
          <button
            className="floorball-action-button available"
            onClick={() => navigate('/admin/floorball/seasons')}
          >
            <div className="button-content">
              <span className="emoji">📅</span>
              <span className="text">{t('floorball.management.actions.seasons', 'Manage Seasons')}</span>
              <div className="status-badge available">{t('floorball.management.badges.ready', '✨ Ready!')}</div>
            </div>
          </button>
          
          <button
            className="floorball-action-button available"
            onClick={() => navigate('/admin/floorball/matches')}
          >
            <div className="button-content">
              <span className="emoji">⚔️</span>
              <span className="text">{t('floorball.management.actions.matches', 'Manage Matches')}</span>
              <div className="status-badge available">{t('floorball.management.badges.ready', '✨ Ready!')}</div>
            </div>
          </button>
          
          {/* Coming Soon Features */}
          <button
            className="floorball-action-button coming-soon"
            onClick={() => handleComingSoonClick('Manage Coaches')}
          >
            <div className="button-content">
              <span className="emoji">👨‍🏫</span>
              <span className="text">{t('floorball.management.actions.coaches', 'Manage Coaches')}</span>
              <div className="status-badge coming-soon">{t('floorball.management.badges.soon', '🚧 Soon')}</div>
            </div>
          </button>
          
          <button
            className="floorball-action-button coming-soon"
            onClick={() => handleComingSoonClick('Manage Team Managers')}
          >
            <div className="button-content">
              <span className="emoji">👔</span>
              <span className="text">{t('floorball.management.actions.teamManagers', 'Manage Team Managers')}</span>
              <div className="status-badge coming-soon">{t('floorball.management.badges.soon', '🚧 Soon')}</div>
            </div>
          </button>
          
          <button
            className="floorball-action-button available"
            onClick={() => navigate('/admin/floorball/referees')}
          >
            <div className="button-content">
              <span className="emoji">👨‍⚖️</span>
              <span className="text">{t('floorball.management.actions.referees', 'Manage Referees')}</span>
              <div className="status-badge available">{t('floorball.management.badges.ready', '✨ Ready!')}</div>
            </div>
          </button>
          
          <button
            className="floorball-action-button coming-soon"
            onClick={() => handleComingSoonClick('Manage Match Events')}
          >
            <div className="button-content">
              <span className="emoji">📊</span>
              <span className="text">{t('floorball.management.actions.matchEvents', 'Manage Match Events')}</span>
              <div className="status-badge coming-soon">{t('floorball.management.badges.soon', '🚧 Soon')}</div>
            </div>
          </button>
          

        </div>
      
      </div>
    </PageTemplate>
  );
};

export default FloorballManagementPage; 