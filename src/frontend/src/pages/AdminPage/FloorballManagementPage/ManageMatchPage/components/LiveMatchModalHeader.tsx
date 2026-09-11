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
  const leftTeamName = isSidesSwapped ? awayTeam?.name || 'Away' : homeTeam?.name || 'Home';
  const rightTeamName = isSidesSwapped ? homeTeam?.name || 'Home' : awayTeam?.name || 'Away';

  return (
    <div className="modal-header">
      <div className="live-match-info">
        <div className="title-and-swap">
          <h2>{leftTeamName} vs {rightTeamName}</h2>
          <button
            onClick={onToggleSides}
            className="swap-sides-button"
            title="Swap visual sides for teams"
          >
            ↔ Swap sides
          </button>
        </div>
        <div className="status-controls">
          {currentMatch.status === 'Completed' ? (
            <>
              <span className="match-status">🏁 {t('floorball.matches.manage.finished', 'FINISHED')}</span>
              <button
                onClick={onReopen}
                className="reopen-match-button"
                title={t('floorball.matches.manage.reopenMatchTitle', 'Reopen this match for editing')}
              >
                🔓 {t('floorball.matches.manage.reopenMatch', 'Open match')}
              </button>
              <button onClick={onClose} className="close-modal-button" title="Close the match modal">
                ✕ {t('common.close', 'Close')}
              </button>
            </>
          ) : currentMatch.status === 'InProgress' ? (
            <>
              <button onClick={onCompleteLive} className="cancel-live-button" title="Stop live tracking and mark match as finished">
                ⏹️ Finish Match
              </button>
              <span className="match-status">🔴 LIVE</span>
            </>
          ) : null}
        </div>
      </div>
    </div>
  );
};

export default LiveMatchModalHeader;
