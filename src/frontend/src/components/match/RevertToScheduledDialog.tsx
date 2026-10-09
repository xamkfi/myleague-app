import { useTranslation } from 'react-i18next';
import ConfirmationDialog from '../ConfirmationDialog/ConfirmationDialog';

interface RevertToScheduledDialogProps {
  isOpen: boolean;
  isLoading: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

/**
 * Confirms putting a match that was started by mistake back to not started. Shared by the
 * floorball, football and hockey live desks.
 */
function RevertToScheduledDialog({ isOpen, isLoading, onConfirm, onCancel }: RevertToScheduledDialogProps) {
  const { t } = useTranslation();

  return (
    <ConfirmationDialog
      isOpen={isOpen}
      icon="↩️"
      title={t('matchManage.revertToScheduled.title', 'Revert match to not started?')}
      message={t(
        'matchManage.revertToScheduled.message',
        'The match goes back to not started and its clock is reset. You can start it again later.',
      )}
      warningMessage={t(
        'matchManage.revertToScheduled.warning',
        'Only possible while the score is 0-0 and no events have been recorded.',
      )}
      confirmText={t('matchManage.revertToScheduled.confirm', 'Revert to not started')}
      cancelText={t('common.cancel', 'Cancel')}
      isLoading={isLoading}
      onConfirm={onConfirm}
      onCancel={onCancel}
    />
  );
}

export default RevertToScheduledDialog;
