import { useMemo, type KeyboardEvent, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type {
  FloorballPlayoffBracketDto,
  FloorballPlayoffMatchDto,
  FloorballPlayoffRoundDto,
  FloorballPlayoffRoundKey,
  FloorballPlayoffTeamDto
} from '../../types/floorball/tournamentTypes';
import './TournamentBracket.scss';

interface TournamentBracketProps {
  bracket: FloorballPlayoffBracketDto;
  /**
   * Compact mode trims the card padding and font sizes so the bracket fits inside narrower
   * containers (e.g. the admin EditTournamentPage right column). Optional.
   */
  compact?: boolean;
  /**
   * Where to navigate when a match card is clicked.
   *  - 'public' (default): navigate to the public match detail page (`/match/{id}`).
   *  - 'admin': navigate to the admin match management view (`/admin/floorball/matches/manage/{id}`).
   */
  linkMode?: 'public' | 'admin';
}

const ROUND_DISPLAY_ORDER: FloorballPlayoffRoundKey[] = [
  'QuarterFinal',
  'SemiFinal',
  'ThirdPlaceMatch',
  'Final'
];

function formatScheduledDateTime(iso: string): string {
  const date = new Date(iso);
  return date.toLocaleString('fi-FI', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

function isCompleted(status: string): boolean {
  return status.toLowerCase() === 'completed';
}

function isInProgress(status: string): boolean {
  const normalized = status.toLowerCase();
  return normalized === 'inprogress' || normalized === 'live' || normalized === 'paused';
}

interface TeamSlotProps {
  team: FloorballPlayoffTeamDto | null;
  resolved: boolean;
  score: number | null;
  isWinner: boolean;
  tbdLabel: string;
}

function TeamSlot({ team, resolved, score, isWinner, tbdLabel }: TeamSlotProps): ReactElement {
  const showTbd = !resolved || team == null;
  return (
    <div className={`tournament-bracket__slot${isWinner ? ' tournament-bracket__slot--winner' : ''}${showTbd ? ' tournament-bracket__slot--tbd' : ''}`}>
      <div className="tournament-bracket__slot-team">
        {showTbd ? (
          <>
            <div className="tournament-bracket__slot-logo tournament-bracket__slot-logo--placeholder" aria-hidden="true" />
            <span className="tournament-bracket__slot-name tournament-bracket__slot-name--tbd">{tbdLabel}</span>
          </>
        ) : (
          <>
            {team!.teamLogo && team!.teamLogo.trim() !== '' ? (
              <img
                className="tournament-bracket__slot-logo"
                src={team!.teamLogo}
                alt={team!.teamName}
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
              />
            ) : (
              <div className="tournament-bracket__slot-logo tournament-bracket__slot-logo--placeholder" aria-hidden="true" />
            )}
            <span className="tournament-bracket__slot-name">{team!.teamName}</span>
          </>
        )}
      </div>
      <div className="tournament-bracket__slot-score">
        {score == null ? '–' : score}
      </div>
    </div>
  );
}

interface MatchCardProps {
  match: FloorballPlayoffMatchDto;
  roundKey: FloorballPlayoffRoundKey;
  onSelect: (matchId: string) => void;
  tbdLabel: string;
  notStartedLabel: string;
  liveLabel: string;
  completedLabel: string;
}

function MatchCard({
  match,
  roundKey,
  onSelect,
  tbdLabel,
  notStartedLabel,
  liveLabel,
  completedLabel
}: MatchCardProps): ReactElement {
  const completed = isCompleted(match.status);
  const live = isInProgress(match.status);
  const homeWon = completed && match.homeScore > match.awayScore;
  const awayWon = completed && match.awayScore > match.homeScore;

  // Show scores only when something has been entered. For not-yet-started matches the slots show "–".
  const showScores = completed || live;

  const statusLabel = completed ? completedLabel : live ? liveLabel : notStartedLabel;

  const handleClick = (): void => {
    onSelect(match.matchId);
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLDivElement>): void => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      onSelect(match.matchId);
    }
  };

  return (
    <div
      className={`tournament-bracket__match${completed ? ' tournament-bracket__match--completed' : ''}${live ? ' tournament-bracket__match--live' : ''}${roundKey === 'ThirdPlaceMatch' ? ' tournament-bracket__match--bronze' : ''}`}
      role="button"
      tabIndex={0}
      onClick={handleClick}
      onKeyDown={handleKeyDown}
    >
      <div className="tournament-bracket__match-header">
        <span className="tournament-bracket__match-date">
          {formatScheduledDateTime(match.scheduledDateTime)}
        </span>
        <span className={`tournament-bracket__match-status tournament-bracket__match-status--${completed ? 'completed' : live ? 'live' : 'scheduled'}`}>
          {statusLabel}
        </span>
      </div>
      <TeamSlot
        team={match.homeTeam}
        resolved={match.isHomeFeederResolved}
        score={showScores ? match.homeScore : null}
        isWinner={homeWon}
        tbdLabel={tbdLabel}
      />
      <TeamSlot
        team={match.awayTeam}
        resolved={match.isAwayFeederResolved}
        score={showScores ? match.awayScore : null}
        isWinner={awayWon}
        tbdLabel={tbdLabel}
      />
      {match.venue && (
        <div className="tournament-bracket__match-venue">
          <i className="fas fa-map-marker-alt" aria-hidden="true"></i>
          {match.venue}
        </div>
      )}
    </div>
  );
}

export default function TournamentBracket({ bracket, compact = false, linkMode = 'public' }: TournamentBracketProps): ReactElement {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const orderedRounds = useMemo<FloorballPlayoffRoundDto[]>(() => {
    const roundMap = new Map<FloorballPlayoffRoundKey, FloorballPlayoffRoundDto>();
    bracket.rounds.forEach((r) => {
      roundMap.set(r.round, r);
    });
    return ROUND_DISPLAY_ORDER
      .map((key) => roundMap.get(key))
      .filter((r): r is FloorballPlayoffRoundDto => r !== undefined);
  }, [bracket.rounds]);

  const handleSelectMatch = (matchId: string): void => {
    const target = linkMode === 'admin'
      ? `/admin/floorball/matches/manage/${matchId}`
      : `/match/${matchId}`;
    navigate(target);
  };

  const tbdLabel = t('tournaments.playoffs.tbd', 'TBD');
  const notStartedLabel = t('tournaments.playoffs.statusScheduled', 'Aikataulutettu');
  const liveLabel = t('tournaments.playoffs.statusLive', 'Käynnissä');
  const completedLabel = t('tournaments.playoffs.statusCompleted', 'Päättynyt');

  if (orderedRounds.length === 0) {
    return (
      <div className="tournament-bracket tournament-bracket--empty">
        <p>{t('tournaments.playoffs.empty', 'Pudotuspelikaaviota ei ole vielä luotu.')}</p>
      </div>
    );
  }

  const roundLabels: Record<FloorballPlayoffRoundKey, string> = {
    QuarterFinal: t('tournaments.playoffs.rounds.quarterfinal', 'Puolivälierä'),
    SemiFinal: t('tournaments.playoffs.rounds.semifinal', 'Välierä'),
    ThirdPlaceMatch: t('tournaments.playoffs.rounds.thirdPlace', 'Pronssiottelu'),
    Final: t('tournaments.playoffs.rounds.final', 'Finaali')
  };

  return (
    <div className={`tournament-bracket${compact ? ' tournament-bracket--compact' : ''}`}>
      {bracket.champion && (
        <div className="tournament-bracket__champion" role="status">
          <span className="tournament-bracket__champion-label">
            <span className="tournament-bracket__champion-icon" aria-hidden="true">🏆</span>
            {t('tournaments.playoffs.champion', 'Mestari')}
          </span>
          <div className="tournament-bracket__champion-team">
            {bracket.champion.teamLogo && bracket.champion.teamLogo.trim() !== '' ? (
              <img
                className="tournament-bracket__champion-logo"
                src={bracket.champion.teamLogo}
                alt={bracket.champion.teamName}
              />
            ) : (
              <div className="tournament-bracket__champion-logo tournament-bracket__champion-logo--placeholder" aria-hidden="true" />
            )}
            <span className="tournament-bracket__champion-name">{bracket.champion.teamName}</span>
          </div>
        </div>
      )}

      <div className="tournament-bracket__scroller">
        <div className="tournament-bracket__rounds">
          {orderedRounds.map((round) => (
            <section key={round.round} className={`tournament-bracket__round tournament-bracket__round--${round.round.toLowerCase()}`}>
              <header className="tournament-bracket__round-header">
                <h3>{roundLabels[round.round]}</h3>
              </header>
              <div className="tournament-bracket__round-matches">
                {round.matches.map((match) => (
                  <MatchCard
                    key={match.matchId}
                    match={match}
                    roundKey={round.round}
                    onSelect={handleSelectMatch}
                    tbdLabel={tbdLabel}
                    notStartedLabel={notStartedLabel}
                    liveLabel={liveLabel}
                    completedLabel={completedLabel}
                  />
                ))}
              </div>
            </section>
          ))}
        </div>
      </div>
    </div>
  );
}
