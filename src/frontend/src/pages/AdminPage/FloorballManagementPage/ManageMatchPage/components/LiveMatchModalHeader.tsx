import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import './LiveMatchModalHeader.scss';

interface LiveMatchModalHeaderProps {
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  currentMatch: FloorballMatchDto;
  isSidesSwapped: boolean;
  onToggleSides: () => void;
  onClose: () => void;
  onCompleteLive: () => void;
  onReopen: () => void;
}

const LiveMatchModalHeader = ({
  homeTeam,
  awayTeam,
  currentMatch,
  isSidesSwapped,
  onToggleSides,
  onClose,
  onCompleteLive,
  onReopen,
}: LiveMatchModalHeaderProps) => {
  const { t } = useTranslation();
  const homeName: string = homeTeam?.name || t('floorball.matches.manage.scoreboard.home', 'Home');
  const awayName: string = awayTeam?.name || t('floorball.matches.manage.scoreboard.away', 'Away');
  const leftTeamName: string = isSidesSwapped ? awayName : homeName;
  const rightTeamName: string = isSidesSwapped ? homeName : awayName;
  const isLive: boolean = currentMatch.status === 'InProgress';
  const isCompleted: boolean = currentMatch.status === 'Completed';

  return (
    <div className={`modal-header${isLive ? ' modal-header--live' : ''}`}>
      <div className="live-match-info">
        <div className="title-and-swap">
          <h2>
            <span className="team-label">{leftTeamName}</span>
            <span className="vs-label">{t('floorball.matches.manage.header.vs', 'vs')}</span>
            <span className="team-label">{rightTeamName}</span>
          </h2>
          <button
            type="button"
            onClick={onToggleSides}
            className="swap-sides-button"
            title={t('floorball.matches.manage.header.swapSidesTitle', 'Swap the visual sides of the teams')}
          >
            <i className="fas fa-exchange-alt" aria-hidden="true"></i>
            {t('floorball.matches.manage.header.swapSides', 'Swap sides')}
          </button>
        </div>
        <div className="status-controls">
          {isCompleted && (
            <>
              <span className="match-status match-status--finished">
                <i className="fas fa-flag-checkered" aria-hidden="true"></i>
                {t('floorball.matches.manage.finished', 'FINISHED')}
              </span>
              <button
                type="button"
                onClick={onReopen}
                className="reopen-match-button"
                title={t('floorball.matches.manage.reopenMatchTitle', 'Reopen this match for editing')}
              >
                <i className="fas fa-lock-open" aria-hidden="true"></i>
                {t('floorball.matches.manage.reopenMatch', 'Open match')}
              </button>
              <button
                type="button"
                onClick={onClose}
                className="close-modal-button"
                title={t('floorball.matches.manage.header.closeTitle', 'Back to the match list')}
              >
                <i className="fas fa-times" aria-hidden="true"></i>
                {t('common.close', 'Close')}
              </button>
            </>
          )}
          {isLive && (
            <>
              <button
                type="button"
                onClick={onCompleteLive}
                className="cancel-live-button"
                title={t('floorball.matches.manage.header.finishMatchTitle', 'Stop live tracking and mark the match as finished')}
              >
                <i className="fas fa-stop" aria-hidden="true"></i>
                {t('floorball.matches.manage.header.finishMatch', 'Finish match')}
              </button>
              <span className="match-status match-status--live">
                <span className="live-dot" aria-hidden="true"></span>
                {t('floorball.matches.manage.header.live', 'LIVE')}
              </span>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default LiveMatchModalHeader;
