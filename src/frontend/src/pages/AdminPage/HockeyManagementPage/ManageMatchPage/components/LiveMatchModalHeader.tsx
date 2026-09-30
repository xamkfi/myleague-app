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
  const homeName: string = homeTeam?.name || t('hockey.matches.manage.home', 'Home');
  const awayName: string = awayTeam?.name || t('hockey.matches.manage.away', 'Away');
  const leftTeamName: string = isSidesSwapped ? awayName : homeName;
  const rightTeamName: string = isSidesSwapped ? homeName : awayName;

  return (
    <div className="modal-header">
      <div className="live-match-info">
        <div className="title-and-swap">
          <h2>{leftTeamName} vs {rightTeamName}</h2>
          <button
            type="button"
            onClick={onToggleSides}
            className="swap-sides-button"
            title={t('hockey.matches.manage.swapSidesTitle', 'Swap visual sides for teams')}
          >
            ↔ {t('hockey.matches.manage.swapSides', 'Swap sides')}
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
              <button type="button" onClick={onClose} className="close-modal-button" title={t('hockey.matches.manage.closeTitle', 'Close match management')}>
                ✕ {t('common.close', 'Close')}
              </button>
            </>
          ) : isHockeyMatchLive(currentMatch.status) ? (
            <>
              <button
                type="button"
                onClick={onCompleteLive}
                className="cancel-live-button"
                title={t('hockey.matches.manage.finishMatchTitle', 'Stop live tracking and mark match as finished')}
              >
                ⏹️ {t('hockey.matches.manage.finishMatch', 'Finish match')}
              </button>
              <span className="match-status">🔴 {t('hockey.matches.manage.live', 'LIVE')}</span>
            </>
          ) : null}
        </div>
      </div>
    </div>
  );
}

export default LiveMatchModalHeader;
