import ConfirmationDialog from './ConfirmationDialog';
import type { ProcessedEvent } from './types';
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
  
  // Delete Event
  eventToDelete: ProcessedEvent | null;
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
  
  eventToDelete,
  deleteEventLoading,
  onDeleteEventConfirm,
  onDeleteEventCancel,
  
  matchLoading,
}: MatchConfirmationDialogsProps) => {
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

      {/* Delete Event Confirmation */}
      <ConfirmationDialog
        isOpen={!!eventToDelete}
        icon="🗑️"
        title="Delete Event"
        message={
          eventToDelete
            ? `Delete ${eventToDelete.type} for ${eventToDelete.teamName} at ${formatMatchEventTime(eventToDelete.periodNumber, eventToDelete.timeInSeconds)}?`
            : ''
        }
        warningMessage="This action cannot be undone."
        confirmText="Delete"
        isLoading={deleteEventLoading}
        onConfirm={onDeleteEventConfirm}
        onCancel={onDeleteEventCancel}
      />
    </>
  );
};

export default MatchConfirmationDialogs;

