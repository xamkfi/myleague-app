import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import NewsList from './components/NewsList';
import './NewsManagementPage.scss';
import PageTemplate from '../../../components/PageTemplate/PageTemplate';

const NewsManagementPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handleCreateNew = () => {
    navigate('/admin/news/create');
  };

  return (
    <PageTemplate title={t('admin.news.title', 'News Management')}>
    <div className="news-management-page">
      <div className="page-header">
        <h1>{t('admin.news.title', 'News Management')}</h1>
        <button 
          className="create-new-button"
          onClick={handleCreateNew}
        >
          {t('admin.news.createNew', 'Create New')}
        </button>
      </div>
      
      <div className="news-content">
        <NewsList />
      </div>
    </div>
    </PageTemplate>
  );
};

export default NewsManagementPage;
