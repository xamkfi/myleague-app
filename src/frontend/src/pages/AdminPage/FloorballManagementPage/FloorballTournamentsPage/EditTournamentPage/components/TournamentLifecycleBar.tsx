import { useEffect, useMemo, useRef, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballTournamentDto } from '../../../../../../types/floorball/tournamentTypes';

type TranslateFn = ReturnType<typeof useTranslation>['t'];
import { FloorballMatchStatus, type FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import TournamentLifecycleConfirmModal, {
  type LifecycleModalVariant,
  type LifecyclePrerequisite,
} from './TournamentLifecycleConfirmModal';
import './TournamentLifecycleBar.scss';

export type LifecycleAction =
  | 'startGroupStage'
  | 'startPlayoffStage'
  | 'complete'
  | 'cancel';

/**
 * Destructive non-lifecycle actions surfaced from the "Lisätoiminnot" menu. Kept
 * separate from {@link LifecycleAction} because deleting hits a different API
 * (`floorballTournamentService.delete`) and must navigate the user away from
 * the edit page instead of refreshing it in place.
 */
export type LifecycleMoreAction = 'delete';

interface TournamentLifecycleBarProps {
  tournament: FloorballTournamentDto;
  matches: ReadonlyArray<FloorballMatchDto>;
  matchesLoading: boolean;
  matchesError: string | null;
  loading: boolean;
  onAction: (action: LifecycleAction) => Promise<void>;
  /**
   * Optional handler for non-lifecycle actions in the "Lisätoiminnot" menu (currently just
   * delete). Omit when delete is not appropriate (e.g. on pages where the parent has no way
   * to navigate away after the tournament disappears).
   */
  onMoreAction?: (action: LifecycleMoreAction) => Promise<void>;
}

interface ActionEligibility {
  readonly visible: boolean;
  readonly enabled: boolean;
  readonly tooltip: string;
  readonly disabledReason: string | null;
  readonly prerequisites: ReadonlyArray<LifecyclePrerequisite>;
}

const SUPPORTED_PLAYOFF_SIZES: ReadonlyArray<number> = [2, 4, 8];

const getStatusLabel = (
  status: string,
  t: TranslateFn
): string => {
  switch (status) {
    case 'Draft':
      return t('floorball.tournaments.status.draft', 'Luonnos');
    case 'GroupStage':
      return t('floorball.tournaments.status.groupStage', 'Alkulohkot');
    case 'PlayoffStage':
      return t('floorball.tournaments.status.playoffStage', 'Pudotuspelit');
    case 'Completed':
      return t('floorball.tournaments.status.completed', 'Päättynyt');
    case 'Cancelled':
      return t('floorball.tournaments.status.cancelled', 'Peruttu');
    default:
      return status;
  }
};

const getStatusBadgeClass = (status: string): string => {
  switch (status) {
    case 'Draft':
      return 'tlb-status-pill--draft';
    case 'GroupStage':
      return 'tlb-status-pill--group';
    case 'PlayoffStage':
      return 'tlb-status-pill--playoff';
    case 'Completed':
      return 'tlb-status-pill--completed';
    case 'Cancelled':
      return 'tlb-status-pill--cancelled';
    default:
      return 'tlb-status-pill--draft';
  }
};

const getStatusDescription = (
  tournament: FloorballTournamentDto,
  t: TranslateFn
): string => {
  const hasPlayoff: boolean = tournament.tournamentRules?.hasPlayoffStage ?? false;
  switch (tournament.tournamentStatus) {
    case 'Draft':
      return t(
        'floorball.tournaments.lifecycle.statusDescription.draft',
        'Turnaus on luonnostilassa. Lisää lohkot ja joukkueet, ja aloita sitten alkulohkot, jotta ottelut voidaan aikatauluttaa.'
      );
    case 'GroupStage':
      return hasPlayoff
        ? t(
            'floorball.tournaments.lifecycle.statusDescription.groupStageWithPlayoff',
            'Alkulohkot käynnissä. Kun kaikki alkulohkojen ottelut on pelattu loppuun, voit siirtyä pudotuspeleihin.'
          )
        : t(
            'floorball.tournaments.lifecycle.statusDescription.groupStageNoPlayoff',
            'Alkulohkot käynnissä. Kun kaikki ottelut on pelattu loppuun, voit päättää turnauksen.'
          );
    case 'PlayoffStage':
      return t(
        'floorball.tournaments.lifecycle.statusDescription.playoffStage',
        'Pudotuspelit käynnissä. Päätä turnaus, kun kaikki pudotuspeliottelut ovat ratkenneet.'
      );
    case 'Completed':
      return t(
        'floorball.tournaments.lifecycle.statusDescription.completed',
        'Turnaus on päättynyt. Lifecycle-toimintoja ei ole enää saatavilla.'
      );
    case 'Cancelled':
      return t(
        'floorball.tournaments.lifecycle.statusDescription.cancelled',
        'Turnaus on peruttu. Lifecycle-toimintoja ei ole enää saatavilla.'
      );
    default:
      return '';
  }
};

export const TournamentLifecycleBar = ({
  tournament,
  matches,
  matchesLoading,
  matchesError,
  loading,
  onAction,
  onMoreAction,
}: TournamentLifecycleBarProps): ReactElement => {
  const { t } = useTranslation();
  const [activeAction, setActiveAction] = useState<LifecycleAction | null>(null);
  // Tracks an active non-lifecycle confirmation (currently only "delete"). Kept separate from
  // `activeAction` so the existing lifecycle confirm modal can stay focused on the four
  // status-transition actions without growing extra branches.
  const [activeMoreAction, setActiveMoreAction] = useState<LifecycleMoreAction | null>(null);
  const [moreOpen, setMoreOpen] = useState<boolean>(false);
  const moreRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent): void => {
      if (moreRef.current && !moreRef.current.contains(event.target as Node)) {
        setMoreOpen(false);
      }
    };
    if (moreOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [moreOpen]);

  const status: string = tournament.tournamentStatus;
  const hasPlayoffStage: boolean = tournament.tournamentRules?.hasPlayoffStage ?? false;
  const teamsAdvancingPerGroup: number =
    tournament.tournamentRules?.teamsAdvancingPerGroup ?? 0;
  const groupCount: number = tournament.groups?.length ?? 0;

  const groupStageMatches: ReadonlyArray<FloorballMatchDto> = useMemo(
    () => matches.filter((m) => Boolean(m.tournamentGroupId)),
    [matches]
  );
  const completedGroupStageMatches: number = useMemo(
    () =>
      groupStageMatches.filter(
        (m) =>
          m.status === FloorballMatchStatus.Completed ||
          m.status === FloorballMatchStatus.Cancelled
      ).length,
    [groupStageMatches]
  );
  const totalGroupStageMatches: number = groupStageMatches.length;

  const totalMatches: number = matches.length;
  const completedMatches: number = useMemo(
    () =>
      matches.filter(
        (m) =>
          m.status === FloorballMatchStatus.Completed ||
          m.status === FloorballMatchStatus.Cancelled
      ).length,
    [matches]
  );

  const playoffSize: number = teamsAdvancingPerGroup * groupCount;
  const playoffSizeSupported: boolean =
    SUPPORTED_PLAYOFF_SIZES.includes(playoffSize);

  const startGroupStageEligibility: ActionEligibility = useMemo<ActionEligibility>(() => {
    const visible: boolean = status === 'Draft';
    if (!visible) {
      return { visible, enabled: false, tooltip: '', disabledReason: null, prerequisites: [] };
    }
    const groupsExist: boolean = groupCount > 0;
    const prereqs: LifecyclePrerequisite[] = [
      {
        label: t(
          'floorball.tournaments.lifecycle.prereq.tournamentInDraft',
          'Turnaus on luonnostilassa.'
        ),
        met: status === 'Draft',
      },
      {
        label: t(
          'floorball.tournaments.lifecycle.prereq.hasAtLeastOneGroup',
          'Turnauksessa on vähintään yksi lohko.'
        ),
        met: groupsExist,
      },
    ];

    const enabled: boolean = groupsExist;
    const disabledReason: string | null = enabled
      ? null
      : t(
          'floorball.tournaments.lifecycle.disabled.noGroups',
          'Lisää ainakin yksi lohko, jotta voit aloittaa alkulohkot.'
        );

    return {
      visible,
      enabled,
      tooltip: enabled
        ? t(
            'floorball.tournaments.lifecycle.tooltip.startGroupStage',
            'Merkitsee turnauksen aktiiviseksi ja paljastaa ottelut yleisösivuilla.'
          )
        : disabledReason ?? '',
      disabledReason,
      prerequisites: prereqs,
    };
  }, [status, groupCount, t]);

  const startPlayoffEligibility: ActionEligibility = useMemo<ActionEligibility>(() => {
    const visible: boolean = status === 'GroupStage' && hasPlayoffStage;
    if (!visible) {
      return { visible, enabled: false, tooltip: '', disabledReason: null, prerequisites: [] };
    }
    const groupStageHasMatches: boolean = totalGroupStageMatches > 0;
    const allGroupStageDone: boolean =
      groupStageHasMatches && completedGroupStageMatches === totalGroupStageMatches;
    const prereqs: LifecyclePrerequisite[] = [
      {
        label: t(
          'floorball.tournaments.lifecycle.prereq.hasPlayoffStageEnabled',
          'Turnauksessa on pudotuspelivaihe käytössä.'
        ),
        met: hasPlayoffStage,
      },
      {
        label: t(
          'floorball.tournaments.lifecycle.prereq.supportedPlayoffSize',
          'Pudotuspelikaavion koko on tuettu (2, 4 tai 8 joukkuetta). Nyt: {{size}}.',
          { size: playoffSize }
        ),
        met: playoffSizeSupported,
      },
      {
        label: t(
          'floorball.tournaments.lifecycle.prereq.allGroupStageMatchesDone',
          'Kaikki alkulohkojen ottelut on pelattu loppuun ({{completed}}/{{total}}).',
          { completed: completedGroupStageMatches, total: totalGroupStageMatches }
        ),
        met: allGroupStageDone,
      },
    ];

    const enabled: boolean = allGroupStageDone && playoffSizeSupported && !matchesLoading;
    let disabledReason: string | null = null;
    if (matchesLoading) {
      disabledReason = t(
        'floorball.tournaments.lifecycle.disabled.loadingMatches',
        'Ladataan otteluiden tila...'
      );
    } else if (!playoffSizeSupported) {
      disabledReason = t(
        'floorball.tournaments.lifecycle.disabled.unsupportedPlayoffSize',
        'Pudotuspelikaavion koko ei ole tuettu ({{size}}). Säädä lohkoja tai jatkajia per lohko niin, että summa on 2, 4 tai 8.',
        { size: playoffSize }
      );
    } else if (!groupStageHasMatches) {
      disabledReason = t(
        'floorball.tournaments.lifecycle.disabled.noGroupStageMatches',
        'Yhtään alkulohkon ottelua ei ole vielä luotu.'
      );
    } else if (!allGroupStageDone) {
      disabledReason = t(
        'floorball.tournaments.lifecycle.disabled.groupStageInProgress',
        '{{completed}}/{{total}} alkulohkon ottelua pelattu. Pelaa kaikki loppuun ennen pudotuspelejä.',
        { completed: completedGroupStageMatches, total: totalGroupStageMatches }
      );
    }

    return {
      visible,
      enabled,
      tooltip: enabled
        ? t(
            'floorball.tournaments.lifecycle.tooltip.startPlayoffStage',
            'Laskee sarjataulukot, luo pudotuspelikaavion ja aikatauluttaa pudotuspeliottelut. Vaatii, että kaikki alkulohkojen ottelut on pelattu.'
          )
        : disabledReason ?? '',
      disabledReason,
      prerequisites: prereqs,
    };
  }, [
    status,
    hasPlayoffStage,
    totalGroupStageMatches,
    completedGroupStageMatches,
    playoffSize,
    playoffSizeSupported,
    matchesLoading,
    t,
  ]);

  const completeEligibility: ActionEligibility = useMemo<ActionEligibility>(() => {
    const visible: boolean =
      status === 'PlayoffStage' || (status === 'GroupStage' && !hasPlayoffStage);
    if (!visible) {
      return { visible, enabled: false, tooltip: '', disabledReason: null, prerequisites: [] };
    }
    const hasAnyMatches: boolean = totalMatches > 0;
    const allDone: boolean = hasAnyMatches && completedMatches === totalMatches;

    const prereqs: LifecyclePrerequisite[] = [
      {
        label: t(
          'floorball.tournaments.lifecycle.prereq.tournamentIsRunning',
          'Turnaus on käynnissä (alkulohkot tai pudotuspelit).'
        ),
        met: status === 'GroupStage' || status === 'PlayoffStage',
      },
      {
        label: t(
          'floorball.tournaments.lifecycle.prereq.allMatchesDone',
          'Kaikki ottelut on pelattu loppuun ({{completed}}/{{total}}).',
          { completed: completedMatches, total: totalMatches }
        ),
        met: allDone,
      },
    ];

    const enabled: boolean = allDone && !matchesLoading;
    let disabledReason: string | null = null;
    if (matchesLoading) {
      disabledReason = t(
        'floorball.tournaments.lifecycle.disabled.loadingMatches',
        'Ladataan otteluiden tila...'
      );
    } else if (!hasAnyMatches) {
      disabledReason = t(
        'floorball.tournaments.lifecycle.disabled.noMatches',
        'Turnauksessa ei ole vielä otteluita.'
      );
    } else if (!allDone) {
      disabledReason = t(
        'floorball.tournaments.lifecycle.disabled.matchesInProgress',
        '{{completed}}/{{total}} ottelua pelattu. Pelaa kaikki loppuun ennen päättämistä.',
        { completed: completedMatches, total: totalMatches }
      );
    }

    return {
      visible,
      enabled,
      tooltip: enabled
        ? t(
            'floorball.tournaments.lifecycle.tooltip.complete',
            'Sulkee turnauksen. Vaatii että kaikki ottelut on pelattu loppuun.'
          )
        : disabledReason ?? '',
      disabledReason,
      prerequisites: prereqs,
    };
  }, [
    status,
    hasPlayoffStage,
    totalMatches,
    completedMatches,
    matchesLoading,
    t,
  ]);

  const cancelEligibility: ActionEligibility = useMemo<ActionEligibility>(() => {
    const visible: boolean = status !== 'Completed' && status !== 'Cancelled';
    return {
      visible,
      enabled: visible,
      tooltip: t(
        'floorball.tournaments.lifecycle.tooltip.cancel',
        'Peruu turnauksen. Toiminto on lopullinen eikä sitä voi peruuttaa.'
      ),
      disabledReason: null,
      prerequisites: [],
    };
  }, [status, t]);

  const primaryAction: LifecycleAction | null = useMemo<LifecycleAction | null>(() => {
    if (startGroupStageEligibility.visible) return 'startGroupStage';
    if (startPlayoffEligibility.visible) return 'startPlayoffStage';
    if (completeEligibility.visible) return 'complete';
    return null;
  }, [startGroupStageEligibility.visible, startPlayoffEligibility.visible, completeEligibility.visible]);

  const renderPrimaryButton = (): ReactElement | null => {
    if (!primaryAction) return null;
    const eligibility: ActionEligibility =
      primaryAction === 'startGroupStage'
        ? startGroupStageEligibility
        : primaryAction === 'startPlayoffStage'
          ? startPlayoffEligibility
          : completeEligibility;

    const label: string =
      primaryAction === 'startGroupStage'
        ? t('floorball.tournaments.lifecycle.startGroupStage', 'Aloita Alkulohkot')
        : primaryAction === 'startPlayoffStage'
          ? t('floorball.tournaments.lifecycle.startPlayoffStage', 'Aloita Pudotuspelit')
          : t('floorball.tournaments.lifecycle.complete', 'Päätä Turnaus');

    const iconClass: string =
      primaryAction === 'startGroupStage'
        ? 'fas fa-play'
        : primaryAction === 'startPlayoffStage'
          ? 'fas fa-trophy'
          : 'fas fa-flag-checkered';

    const disabled: boolean = !eligibility.enabled || loading;

    return (
      <button
        type="button"
        className={`tlb-action tlb-action--primary tlb-action--${primaryAction}`}
        onClick={(): void => setActiveAction(primaryAction)}
        disabled={disabled}
        title={eligibility.tooltip}
        aria-label={label}
      >
        <i className={iconClass} aria-hidden="true"></i>
        <span>{label}</span>
      </button>
    );
  };

  const handleConfirm = async (): Promise<void> => {
    if (!activeAction) return;
    await onAction(activeAction);
    setActiveAction(null);
  };

  const modalConfig: {
    title: string;
    description: string;
    confirmLabel: string;
    variant: LifecycleModalVariant;
    destructiveAcknowledge?: string;
    prerequisites: ReadonlyArray<LifecyclePrerequisite>;
  } | null = useMemo(() => {
    if (!activeAction) return null;
    switch (activeAction) {
      case 'startGroupStage':
        return {
          title: t(
            'floorball.tournaments.lifecycle.confirm.startGroupStage.title',
            'Aloita alkulohkot'
          ),
          description: t(
            'floorball.tournaments.lifecycle.confirm.startGroupStage.description',
            'Merkitsee turnauksen aktiiviseksi. Tämän jälkeen aikataulu ja otteluiden tiedot näkyvät yleisösivuilla.'
          ),
          confirmLabel: t(
            'floorball.tournaments.lifecycle.confirm.startGroupStage.confirm',
            'Aloita alkulohkot'
          ),
          variant: 'default',
          prerequisites: startGroupStageEligibility.prerequisites,
        };
      case 'startPlayoffStage':
        return {
          title: t(
            'floorball.tournaments.lifecycle.confirm.startPlayoffStage.title',
            'Aloita pudotuspelit'
          ),
          description: t(
            'floorball.tournaments.lifecycle.confirm.startPlayoffStage.description',
            'Laskee sarjataulukot, luo pudotuspelikaavion ja aikatauluttaa pudotuspeliottelut. Toimintoa ei voi peruuttaa ilman erillisiä toimia.'
          ),
          confirmLabel: t(
            'floorball.tournaments.lifecycle.confirm.startPlayoffStage.confirm',
            'Aloita pudotuspelit'
          ),
          variant: 'default',
          prerequisites: startPlayoffEligibility.prerequisites,
        };
      case 'complete':
        return {
          title: t(
            'floorball.tournaments.lifecycle.confirm.complete.title',
            'Päätä turnaus'
          ),
          description: t(
            'floorball.tournaments.lifecycle.confirm.complete.description',
            'Sulkee turnauksen lopullisesti. Tämän jälkeen otteluita tai pisteitä ei enää muuteta.'
          ),
          confirmLabel: t(
            'floorball.tournaments.lifecycle.confirm.complete.confirm',
            'Päätä turnaus'
          ),
          variant: 'default',
          prerequisites: completeEligibility.prerequisites,
        };
      case 'cancel':
        return {
          title: t(
            'floorball.tournaments.lifecycle.confirm.cancel.title',
            'Peru turnaus'
          ),
          description: t(
            'floorball.tournaments.lifecycle.confirm.cancel.description',
            'Peruu turnauksen. Tämä on lopullinen toiminto, eikä sitä voi peruuttaa ilman tietokantatoimia. Otteluiden tiedot säilyvät, mutta turnaus merkitään peruutetuksi.'
          ),
          confirmLabel: t(
            'floorball.tournaments.lifecycle.confirm.cancel.confirm',
            'Kyllä, peru turnaus'
          ),
          variant: 'destructive',
          destructiveAcknowledge: t(
            'floorball.tournaments.lifecycle.confirm.cancel.acknowledge',
            'Ymmärrän, että turnauksen peruminen on lopullinen toiminto.'
          ),
          prerequisites: [],
        };
      default:
        return null;
    }
  }, [
    activeAction,
    startGroupStageEligibility.prerequisites,
    startPlayoffEligibility.prerequisites,
    completeEligibility.prerequisites,
    t,
  ]);

  const isTerminalStatus: boolean =
    status === 'Completed' || status === 'Cancelled';

  return (
    <div className="tlb">
      <div className="tlb-status-row">
        <div className="tlb-status-pair">
          <span className="tlb-status-label">
            {t('floorball.tournaments.statusLabel', 'Tila')}:
          </span>
          <span className={`tlb-status-pill ${getStatusBadgeClass(status)}`}>
            {getStatusLabel(status, t)}
          </span>
        </div>
        <p className="tlb-status-description">
          {getStatusDescription(tournament, t)}
        </p>
      </div>

      {(!isTerminalStatus || onMoreAction) && (
        <div className="tlb-actions-row">
          <div className="tlb-actions-primary">{renderPrimaryButton()}</div>

          <div className="tlb-actions-secondary" ref={moreRef}>
            {(cancelEligibility.visible || onMoreAction) && (
              <>
                <button
                  type="button"
                  className="tlb-more-trigger"
                  onClick={(): void => setMoreOpen((prev) => !prev)}
                  aria-haspopup="menu"
                  aria-expanded={moreOpen}
                  aria-label={t(
                    'floorball.tournaments.lifecycle.moreActions',
                    'Lisätoiminnot'
                  )}
                  disabled={loading}
                >
                  <span>
                    {t(
                      'floorball.tournaments.lifecycle.moreActions',
                      'Lisätoiminnot'
                    )}
                  </span>
                  <i
                    className={`fas fa-chevron-${moreOpen ? 'up' : 'down'}`}
                    aria-hidden="true"
                  ></i>
                </button>
                {moreOpen && (
                  <div className="tlb-more-menu" role="menu">
                    {cancelEligibility.visible && (
                      <button
                        type="button"
                        className="tlb-more-item tlb-more-item--danger"
                        onClick={(): void => {
                          setMoreOpen(false);
                          setActiveAction('cancel');
                        }}
                        title={cancelEligibility.tooltip}
                        role="menuitem"
                      >
                        <i className="fas fa-ban" aria-hidden="true"></i>
                        <span>
                          {t(
                            'floorball.tournaments.lifecycle.cancel',
                            'Peru turnaus'
                          )}
                        </span>
                      </button>
                    )}
                    {onMoreAction && (
                      <button
                        type="button"
                        className="tlb-more-item tlb-more-item--danger"
                        onClick={(): void => {
                          setMoreOpen(false);
                          setActiveMoreAction('delete');
                        }}
                        title={t(
                          'floorball.tournaments.lifecycle.tooltip.delete',
                          'Poistaa turnauksen pysyvästi. Toimintoa ei voi peruuttaa.'
                        )}
                        role="menuitem"
                      >
                        <i className="fas fa-trash" aria-hidden="true"></i>
                        <span>
                          {t(
                            'floorball.tournaments.lifecycle.delete',
                            'Poista turnaus'
                          )}
                        </span>
                      </button>
                    )}
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      )}

      {matchesError && !matchesLoading && (
        <p className="tlb-matches-error" role="status">
          <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>{' '}
          {t(
            'floorball.tournaments.lifecycle.matchesLoadError',
            'Otteluiden tilan lataaminen epäonnistui — esiehtoja ei voida vahvistaa. Tarkista ottelujen tilanne ennen lifecycle-toimintoja.'
          )}
        </p>
      )}

      {modalConfig && (
        <TournamentLifecycleConfirmModal
          isOpen={activeAction !== null}
          variant={modalConfig.variant}
          title={modalConfig.title}
          description={modalConfig.description}
          prerequisites={modalConfig.prerequisites}
          confirmLabel={modalConfig.confirmLabel}
          destructiveAcknowledgeLabel={modalConfig.destructiveAcknowledge}
          loading={loading}
          onConfirm={handleConfirm}
          onCancel={(): void => setActiveAction(null)}
        />
      )}

      {activeMoreAction === 'delete' && onMoreAction && (
        <TournamentLifecycleConfirmModal
          isOpen={true}
          variant="destructive"
          title={t(
            'floorball.tournaments.lifecycle.confirm.delete.title',
            'Poista turnaus'
          )}
          description={t(
            'floorball.tournaments.lifecycle.confirm.delete.description',
            'Poistaa turnauksen "{{name}}" pysyvästi. Kaikki turnauksen lohkot, joukkueliitokset ja ottelut poistetaan eikä toimintoa voi peruuttaa.',
            { name: tournament.name }
          )}
          prerequisites={[]}
          confirmLabel={t(
            'floorball.tournaments.lifecycle.confirm.delete.confirm',
            'Kyllä, poista turnaus'
          )}
          destructiveAcknowledgeLabel={t(
            'floorball.tournaments.lifecycle.confirm.delete.acknowledge',
            'Ymmärrän, että turnauksen poistaminen on lopullinen toiminto.'
          )}
          loading={loading}
          onConfirm={async (): Promise<void> => {
            await onMoreAction('delete');
            setActiveMoreAction(null);
          }}
          onCancel={(): void => setActiveMoreAction(null)}
        />
      )}
    </div>
  );
};

export default TournamentLifecycleBar;
