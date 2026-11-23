import { useTranslation } from 'react-i18next';
import ConfirmationDialog from '../../../../../components/ConfirmationDialog/ConfirmationDialog';
import type { EventGroup } from './types';

interface MatchConfirmationDialogsProps {
  // End Period
  showEndPeriodConfirmation: boolean;
  currentPeriod: number;
  currentTimeFormatted: string;
  periodLoading: Record<number, boolean>;
  onEndPeriodConfirm: () => Promise<void>;
  onEndPeriodCancel: () => void;
  
  // Overtime
  showOvertimeConfirmation: boolean;
  onOvertimeConfirm: () => Promise<void>;
  onOvertimeCancel: () => void;
  
  // Shootout
  showShootoutConfirmation: boolean;
  onShootoutConfirm: () => Promise<void>;
  onShootoutCancel: () => void;
  
  // End Match
  showEndMatchConfirmation: boolean;
  isShootout: boolean;
  onEndMatchConfirm: () => Promise<void>;
  onEndMatchCancel: () => void;

  // Reopen Match
  showReopenConfirmation: boolean;
  onReopenConfirm: () => Promise<void>;
  onReopenCancel: () => void;

  // Delete Event(s). One entry → single-row delete (or a bulk-save cluster expanded into
  // one EventGroup). Multiple entries → multi-select bulk delete from the events history.
  groupsToDelete: EventGroup[] | null;
  deleteEventLoading: boolean;
  onDeleteEventConfirm: () => Promise<void>;
  onDeleteEventCancel: () => void;
  
  // Loading state
  matchLoading: boolean;
}

export const MatchConfirmationDialogs = ({
  showEndPeriodConfirmation,
  currentPeriod,
  currentTimeFormatted,
  periodLoading,
  onEndPeriodConfirm,
  onEndPeriodCancel,
  
  showOvertimeConfirmation,
  onOvertimeConfirm,
  onOvertimeCancel,
  
  showShootoutConfirmation,
  onShootoutConfirm,
  onShootoutCancel,
  
  showEndMatchConfirmation,
  isShootout,
  onEndMatchConfirm,
  onEndMatchCancel,

  showReopenConfirmation,
  onReopenConfirm,
  onReopenCancel,

  groupsToDelete,
  deleteEventLoading,
  onDeleteEventConfirm,
  onDeleteEventCancel,
  
  matchLoading,
}: MatchConfirmationDialogsProps) => {
  const { t } = useTranslation();

  const totalEventCount: number = groupsToDelete
    ? groupsToDelete.reduce((sum: number, g: EventGroup) => sum + g.events.length, 0)
    : 0;
  return (
    <>
      {/* End Period Confirmation */}
      <ConfirmationDialog
        isOpen={showEndPeriodConfirmation}
        icon="⚠️"
        title={t('football.matches.manage.confirmEndHalf.title')}
        message={t('football.matches.manage.confirmEndHalf.message', { number: currentPeriod, time: currentTimeFormatted })}
        warningMessage={t('matchManage.confirmDelete.warning')}
        confirmText={t('football.matches.manage.confirmEndHalf.confirm')}
        isLoading={periodLoading[currentPeriod]}
        onConfirm={onEndPeriodConfirm}
        onCancel={onEndPeriodCancel}
      />

      {/* Overtime Confirmation */}
      <ConfirmationDialog
        isOpen={showOvertimeConfirmation}
        icon="⏰"
        title={t('football.matches.manage.confirmExtraTime.title')}
        message={t('football.matches.manage.confirmExtraTime.message')}
        warningMessage={t('football.matches.manage.confirmExtraTime.warning')}
        confirmText={t('football.matches.manage.periodControl.startExtraTime')}
        isLoading={matchLoading}
        onConfirm={onOvertimeConfirm}
        onCancel={onOvertimeCancel}
      />

      {/* Shootout Confirmation */}
      <ConfirmationDialog
        isOpen={showShootoutConfirmation}
        icon="🎯"
        title={t('football.matches.manage.confirmShootout.title')}
        message={t('football.matches.manage.confirmShootout.message')}
        warningMessage={t('football.matches.manage.confirmShootout.warning')}
        confirmText={t('football.matches.manage.confirmShootout.confirm')}
        isLoading={matchLoading}
        onConfirm={onShootoutConfirm}
        onCancel={onShootoutCancel}
      />

      {/* End Match Confirmation */}
      <ConfirmationDialog
        isOpen={showEndMatchConfirmation}
        icon="🏁"
        title={t('football.matches.manage.confirmEndMatch.title')}
        message={isShootout
          ? t('football.matches.manage.confirmEndMatch.messageShootout')
          : t('football.matches.manage.confirmEndMatch.message')}
        warningMessage={t('football.matches.manage.confirmEndMatch.warning')}
        confirmText={t('matchManage.header.finishMatch')}
        isLoading={matchLoading}
        onConfirm={onEndMatchConfirm}
        onCancel={onEndMatchCancel}
      />

      {/* Reopen Match Confirmation */}
      <ConfirmationDialog
        isOpen={showReopenConfirmation}
        icon="🔓"
        title={t('football.matches.manage.confirmReopen.title', 'Reopen match for editing')}
        message={t(
          'football.matches.manage.confirmReopen.message',
          'Are you sure you want to reopen this match? It will move back to In progress so you can edit events or continue play (e.g. if the match was finished by accident).'
        )}
        warningMessage={t(
          'football.matches.manage.confirmReopen.warning',
          'Per-match team, player and goalie season aggregates will be reverted. Statistics will be recalculated when you finish the match again.'
        )}
        confirmText={t('football.matches.manage.confirmReopen.confirm', 'Yes, reopen match')}
        isLoading={matchLoading}
        onConfirm={onReopenConfirm}
        onCancel={onReopenCancel}
      />

      {/* Delete Event Confirmation */}
      <ConfirmationDialog
        isOpen={!!groupsToDelete && groupsToDelete.length > 0}
        icon="🗑️"
        title={totalEventCount > 1
          ? t('matchManage.confirmDelete.titleMany', { count: totalEventCount })
          : t('matchManage.confirmDelete.title')}
        message={totalEventCount > 1
          ? t('matchManage.confirmDelete.messageMany', { count: totalEventCount })
          : t('matchManage.confirmDelete.message')}
        warningMessage={t('matchManage.confirmDelete.warning')}
        confirmText={t('matchManage.confirmDelete.confirm')}
        isLoading={deleteEventLoading}
        onConfirm={onDeleteEventConfirm}
        onCancel={onDeleteEventCancel}
      />
    </>
  );
};

export default MatchConfirmationDialogs;

