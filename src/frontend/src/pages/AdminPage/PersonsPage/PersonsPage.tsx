import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../components/PageTemplate/PageTemplate';
import PersonList from './components/PersonList/PersonList';
import './PersonsPage.scss';

const PersonsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handleAddPerson = () => {
    navigate('/admin/persons/new');
  };

  return (
    <PageTemplate title={t('admin.persons.title', 'Person Management')}>
      <div className="persons-container">
        <div className="persons-header">
          <h2>{t('admin.persons.subtitle', 'Manage Persons')}</h2>
          <button 
            className="persons-add-button"
            onClick={handleAddPerson}
          >
            {t('admin.persons.actions.add', 'Add New Person')}
          </button>
        </div>
        <div className="persons-content">
          <PersonList />
        </div>
      </div>
    </PageTemplate>
  );
};

export default PersonsPage; 