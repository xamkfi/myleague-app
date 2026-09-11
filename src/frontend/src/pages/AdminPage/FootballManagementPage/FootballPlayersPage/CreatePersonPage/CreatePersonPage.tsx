import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import PersonForm from '../../../PersonsPage/components/PersonForm/PersonForm';
import type { Person } from '../../../../../types/admin/personTypes';
import './CreatePersonPage.scss';

const CreatePersonPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handlePersonCreated = (newPerson: Person) => {
    // Navigate back to create player page with success message
    navigate('/admin/football/players/create', { 
      state: { 
        newPersonCreated: newPerson,
        successMessage: t('football.players.personCreated', '{{personName}} created successfully! You can now create a player from them.', { 
          personName: newPerson.fullName 
        })
      }
    });
  };

  const handleCancel = () => {
    navigate('/admin/football/players/create');
  };

  return (
    <PageTemplate title={t('football.players.createNewPerson', 'Create New Person')}>
      <div className="create-person-container">
        
        {/* Embedded PersonForm */}
        <div className="person-form-container">
          <PersonForm
            mode="embedded"
            showTeamAssignment={false}
            onSuccess={handlePersonCreated}
            onCancel={handleCancel}
          />
        </div>
      </div>
    </PageTemplate>
  );
};

export default CreatePersonPage;
