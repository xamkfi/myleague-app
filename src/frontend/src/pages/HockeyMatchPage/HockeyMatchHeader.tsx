import { useMatchTimer } from '../../hooks/useMatchTimer';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { formatMatchHeaderDate, getTeamInitials } from '../../components/match/matchHeaderUtils';
import { slugify } from '../../utils/slugUtils';
import { isHockeyMatchFinished, isHockeyMatchLive, type HockeyMatchDto } from '../../types/hockey/hockeyTypes';
import '../../components/match/MatchScoreHeader.scss';
import './HockeyMatchHeader.scss';

interface HockeyMatchHeaderProps {
  match: HockeyMatchDto;
  homeName: string;
  awayName: string;
}

function HockeyLiveClock({ matchId }: { matchId: string }) {
  const { displayTime, initialLoadComplete } = useMatchTimer({
    matchId,
    autoConnect: true,
  });
  return (
    <span className="match-clock">{initialLoadComplete ? displayTime : '--:--'}</span>
  );
}

function HockeyMatchHeader({ match, homeName, awayName }: HockeyMatchHeaderProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const scheduled = formatMatchHeaderDate(match.scheduledStartTime);
  const homeClickable = Boolean(match.homeTeamId);
  const awayClickable = Boolean(match.awayTeamId);
  const live = isHockeyMatchLive(match.status);

  const goToTeam = (teamName: string, teamId: string | null): void => {
    if (!teamId) {
      return;
    }
    navigate(`/hockey/team/${slugify(teamName)}`);
  };

  return (
    <div className="match-header">
      <div className="teams-container">
        <div
          className={`team-section home${homeClickable ? ' clickable' : ''}`}
          role={homeClickable ? 'link' : undefined}
          tabIndex={homeClickable ? 0 : undefined}
          onClick={() => goToTeam(homeName, match.homeTeamId)}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              goToTeam(homeName, match.homeTeamId);
            }
          }}
        >
          <div className="team-crest">{getTeamInitials(homeName)}</div>
          <div className="team-name">{homeName}</div>
        </div>

        <div className="score-container">
          {match.status === 'Scheduled' ? (
            <div className="vs-separator">VS</div>
          ) : (
            <div className="match-score">
              <span className="home-score">{match.homeScore}</span>
              <span className="score-separator">—</span>
              <span className="away-score">{match.awayScore}</span>
            </div>
          )}
        </div>

        <div
          className={`team-section away${awayClickable ? ' clickable' : ''}`}
          role={awayClickable ? 'link' : undefined}
          tabIndex={awayClickable ? 0 : undefined}
          onClick={() => goToTeam(awayName, match.awayTeamId)}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              goToTeam(awayName, match.awayTeamId);
            }
          }}
        >
          <div className="team-crest">{getTeamInitials(awayName)}</div>
          <div className="team-name">{awayName}</div>
        </div>
      </div>

      <div className="match-date-time">
        <span className="weekday">{scheduled.weekday}</span>
        <span className="separator">·</span>
        <span className="date">{scheduled.date}</span>
        <span className="separator">·</span>
        <span className="time">{scheduled.time}</span>
      </div>

      {live && (
        <div className="match-status live">
          <span className="status-dot" aria-label={t('hockeyPage.live', 'Live')} />
          <span>{t('hockeyPage.live', 'LIVE')}</span>
          {match.currentPeriodNumber > 0 && (
            <span>
              · {t('hockeyPage.livePeriod', 'P{{number}}', { number: match.currentPeriodNumber })}
            </span>
          )}
          <HockeyLiveClock matchId={match.id} />
        </div>
      )}

      {isHockeyMatchFinished(match.status) && (
        <div className="match-status final">
          <span>{t('hockeyPage.final', 'FINAL')}</span>
        </div>
      )}
    </div>
  );
}

export default HockeyMatchHeader;
