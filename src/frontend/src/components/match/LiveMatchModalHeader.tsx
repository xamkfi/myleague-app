import { useTranslation } from 'react-i18next';
import './LiveMatchModalHeader.scss';

interface HeaderTeam {
  name: string;
}

interface LiveMatchModalHeaderProps {
  homeTeam: HeaderTeam | null;
  awayTeam: HeaderTeam | null;
  isLive: boolean;
  isFinished: boolean;
  isSidesSwapped: boolean;
  onToggleSides: () => void;
  onClose: () => void;
  onCompleteLive: () => void;
  onReopen: () => void;
  /** Shows "Revert to not started" while live. Only for a 0-0 match with no recorded events. */
  canRevertToScheduled?: boolean;
  onRevertToScheduled?: () => void;
}

const LiveMatchModalHeader = ({
  homeTeam,
  awayTeam,
  isLive,
  isFinished,
  isSidesSwapped,
  onToggleSides,
  onClose,
  onCompleteLive,
  onReopen,
  canRevertToScheduled = false,
  onRevertToScheduled,
}: LiveMatchModalHeaderProps) => {
  const { t } = useTranslation();
  const homeName: string = homeTeam?.name || t('matchManage.scoreboard.home', 'Home');
  const awayName: string = awayTeam?.name || t('matchManage.scoreboard.away', 'Away');
  const leftTeamName: string = isSidesSwapped ? awayName : homeName;
  const rightTeamName: string = isSidesSwapped ? homeName : awayName;

  return (
    <div className={`modal-header${isLive ? ' modal-header--live' : ''}`}>
      <div className="live-match-info">
        <div className="title-and-swap">
          <h2>
            <span className="team-label">{leftTeamName}</span>
            <span className="vs-label">{t('matchManage.header.vs', 'vs')}</span>
            <span className="team-label">{rightTeamName}</span>
          </h2>
          <button
            type="button"
            onClick={onToggleSides}
            className="swap-sides-button"
            title={t('matchManage.header.swapSidesTitle', 'Swap the visual sides of the teams')}
          >
            <i className="fas fa-exchange-alt" aria-hidden="true"></i>
            {t('matchManage.header.swapSides', 'Swap sides')}
          </button>
        </div>
        <div className="status-controls">
          {isFinished && (
            <>
              <span className="match-status match-status--finished">
                <i className="fas fa-flag-checkered" aria-hidden="true"></i>
                {t('matchManage.header.finished', 'FINISHED')}
              </span>
              <button
                type="button"
                onClick={onReopen}
                className="reopen-match-button"
                title={t('matchManage.header.reopenMatchTitle', 'Reopen this match for editing')}
              >
                <i className="fas fa-lock-open" aria-hidden="true"></i>
                {t('matchManage.header.reopenMatch', 'Open match')}
              </button>
              <button
                type="button"
                onClick={onClose}
                className="close-modal-button"
                title={t('matchManage.header.closeTitle', 'Back to the match list')}
              >
                <i className="fas fa-times" aria-hidden="true"></i>
                {t('common.close', 'Close')}
              </button>
            </>
          )}
          {isLive && (
            <>
              {canRevertToScheduled && onRevertToScheduled && (
                <button
                  type="button"
                  onClick={onRevertToScheduled}
                  className="revert-match-button"
                  title={t('matchManage.header.revertToScheduledTitle', 'Return the match to not started. Only possible at 0-0 with no recorded events.')}
                >
                  <i className="fas fa-undo" aria-hidden="true"></i>
                  {t('matchManage.header.revertToScheduled', 'Revert to not started')}
                </button>
              )}
              <button
                type="button"
                onClick={onCompleteLive}
                className="cancel-live-button"
                title={t('matchManage.header.finishMatchTitle', 'Stop live tracking and mark the match as finished')}
              >
                <i className="fas fa-stop" aria-hidden="true"></i>
                {t('matchManage.header.finishMatch', 'Finish match')}
              </button>
              <span className="match-status match-status--live">
                <span className="live-dot" aria-hidden="true"></span>
                {t('matchManage.header.live', 'LIVE')}
              </span>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default LiveMatchModalHeader;
