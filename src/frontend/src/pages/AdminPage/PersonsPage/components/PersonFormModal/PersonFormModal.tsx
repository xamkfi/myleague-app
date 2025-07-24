import { useTranslation } from 'react-i18next';
import type { Person, EnhancedPersonFormData } from '../../../../../types/admin/personTypes';
import PersonForm from '../PersonForm/PersonForm';
import './PersonFormModal.scss';

interface PersonFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  mode: 'create' | 'edit';
  personId?: string;
  onSuccess?: (person: Person) => void;
  showTeamAssignment?: boolean;
  initialData?: Partial<EnhancedPersonFormData>;
}

const PersonFormModal = ({
  isOpen,
  onClose,
  mode,
  personId,
  onSuccess,
  showTeamAssignment = true,
  initialData
}: PersonFormModalProps) => {
  const { t } = useTranslation();

  if (!isOpen) return null;

  const handleSuccess = (person: Person) => {
    if (onSuccess) {
      onSuccess(person);
    }
    onClose();
  };

  const handleCancel = () => {
    onClose();
  };

  return (
    <div className="person-form-modal-overlay" onClick={onClose}>
      <div 
        className="person-form-modal" 
        onClick={(e) => e.stopPropagation()}
      >
        <div className="person-form-modal-header">
          <h2>
            {mode === 'create' 
              ? t('admin.persons.modal.createTitle', 'Create New Person')
              : t('admin.persons.modal.editTitle', 'Edit Person')
            }
          </h2>
          <button 
            className="person-form-modal-close" 
            onClick={onClose}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>
        
        <div className="person-form-modal-content">
          <PersonForm
            mode="embedded"
            personId={personId}
            onSuccess={handleSuccess}
            onCancel={handleCancel}
            showTeamAssignment={showTeamAssignment}
            initialData={initialData}
          />
        </div>
      </div>
    </div>
  );
};

export default PersonFormModal; 