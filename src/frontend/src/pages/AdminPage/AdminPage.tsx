import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import AdminPageTemplate from '../../components/PageTemplate/AdminPageTemplate';
import './AdminPage.scss';
import PersonIcon from '../../assets/adminIcons/Persons.svg';
import NewsIcon from '../../assets/adminIcons/News.svg';
import SportsIcon from '../../assets/adminIcons/Sports.svg';

const AdminPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <AdminPageTemplate title={t('admin.title', 'Admin Dashboard')}>
      <div className="admin-container">
        <h2 className="admin-overview">OVERVIEW</h2>
        <div className="admin-actions">
          <button
            className="admin-action-button"
            onClick={() => navigate('/admin/persons')}
          >
            <div className="button-text">
              <span className="button-title">{t('admin.actions.persons', 'Persons')}</span>
              <span className="button-subtitle">{t('admin.actions.managePerson', 'Manage persons')}</span>
            </div>
            <img src={PersonIcon} alt="Person" className="button-icon" />
          </button>
          <button
            className="admin-action-button"
            onClick={() => navigate('/admin/news')}
          >
            <div className="button-text">
              <span className="button-title">{t('admin.actions.news', 'News')}</span>
              <span className="button-subtitle">{t('admin.actions.manageNews', 'Manage news')}</span>
            </div>
            <img src={NewsIcon} alt="News" className="button-icon" />
          </button>
          
        </div>
        <h2 className="admin-sport-selection">SPORTS SELECTION</h2>
        <div className="admin-actions">
          <button
            className="admin-action-button"
            onClick={() => navigate('/admin/floorball')}
          >
            <div className="button-text">
              <span className="button-title">{t('admin.actions.floorball', 'Floorball')}</span>
              <span className="button-subtitle">{t('admin.actions.manageFloorball', 'Manage floorball')}</span>
            </div>
            <img src={SportsIcon} alt="Floorball" className="button-icon" />
          </button>
        </div>
      </div>
    </AdminPageTemplate>
  );
};

export default AdminPage; 