import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/PageTemplate';
import PersonList from './components/PersonList/PersonList';
import PersonFormModal from './components/PersonFormModal/PersonFormModal';
import type { Person } from '../../../types/admin/personTypes';
import './PersonsPage.scss';
import BackButton from '../../../components/BackButton/BackButton';

const PersonsPage = () => {
  const { t } = useTranslation();
  const [modalState, setModalState] = useState<{
    isOpen: boolean;
    mode: 'create' | 'edit';
    personId?: string;
  }>({
    isOpen: false,
    mode: 'create'
  });
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const handleAddPerson = () => {
    setModalState({
      isOpen: true,
      mode: 'create'
    });
  };

  const handleEditPerson = (personId: string) => {
    setModalState({
      isOpen: true,
      mode: 'edit',
      personId
    });
  };

  const handleCloseModal = () => {
    setModalState({
      isOpen: false,
      mode: 'create'
    });
  };

  const handlePersonSuccess = (person: Person) => {
    // You could add a toast notification here
    console.log(`Person ${modalState.mode}d successfully:`, person);
    // Trigger refresh of the person list
    setRefreshTrigger(prev => prev + 1);
    // The modal will close automatically via handleCloseModal
  };

  return (
    <div className="persons-page-wrapper">
      <PageTemplate title={t('admin.persons.title', 'Person Management')}>
        <div className="persons-container">
          {/* Back button */}
          <BackButton 
            to="/admin/" 
            text={t('common.back', 'Back to Floorball Management')} 
          />
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
            <PersonList 
              onEditPerson={handleEditPerson} 
              refreshTrigger={refreshTrigger}
            />
          </div>
        </div>

        {/* Person Form Modal */}
        <PersonFormModal
          isOpen={modalState.isOpen}
          onClose={handleCloseModal}
          mode={modalState.mode}
          personId={modalState.personId}
          onSuccess={handlePersonSuccess}
          showTeamAssignment={true}
        />
      </PageTemplate>
    </div>
  );
};

export default PersonsPage; 