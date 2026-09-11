import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import MatchRow from '../MatchRow';
import type {
  FloorballPlayoffRoundKey,
  FloorballTournamentDto,
  PlayoffScheduleSlotDto,
} from '../../types/floorball/tournamentTypes';
import './PlannedPlayoffSchedule.scss';

interface Props {
  tournament: FloorballTournamentDto;
  /**
   * When true, the schedule is hidden behind a toggle so regular fixtures stay primary.
   * Used on the public tournament Otteluohjelma tab.
   */
  collapsible?: boolean;
  /** Initial open state when `collapsible` is true. Defaults to collapsed. */
  defaultExpanded?: boolean;
  /** Adds spacing suited for rendering below the main match list. */
  afterMatchList?: boolean;
}

/**
 * Renders the tournament's pre-defined playoff bracket slots as muted "TBD vs TBD" rows above
 * the actual match list. Lets end-users see the full programme — including playoff kickoff
 * times — before the bracket is generated.
 *
 * Visibility rules:
 *  - No-op when the tournament has no `playoffSchedule`.
 *  - Hidden once the tournament status reaches `PlayoffStage`/`Completed` because real playoff
 *    matches exist by then and the StartPlayoffStage handler uses these very slots for them.
 */
export default function PlannedPlayoffSchedule({
  tournament,
  collapsible = false,
  defaultExpanded = false,
  afterMatchList = false
}: Props) {
  const { t } = useTranslation();
  const [expanded, setExpanded] = useState<boolean>(() => !collapsible || defaultExpanded);

  const visible = useMemo(() => {
    if (!tournament.playoffSchedule || tournament.playoffSchedule.length === 0) return false;
    // Once the bracket has been generated the schedule lives in the real matches list, so
    // hide the placeholders to avoid showing each row twice.
    return tournament.tournamentStatus !== 'PlayoffStage' && tournament.tournamentStatus !== 'Completed';
  }, [tournament.playoffSchedule, tournament.tournamentStatus]);

  // Sort the schedule chronologically so the list reads top-down by kickoff time. Same-time
  // slots fall back to (round, order) to keep adjacent bracket positions next to each other.
  const orderedSlots = useMemo<PlayoffScheduleSlotDto[]>(() => {
    if (!tournament.playoffSchedule) return [];
    const ROUND_ORDER: Record<FloorballPlayoffRoundKey, number> = {
      QuarterFinal: 1,
      SemiFinal: 2,
      ThirdPlaceMatch: 3,
      Final: 4,
    };
    return [...tournament.playoffSchedule].sort((a, b) => {
      const aTime = new Date(a.scheduledDateTime).getTime();
      const bTime = new Date(b.scheduledDateTime).getTime();
      if (aTime !== bTime) return aTime - bTime;
      const aRound = ROUND_ORDER[a.round] ?? 99;
      const bRound = ROUND_ORDER[b.round] ?? 99;
      if (aRound !== bRound) return aRound - bRound;
      return a.order - b.order;
    });
  }, [tournament.playoffSchedule]);

  if (!visible) return null;

  const tbd = t('floorball.tournaments.playoffSchedule.tbd', 'TBD');
  const tooltip = t(
    'floorball.tournaments.playoffSchedule.placeholderTooltip',
    'This is a planned playoff slot. The actual match will be generated when the group stage finishes.',
  );
  const slotCount = orderedSlots.length;
  const rootClassName = [
    'planned-playoff-schedule',
    afterMatchList ? 'planned-playoff-schedule--after-list' : '',
    collapsible && !expanded ? 'planned-playoff-schedule--collapsed' : ''
  ]
    .filter(Boolean)
    .join(' ');

  const toggleLabel = expanded
    ? t('floorball.tournaments.playoffSchedule.toggleHide', 'Piilota pudotuspeliaikataulu')
    : t('floorball.tournaments.playoffSchedule.toggleShow', 'Näytä pudotuspeliaikataulu ({{count}})', {
        count: slotCount
      });

  return (
    <div className={rootClassName}>
      {collapsible && (
        <button
          type="button"
          className="planned-playoff-schedule__toggle"
          onClick={() => setExpanded((open) => !open)}
          aria-expanded={expanded}
        >
          <span className="planned-playoff-schedule__toggle-label">{toggleLabel}</span>
          <i
            className={`fas fa-chevron-${expanded ? 'up' : 'down'} planned-playoff-schedule__toggle-icon`}
            aria-hidden="true"
          />
        </button>
      )}

      {(!collapsible || expanded) && (
        <>
          <div className="planned-playoff-schedule__header">
            <span className="planned-playoff-schedule__title">
              {t('floorball.tournaments.playoffSchedule.title', 'Planned playoff matches')}
            </span>
            <span className="planned-playoff-schedule__hint">
              {t(
                'floorball.tournaments.playoffSchedule.hint',
                'Teams are filled in automatically after the group stage finishes.',
              )}
            </span>
          </div>
          <div className="planned-playoff-schedule__rows">
            {orderedSlots.map((slot) => {
              const label = formatSlotLabel(slot, t);
              return (
                <MatchRow
                  key={`${slot.round}-${slot.order}`}
                  id={`planned-${slot.round}-${slot.order}`}
                  scheduledDateTime={slot.scheduledDateTime}
                  homeTeamName={tbd}
                  awayTeamName={tbd}
                  statusComponent={
                    <span className="planned-playoff-schedule__badge">
                      {label}
                      {slot.venue ? ` · ${slot.venue}` : ''}
                    </span>
                  }
                  isPlaceholder
                  placeholderTooltip={tooltip}
                />
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}

/**
 * Builds the human-readable round label shown in the row badge. Uses i18n with sensible
 * English fallbacks so an admin who hasn't run the translation extraction still sees
 * meaningful text.
 */
function formatSlotLabel(slot: PlayoffScheduleSlotDto, t: (key: string, fallback: string) => string): string {
  switch (slot.round) {
    case 'QuarterFinal':
      return `${t('floorball.tournaments.playoffSchedule.rounds.qf', 'QF')}${slot.order + 1}`;
    case 'SemiFinal':
      return `${t('floorball.tournaments.playoffSchedule.rounds.sf', 'SF')}${slot.order + 1}`;
    case 'ThirdPlaceMatch':
      return t('floorball.tournaments.playoffSchedule.rounds.thirdPlace', '3rd place');
    case 'Final':
      return t('floorball.tournaments.playoffSchedule.rounds.final', 'Final');
    default:
      return `${slot.round} #${slot.order + 1}`;
  }
}
