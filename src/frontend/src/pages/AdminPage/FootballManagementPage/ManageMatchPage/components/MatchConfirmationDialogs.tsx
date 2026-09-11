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

  // Pre-compute counts and a single representative once per render so the JSX below stays
  // readable and avoids repeated `.flatMap` calls. `representativeGroup` is intentionally
  // the first group: the dialog only surfaces full per-event detail in the single-group
  // case, and the multi-group case uses an aggregate count summary instead.
  const groupCount: number = groupsToDelete?.length ?? 0;
  const totalEventCount: number = groupsToDelete
    ? groupsToDelete.reduce((sum: number, g: EventGroup) => sum + g.events.length, 0)
    : 0;
  const isBulkMultiGroup: boolean = groupCount > 1;
  // Bulk-save cluster collapsed into a single group still counts as a "single delete"
  // from the dialog's perspective — the original copy already explains "all N saves".
  const isSingleGroup: boolean = groupCount === 1;
  const singleGroup: EventGroup | undefined = isSingleGroup ? groupsToDelete?.[0] : undefined;
  return (
    <>
      {/* End Period Confirmation */}
      <ConfirmationDialog
        isOpen={showEndPeriodConfirmation}
        icon="⚠️"
        title="Confirm End Half"
        message={`Are you sure you want to end half ${currentPeriod} at ${currentTimeFormatted}?`}
        warningMessage="This action cannot be undone."
        confirmText="End Half"
        isLoading={periodLoading[currentPeriod]}
        onConfirm={onEndPeriodConfirm}
        onCancel={onEndPeriodCancel}
      />

      {/* Overtime Confirmation */}
      <ConfirmationDialog
        isOpen={showOvertimeConfirmation}
        icon="⏰"
        title="Start Extra Time"
        message="Are you sure you want to start extra time for this match?"
        warningMessage="This will begin extra time halves according to the match rules."
        confirmText="Start Extra Time"
        isLoading={matchLoading}
        onConfirm={onOvertimeConfirm}
        onCancel={onOvertimeCancel}
      />

      {/* Shootout Confirmation */}
      <ConfirmationDialog
        isOpen={showShootoutConfirmation}
        icon="🎯"
        title="Start Penalty Shootout"
        message="Are you sure you want to start a penalty shootout for this match?"
        warningMessage="Penalty shootout does not use time keeping. Goals will be recorded without time."
        confirmText="Start Penalty Shootout"
        isLoading={matchLoading}
        onConfirm={onShootoutConfirm}
        onCancel={onShootoutCancel}
      />

      {/* End Match Confirmation */}
      <ConfirmationDialog
        isOpen={showEndMatchConfirmation}
        icon="🏁"
        title="Confirm End Match"
        message={`Are you sure you want to complete this match?${isShootout ? ' This will end the penalty shootout.' : ''}`}
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

      {/* Delete Event Confirmation. Three cases share this dialog:                              */}
      {/* 1. Single-row delete            → "Delete goal for ABC at 02:14?"                       */}
      {/* 2. Bulk-save cluster (1 group)  → "Delete all 5 saves for ABC (Doe) at 02:14?"          */}
      {/* 3. Multi-select bulk delete (N) → "Delete 12 selected match events?" + summary list    */}
      <ConfirmationDialog
        isOpen={!!groupsToDelete && groupsToDelete.length > 0}
        icon="🗑️"
        title={
          isBulkMultiGroup
            ? `Delete ${totalEventCount} match events`
            : singleGroup && singleGroup.events.length > 1
              ? `Delete ${singleGroup.events.length} ${singleGroup.representative.type}s`
              : 'Delete Event'
        }
        message={
          isBulkMultiGroup
            ? `Delete ${totalEventCount} selected match event${totalEventCount === 1 ? '' : 's'}` +
              ` (${groupCount} rows)?`
            : singleGroup
              ? singleGroup.events.length > 1
                ? `Delete all ${singleGroup.events.length} ${singleGroup.representative.type}s for ${singleGroup.representative.teamName}${singleGroup.representative.playerName ? ` (${singleGroup.representative.playerName})` : ''} at ${formatMatchEventTime(singleGroup.representative.periodNumber, singleGroup.representative.timeInSeconds)}?`
                : `Delete ${singleGroup.representative.type} for ${singleGroup.representative.teamName} at ${formatMatchEventTime(singleGroup.representative.periodNumber, singleGroup.representative.timeInSeconds)}?`
              : ''
        }
        warningMessage="This action cannot be undone."
        confirmText={
          isBulkMultiGroup
            ? `Delete ${totalEventCount}`
            : singleGroup && singleGroup.events.length > 1
              ? `Delete ${singleGroup.events.length}`
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

