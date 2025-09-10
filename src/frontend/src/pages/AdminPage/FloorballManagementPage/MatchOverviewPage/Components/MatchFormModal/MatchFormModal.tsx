import type { 
  FloorballMatchDto,
  ChangeMatchSeasonRequest,
  ChangeMatchTeamsRequest,
  ChangeMatchVenueRequest,
  ChangeMatchDateTimeRequest
} from '../../../../../../types/floorball/floorballTypes';
import MatchForm from '../MatchForm/MatchForm';
import './MatchFormModal.scss';

interface MatchFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialData?: FloorballMatchDto;
  onSubmit: (matchData: ChangeMatchSeasonRequest | ChangeMatchTeamsRequest | ChangeMatchVenueRequest | ChangeMatchDateTimeRequest) => Promise<void>;
  onCancelMatch?: (matchId: string) => Promise<void>;
  loading?: boolean;
}

const MatchFormModal = ({
  isOpen,
  onClose,
  initialData,
  onSubmit,
  onCancelMatch,
  loading = false
}: MatchFormModalProps) => {

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal create-match-modal" lang="fi">
        <div className="modal-header">
          <h2>Edit Match</h2>
          <button onClick={onClose} className="modal-close">×</button>
        </div>
        
        <div className="modal-form-container">
          <MatchForm
            mode="edit"
            initialData={initialData}
            onSubmit={onSubmit}
            onCancel={onClose}
            onCancelMatch={onCancelMatch}
            loading={loading}
          />
        </div>
      </div>
    </div>
  );
};

export default MatchFormModal; 