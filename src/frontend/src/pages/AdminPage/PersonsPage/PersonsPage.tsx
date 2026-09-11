import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import AddIcon from '../../../assets/basicIcons/add.svg';
import PersonList from './components/PersonList/PersonList';
import './PersonsPage.scss';

const PersonsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handleAddPerson = () => {
    navigate('/admin/persons/new');
  };

  const handleEditPerson = (personId: string) => {
    navigate(`/admin/persons/${personId}/edit`);
  };


  return (
    <div className="persons-page-wrapper">
      <PageTemplate title={t('admin.persons.title', 'Person Management')}>
        <div className="persons-container">

          <div className="persons-header">
            <h2>{t('admin.persons.subtitle', 'Manage Persons')}</h2>
            <Button
              iconLeft={AddIcon}
              rounded="pill"
              onClick={handleAddPerson}
            >
              {t('admin.persons.actions.add', 'Add New Person')}
            </Button>
          </div>
          <div className="persons-content">
            <PersonList 
              onEditPerson={handleEditPerson} 
            />
          </div>
        </div>
      </PageTemplate>
    </div>
  );
};

export default PersonsPage; 