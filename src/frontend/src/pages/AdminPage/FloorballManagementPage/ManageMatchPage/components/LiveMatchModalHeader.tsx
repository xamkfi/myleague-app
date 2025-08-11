import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';

interface LiveMatchModalHeaderProps {
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  currentMatch: FloorballMatchDto;
  onClose: () => void;
  onCompleteLive: () => void;
}

const LiveMatchModalHeader = ({
  homeTeam,
  awayTeam,
  currentMatch,
  onClose,
  onCompleteLive
}: LiveMatchModalHeaderProps) => {
  return (
    <div className="modal-header">
      <div className="match-info">
        <h2>{homeTeam?.name || 'Home'} vs {awayTeam?.name || 'Away'}</h2>
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
              <span className="match-status">🔴 LIVE</span>
              <button onClick={onCompleteLive} className="cancel-live-button" title="Stop live tracking and mark match as finished">
                ⏹️ Finish Match
              </button>
            </>
          ) : (
            <>
              <span className="match-status">⏸️ READY</span>
            </>
          )}
        </div>
      </div>
      <button onClick={onClose} className="close-button">×</button>
    </div>
  );
};

export default LiveMatchModalHeader; 