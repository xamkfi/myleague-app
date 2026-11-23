import { useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import ConfirmationDialog from '../ConfirmationDialog/ConfirmationDialog';

interface DeleteMatchDialogProps {
  /** "Home – Away", so the admin can tell duplicates apart from other matches. */
  matchLabel: string;
  onDelete: () => Promise<void>;
  /** Called after a successful delete. */
  onDeleted: () => void;
  onError: (message: string) => void;
  onClose: () => void;
}

/** Confirms deleting a match with its statistics. Shared by the match lists of all three sports. */
function DeleteMatchDialog({ matchLabel, onDelete, onDeleted, onError, onClose }: DeleteMatchDialogProps): ReactElement {
  const { t } = useTranslation();
  const [deleting, setDeleting] = useState<boolean>(false);

  const handleConfirm = async (): Promise<void> => {
    setDeleting(true);
    try {
      await onDelete();
      onDeleted();
    } catch (error) {
      const message: string = error instanceof Error ? error.message : '';
      onError(message.includes('playoff')
        ? t('matchManage.deleteMatch.playoffStarted')
        : t('matchManage.deleteMatch.failed'));
    } finally {
      onClose();
    }
  };

  return (
    <ConfirmationDialog
      isOpen
      icon="🗑️"
      title={t('matchManage.deleteMatch.title')}
      message={t('matchManage.deleteMatch.message', { match: matchLabel })}
      warningMessage={t('matchManage.deleteMatch.warning')}
      confirmText={t('matchManage.deleteMatch.confirm')}
      cancelText={t('common.cancel')}
      isLoading={deleting}
      onConfirm={() => void handleConfirm()}
      onCancel={onClose}
    />
  );
}

export default DeleteMatchDialog;
