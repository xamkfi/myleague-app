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
}

const LiveMatchModalHeader = ({
  homeTeam,
  awayTeam,
  currentMatch,
  isSidesSwapped,
  onToggleSides,
  onClose,
  onCompleteLive
}: LiveMatchModalHeaderProps) => {
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
              <span className="match-status">🏁 FINISHED</span>
              <button onClick={onClose} className="close-modal-button" title="Close the match modal">
                ✕ Close
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