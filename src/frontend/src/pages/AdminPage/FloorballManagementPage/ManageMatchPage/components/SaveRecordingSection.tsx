import { useCallback } from 'react';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';

interface SaveRecordingSectionProps {
  currentMatch: FloorballMatchDto;
  homeGoalieId: string;
  awayGoalieId: string;
  onRecordSave: (team: 'home' | 'away', goalieId: string) => void;
  loading: boolean;
  keybindsEnabled: boolean;
}
const SaveRecordingSection = ({
  currentMatch,
  homeGoalieId,
  awayGoalieId,
  onRecordSave,
  loading,
  keybindsEnabled
}: SaveRecordingSectionProps) => {
  const handleHomeSave = useCallback(() => onRecordSave('home', homeGoalieId), [onRecordSave, homeGoalieId]);

  const handleAwaySave = useCallback(() => onRecordSave('away', awayGoalieId), [onRecordSave, awayGoalieId]);

  return (
    <div className="save-recording-section">
      <div className="goalie-dropdowns">
        <div className="goalie-dropdown">
          <button
            disabled={!homeGoalieId || loading || currentMatch.status !== 'InProgress'}
            onClick={handleHomeSave}
            className="action-btn save-btn"
          >
            <span className={`key-label ${keybindsEnabled ? '' : 'disabled'}`}>(Q)</span>
            Record Home Save
          </button>
        </div>
        <div className="goalie-dropdown">
          <button
            disabled={!awayGoalieId || loading || currentMatch.status !== 'InProgress'}
            onClick={handleAwaySave}
            className="action-btn save-btn"
          >
            <span className={`key-label ${keybindsEnabled ? '' : 'disabled'}`}>(P)</span>
            Record Away Save
          </button>
        </div>
      </div>
    </div>
  );
};

export default SaveRecordingSection;
