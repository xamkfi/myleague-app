import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import PersonForm from '../../../PersonsPage/components/PersonForm/PersonForm';
import type { Person } from '../../../../../types/admin/personTypes';
import './CreatePersonPage.scss';

function CreateHockeyPersonPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handlePersonCreated = (newPerson: Person): void => {
    navigate('/admin/hockey/players/create', {
      state: {
        newPersonCreated: newPerson,
        successMessage: t(
          'hockey.players.personCreated',
          '{{personName}} created successfully! You can now create a player from them.',
          { personName: newPerson.fullName },
        ),
      },
    });
  };

  return (
    <PageTemplate title={t('hockey.players.createNewPerson', 'Create New Person')}>
      <div className="create-person-container">
        <div className="person-form-container">
          <PersonForm
            mode="embedded"
            showTeamAssignment={false}
            onSuccess={handlePersonCreated}
            onCancel={() => navigate('/admin/hockey/players/create')}
          />
        </div>
      </div>
    </PageTemplate>
  );
}

export default CreateHockeyPersonPage;
