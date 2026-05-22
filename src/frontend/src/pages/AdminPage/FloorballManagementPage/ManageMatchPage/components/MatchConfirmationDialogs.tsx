import { useTranslation } from 'react-i18next';
import ConfirmationDialog from './ConfirmationDialog';
import type { EventGroup } from './types';
import { formatMatchEventTime } from '../../../../../utils/matchEventFormat';

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

  // Delete Event (single event or a bulk-save group of events)
  eventToDelete: EventGroup | null;
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

  eventToDelete,
  deleteEventLoading,
  onDeleteEventConfirm,
  onDeleteEventCancel,
  
  matchLoading,
}: MatchConfirmationDialogsProps) => {
  const { t } = useTranslation();
  return (
    <>
      {/* End Period Confirmation */}
      <ConfirmationDialog
        isOpen={showEndPeriodConfirmation}
        icon="⚠️"
        title="Confirm End Period"
        message={`Are you sure you want to end period ${currentPeriod} at ${currentTimeFormatted}?`}
        warningMessage="This action cannot be undone."
        confirmText="End Period"
        isLoading={periodLoading[currentPeriod]}
        onConfirm={onEndPeriodConfirm}
        onCancel={onEndPeriodCancel}
      />

      {/* Overtime Confirmation */}
      <ConfirmationDialog
        isOpen={showOvertimeConfirmation}
        icon="⏰"
        title="Start Overtime"
        message="Are you sure you want to start overtime for this match?"
        warningMessage="This will begin the overtime period. The clock will be reset to 0:00."
        confirmText="Start Overtime"
        isLoading={matchLoading}
        onConfirm={onOvertimeConfirm}
        onCancel={onOvertimeCancel}
      />

      {/* Shootout Confirmation */}
      <ConfirmationDialog
        isOpen={showShootoutConfirmation}
        icon="🎯"
        title="Start Shootout"
        message="Are you sure you want to start a shootout for this match?"
        warningMessage="Shootout does not use time keeping. Goals will be recorded without time."
        confirmText="Start Shootout"
        isLoading={matchLoading}
        onConfirm={onShootoutConfirm}
        onCancel={onShootoutCancel}
      />

      {/* End Match Confirmation */}
      <ConfirmationDialog
        isOpen={showEndMatchConfirmation}
        icon="🏁"
        title="Confirm End Match"
        message={`Are you sure you want to complete this match?${isShootout ? ' This will end the shootout.' : ''}`}
        warningMessage="This will finalize the match results. This action cannot be undone."
        confirmText="Complete Match"
        isLoading={matchLoading}
        onConfirm={onEndMatchConfirm}
        onCancel={onEndMatchCancel}
      />

      {/* Reopen Match Confirmation */}
      <ConfirmationDialog
        isOpen={showReopenConfirmation}
        icon="🔓"
        title={t('floorball.matches.manage.confirmReopen.title', 'Reopen match for editing')}
        message={t(
          'floorball.matches.manage.confirmReopen.message',
          'Are you sure you want to reopen this match? It will move back to In progress so you can edit events or continue play (e.g. if the match was finished by accident).'
        )}
        warningMessage={t(
          'floorball.matches.manage.confirmReopen.warning',
          'Per-match team, player and goalie season aggregates will be reverted. Statistics will be recalculated when you finish the match again.'
        )}
        confirmText={t('floorball.matches.manage.confirmReopen.confirm', 'Yes, reopen match')}
        isLoading={matchLoading}
        onConfirm={onReopenConfirm}
        onCancel={onReopenCancel}
      />

      {/* Delete Event Confirmation. Bulk-save groups (multiple saves at the exact same
          coordinate) are shown with a count so the user knows the click will remove every
          save in the cluster, not just one. */}
      <ConfirmationDialog
        isOpen={!!eventToDelete}
        icon="🗑️"
        title={
          eventToDelete && eventToDelete.events.length > 1
            ? `Delete ${eventToDelete.events.length} ${eventToDelete.representative.type}s`
            : 'Delete Event'
        }
        message={
          eventToDelete
            ? eventToDelete.events.length > 1
              ? `Delete all ${eventToDelete.events.length} ${eventToDelete.representative.type}s for ${eventToDelete.representative.teamName}${eventToDelete.representative.playerName ? ` (${eventToDelete.representative.playerName})` : ''} at ${formatMatchEventTime(eventToDelete.representative.periodNumber, eventToDelete.representative.timeInSeconds)}?`
              : `Delete ${eventToDelete.representative.type} for ${eventToDelete.representative.teamName} at ${formatMatchEventTime(eventToDelete.representative.periodNumber, eventToDelete.representative.timeInSeconds)}?`
            : ''
        }
        warningMessage="This action cannot be undone."
        confirmText={
          eventToDelete && eventToDelete.events.length > 1
            ? `Delete ${eventToDelete.events.length}`
            : 'Delete'
        }
        isLoading={deleteEventLoading}
        onConfirm={onDeleteEventConfirm}
        onCancel={onDeleteEventCancel}
      />
    </>
  );
};

export default MatchConfirmationDialogs;

