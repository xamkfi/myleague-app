import { useTranslation } from 'react-i18next';
import type { HockeyMatchDto } from '../../../../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished, isHockeyMatchLive } from '../../../../../types/hockey/hockeyTypes';
import './LiveMatchModalHeader.scss';

interface NamedTeam {
  name: string;
}

interface LiveMatchModalHeaderProps {
  homeTeam: NamedTeam | null;
  awayTeam: NamedTeam | null;
  currentMatch: HockeyMatchDto;
  isSidesSwapped: boolean;
  onToggleSides: () => void;
  onClose: () => void;
  onCompleteLive: () => void;
  onReopen: () => void;
}

function LiveMatchModalHeader({
  homeTeam,
  awayTeam,
  currentMatch,
  isSidesSwapped,
  onToggleSides,
  onClose,
  onCompleteLive,
  onReopen,
}: LiveMatchModalHeaderProps) {
  const { t } = useTranslation();
  const leftTeamName = isSidesSwapped ? awayTeam?.name || 'Away' : homeTeam?.name || 'Home';
  const rightTeamName = isSidesSwapped ? homeTeam?.name || 'Home' : awayTeam?.name || 'Away';

  return (
    <div className="modal-header">
      <div className="live-match-info">
        <div className="title-and-swap">
          <h2>{leftTeamName} vs {rightTeamName}</h2>
          <button
            type="button"
            onClick={onToggleSides}
            className="swap-sides-button"
            title="Swap visual sides for teams"
          >
            ↔ Swap sides
          </button>
        </div>
        <div className="status-controls">
          {isHockeyMatchFinished(currentMatch.status) ? (
            <>
              <span className="match-status">🏁 {t('hockey.matches.manage.finished', 'FINISHED')}</span>
              <button
                type="button"
                onClick={onReopen}
                className="reopen-match-button"
                title={t('hockey.matches.manage.reopenMatchTitle', 'Reopen this match for editing')}
              >
                🔓 {t('hockey.matches.manage.reopenMatch', 'Open match')}
              </button>
              <button type="button" onClick={onClose} className="close-modal-button" title="Close the match modal">
                ✕ {t('common.close', 'Close')}
              </button>
            </>
          ) : isHockeyMatchLive(currentMatch.status) ? (
            <>
              <button type="button" onClick={onCompleteLive} className="cancel-live-button" title="Stop live tracking and mark match as finished">
                ⏹️ Finish Match
              </button>
              <span className="match-status">🔴 LIVE</span>
            </>
          ) : null}
        </div>
      </div>
    </div>
  );
}

export default LiveMatchModalHeader;
