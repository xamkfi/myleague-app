import { useTranslation } from 'react-i18next';
import ConfirmationDialog from './ConfirmationDialog';
import type { EventGroup, ProcessedEventType } from './types';
import { formatMatchEventTime } from '../../../../../utils/matchEventFormat';

interface MatchConfirmationDialogsProps {
  // End Period
  showEndPeriodConfirmation: boolean;
  currentPeriod: number;
  /** True when the period being ended is the overtime period (changes the wording). */
  isOvertimePeriod: boolean;
  /** Match clock (mm:ss) captured when the confirmation was opened. */
  currentTimeFormatted: string;
  periodLoading: Record<number, boolean>;
  onEndPeriodConfirm: () => Promise<void>;
  onEndPeriodCancel: () => void;

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

  matchLoading: boolean;
}

export const MatchConfirmationDialogs = ({
  showEndPeriodConfirmation,
  currentPeriod,
  isOvertimePeriod,
  currentTimeFormatted,
  periodLoading,
  onEndPeriodConfirm,
  onEndPeriodCancel,
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

  const groupCount: number = groupsToDelete?.length ?? 0;
  const totalEventCount: number = groupsToDelete
    ? groupsToDelete.reduce((sum: number, g: EventGroup) => sum + g.events.length, 0)
    : 0;
  const isBulkMultiGroup: boolean = groupCount > 1;
  const singleGroup: EventGroup | undefined = groupCount === 1 ? groupsToDelete?.[0] : undefined;

  const eventTypeLabel = (type: ProcessedEventType, count: number): string =>
    t(`floorball.matches.manage.confirmDelete.type.${type}`, { count, defaultValue: type });

  let deleteTitle: string = t('floorball.matches.manage.confirmDelete.title', 'Delete event');
  let deleteMessage: string = '';
  let deleteConfirm: string = t('floorball.matches.manage.confirmDelete.confirm', 'Delete');

  if (isBulkMultiGroup) {
    deleteTitle = t('floorball.matches.manage.confirmDelete.titleMany', { count: totalEventCount, defaultValue: 'Delete {{count}} match events' });
    deleteMessage = t('floorball.matches.manage.confirmDelete.messageMany', {
      count: totalEventCount,
      rows: groupCount,
      defaultValue: 'Delete {{count}} selected match events ({{rows}} rows)?',
    });
    deleteConfirm = t('floorball.matches.manage.confirmDelete.confirmCount', { count: totalEventCount, defaultValue: 'Delete {{count}}' });
  } else if (singleGroup) {
    const representative = singleGroup.representative;
    const eventCount: number = singleGroup.events.length;
    const time: string = formatMatchEventTime(representative.periodNumber, representative.timeInSeconds);
    const typeLabel: string = eventTypeLabel(representative.type, eventCount);
    if (eventCount > 1) {
      deleteTitle = t('floorball.matches.manage.confirmDelete.titleGroup', { count: eventCount, type: typeLabel, defaultValue: 'Delete {{count}} {{type}}' });
      deleteMessage = t('floorball.matches.manage.confirmDelete.messageGroup', {
        count: eventCount,
        type: typeLabel,
        team: representative.teamName,
        player: representative.playerName ? ` (${representative.playerName})` : '',
        time,
        defaultValue: 'Delete all {{count}} {{type}} for {{team}}{{player}} at {{time}}?',
      });
      deleteConfirm = t('floorball.matches.manage.confirmDelete.confirmCount', { count: eventCount, defaultValue: 'Delete {{count}}' });
    } else {
      deleteMessage = t('floorball.matches.manage.confirmDelete.messageSingle', {
        type: typeLabel,
        team: representative.teamName,
        time,
        defaultValue: 'Delete {{type}} for {{team}} at {{time}}?',
      });
    }
  }

  return (
    <>
      <ConfirmationDialog
        isOpen={showEndPeriodConfirmation}
        icon="⚠️"
        title={isOvertimePeriod
          ? t('floorball.matches.manage.confirmEndPeriod.titleOvertime', 'End overtime early?')
          : t('floorball.matches.manage.confirmEndPeriod.title', 'End period early?')}
        message={isOvertimePeriod
          ? t('floorball.matches.manage.confirmEndPeriod.messageOvertime', {
            time: currentTimeFormatted,
            defaultValue: 'Are you sure you want to end overtime at {{time}}?',
          })
          : t('floorball.matches.manage.confirmEndPeriod.message', {
            period: currentPeriod,
            time: currentTimeFormatted,
            defaultValue: 'Are you sure you want to end period {{period}} at {{time}}?',
          })}
        warningMessage={t('common.cannotBeUndone', 'This action cannot be undone.')}
        confirmText={isOvertimePeriod
          ? t('floorball.matches.manage.confirmEndPeriod.confirmOvertime', 'End overtime')
          : t('floorball.matches.manage.confirmEndPeriod.confirm', 'End period')}
        isLoading={Boolean(periodLoading[currentPeriod])}
        onConfirm={onEndPeriodConfirm}
        onCancel={onEndPeriodCancel}
      />

      <ConfirmationDialog
        isOpen={showEndMatchConfirmation}
        icon="🏁"
        title={t('floorball.matches.manage.confirmEndMatch.title', 'Finish match?')}
        message={
          isShootout
            ? t('floorball.matches.manage.confirmEndMatch.messageShootout', 'Are you sure you want to finish this match? This will end the shootout.')
            : t('floorball.matches.manage.confirmEndMatch.message', 'Are you sure you want to finish this match?')
        }
        warningMessage={t('floorball.matches.manage.confirmEndMatch.warning', 'This will finalize the match result. You can reopen the match later if needed.')}
        confirmText={t('floorball.matches.manage.confirmEndMatch.confirm', 'Finish match')}
        isLoading={matchLoading}
        onConfirm={onEndMatchConfirm}
        onCancel={onEndMatchCancel}
      />

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

      <ConfirmationDialog
        isOpen={groupCount > 0}
        icon="🗑️"
        title={deleteTitle}
        message={deleteMessage}
        warningMessage={t('common.cannotBeUndone', 'This action cannot be undone.')}
        confirmText={deleteConfirm}
        isLoading={deleteEventLoading}
        onConfirm={onDeleteEventConfirm}
        onCancel={onDeleteEventCancel}
      />
    </>
  );
};

export default MatchConfirmationDialogs;
