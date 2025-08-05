import { useState, useMemo, useCallback } from 'react';
import type { ChangeEvent } from 'react';
import type { FloorballMatchDto } from '../../../../../../../types/floorball/floorballTypes';
import type { FloorballPlayerDto } from '../../../../../../../api/floorball/floorballPlayerService';
import { FloorballPosition } from '../../../../../../../types/floorball/floorballTypes';

interface SaveRecordingSectionProps {
  currentMatch: FloorballMatchDto;
  homePlayers: FloorballPlayerDto[];
  awayPlayers: FloorballPlayerDto[];
  onRecordSave: (team: 'home' | 'away', goalieId: string) => void;
  loading: boolean;
}

const SaveRecordingSection = ({
  currentMatch,
  homePlayers,
  awayPlayers,
  onRecordSave,
  loading
}: SaveRecordingSectionProps) => {
  const [homeGoalieId, setHomeGoalieId] = useState<string>('');
  const [awayGoalieId, setAwayGoalieId] = useState<string>('');

  const homeGoalkeepers = useMemo(() => {
    const gks = homePlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : homePlayers;
  }, [homePlayers]);

  const awayGoalkeepers = useMemo(() => {
    const gks = awayPlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : awayPlayers;
  }, [awayPlayers]);

  const handleHomeSave = useCallback(() => {
    onRecordSave('home', homeGoalieId);
  }, [onRecordSave, homeGoalieId]);

  const handleAwaySave = useCallback(() => {
    onRecordSave('away', awayGoalieId);
  }, [onRecordSave, awayGoalieId]);

  return (
    <div className="save-recording-section">
      <div className="goalie-dropdowns">
        <div className="goalie-dropdown">
          <select value={homeGoalieId} onChange={(e: ChangeEvent<HTMLSelectElement>) => setHomeGoalieId(e.target.value)}>
            <option value="">SELECT HOME GOALIE</option>
            {homeGoalkeepers.map((gk: FloorballPlayerDto) => (
              <option key={gk.id} value={gk.id}>
                {gk.person.firstName} {gk.person.lastName}
              </option>
            ))}
          </select>
          <button
            disabled={!homeGoalieId || loading || currentMatch.status !== 'InProgress'}
            onClick={handleHomeSave}
            className="action-btn save-btn"
          >
             Record Home Save
          </button>
        </div>
        <div className="goalie-dropdown">
          <select value={awayGoalieId} onChange={(e: ChangeEvent<HTMLSelectElement>) => setAwayGoalieId(e.target.value)}>
            <option value="">SELECT AWAY GOALIE</option>
            {awayGoalkeepers.map((gk: FloorballPlayerDto) => (
              <option key={gk.id} value={gk.id}>
                {gk.person.firstName} {gk.person.lastName}
              </option>
            ))}
          </select>
          <button
            disabled={!awayGoalieId || loading || currentMatch.status !== 'InProgress'}
            onClick={handleAwaySave}
            className="action-btn save-btn"
          >
             Record Away Save
          </button>
        </div>
      </div>
    </div>
  );
};

export default SaveRecordingSection;
