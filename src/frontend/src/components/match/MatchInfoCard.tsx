import { useTranslation } from 'react-i18next';
import type { MatchPersonDto } from '../../types/common/matchPersonTypes';
import './MatchInfoCard.scss';

export type MatchDecision = 'overtime' | 'shootout' | null;

interface MatchInfoCardProps {
  decision: MatchDecision;
  /** Overrides the default overtime/shootout wording (e.g. football extra time). */
  decisionLabel?: string;
  referees?: MatchPersonDto[] | null;
  scorekeepers?: MatchPersonDto[] | null;
}

function namesOf(persons: MatchPersonDto[] | null | undefined): string[] {
  return (persons ?? []).map((person) => person.name.trim()).filter((name) => name.length > 0);
}

/**
 * Public match summary card: how the match was decided plus referees and scorekeepers.
 * Each row is rendered only when it has content; the card is omitted entirely when empty.
 */
function MatchInfoCard({ decision, decisionLabel, referees, scorekeepers }: MatchInfoCardProps) {
  const { t } = useTranslation();
  const refereeNames: string[] = namesOf(referees);
  const scorekeeperNames: string[] = namesOf(scorekeepers);

  if (!decision && refereeNames.length === 0 && scorekeeperNames.length === 0) {
    return null;
  }

  return (
    <section className="match-info-card" aria-label={t('matchPage.matchInfo.title', 'Match info')}>
      {decision && (
        <div className="match-info-card__decision">
          <i
            className={decision === 'shootout' ? 'fas fa-bullseye' : 'fas fa-stopwatch'}
            aria-hidden="true"
          ></i>
          <span>
            {decisionLabel ?? (decision === 'shootout'
              ? t('matchPage.matchInfo.decidedInShootout', 'Decided by shootout')
              : t('matchPage.matchInfo.decidedInOvertime', 'Decided in overtime'))}
          </span>
        </div>
      )}

      {(refereeNames.length > 0 || scorekeeperNames.length > 0) && (
        <dl className="match-info-card__officials">
          {refereeNames.length > 0 && (
            <div className="match-info-card__row">
              <dt>{t('matchPage.matchInfo.referees', 'Referees')}</dt>
              <dd>{refereeNames.join(', ')}</dd>
            </div>
          )}
          {scorekeeperNames.length > 0 && (
            <div className="match-info-card__row">
              <dt>{t('matchPage.matchInfo.scorekeepers', 'Scorekeepers')}</dt>
              <dd>{scorekeeperNames.join(', ')}</dd>
            </div>
          )}
        </dl>
      )}
    </section>
  );
}

export default MatchInfoCard;
