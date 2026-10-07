import type { ReactElement } from 'react';
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

/** The Joomleague importer attaches this placeholder to imported matches; it is not a real person. */
const PLACEHOLDER_NAMES: ReadonlySet<string> = new Set(['import referee']);

function namesOf(persons: MatchPersonDto[] | null | undefined): string[] {
  return (persons ?? [])
    .map((person) => person.name.trim())
    .filter((name) => name.length > 0 && !PLACEHOLDER_NAMES.has(name.toLowerCase()));
}

const WhistleIcon = (): ReactElement => (
  <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true" focusable="false">
    <path
      d="M9 7h11a1 1 0 0 1 1 1v2.5a1 1 0 0 1-.6.9L14 14a5 5 0 1 1-5-7Zm0 3a2 2 0 1 0 0 4 2 2 0 0 0 0-4ZM4 4l2.5 2.5"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const ClipboardIcon = (): ReactElement => (
  <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true" focusable="false">
    <path
      d="M9 4h6v3H9zM7 5.5H6a1 1 0 0 0-1 1V20a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6.5a1 1 0 0 0-1-1h-1M9 12h6M9 16h4"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const StopwatchIcon = (): ReactElement => (
  <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
    <path
      d="M10 2h4M12 9v4l2.5 2M12 21a8 8 0 1 0 0-16 8 8 0 0 0 0 16Z"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

interface OfficialsGroupProps {
  label: string;
  names: string[];
  icon: ReactElement;
}

const OfficialsGroup = ({ label, names, icon }: OfficialsGroupProps): ReactElement => (
  <div className="match-info-card__group">
    <dt className="match-info-card__label">
      <span className="match-info-card__icon">{icon}</span>
      {label}
    </dt>
    <dd className="match-info-card__names">
      <ul>
        {names.map((name) => (
          <li key={name} className="match-info-card__name">{name}</li>
        ))}
      </ul>
    </dd>
  </div>
);

/**
 * Public match summary card: how the match was decided plus referees and scorekeepers.
 * Each part is rendered only when it has content; the card is omitted entirely when empty.
 */
function MatchInfoCard({ decision, decisionLabel, referees, scorekeepers }: MatchInfoCardProps) {
  const { t } = useTranslation();
  const refereeNames: string[] = namesOf(referees);
  const scorekeeperNames: string[] = namesOf(scorekeepers);
  const hasOfficials: boolean = refereeNames.length > 0 || scorekeeperNames.length > 0;

  if (!decision && !hasOfficials) {
    return null;
  }

  return (
    <section className="match-info-card" aria-label={t('matchPage.matchInfo.title', 'Match info')}>
      {decision && (
        <div className="match-info-card__decision">
          <StopwatchIcon />
          <span>
            {decisionLabel ?? (decision === 'shootout'
              ? t('matchPage.matchInfo.decidedInShootout', 'Decided by shootout')
              : t('matchPage.matchInfo.decidedInOvertime', 'Decided in overtime'))}
          </span>
        </div>
      )}

      {hasOfficials && (
        <dl className="match-info-card__officials">
          {refereeNames.length > 0 && (
            <OfficialsGroup
              label={t('matchPage.matchInfo.referees', 'Referees')}
              names={refereeNames}
              icon={<WhistleIcon />}
            />
          )}
          {scorekeeperNames.length > 0 && (
            <OfficialsGroup
              label={t('matchPage.matchInfo.scorekeepers', 'Scorekeepers')}
              names={scorekeeperNames}
              icon={<ClipboardIcon />}
            />
          )}
        </dl>
      )}
    </section>
  );
}

export default MatchInfoCard;
