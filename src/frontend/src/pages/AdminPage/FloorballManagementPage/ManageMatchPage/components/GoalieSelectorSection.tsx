import type { ChangeEvent } from 'react';
import { useMemo } from 'react';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import { FloorballPosition } from '../../../../../types/floorball/floorballTypes';

interface GoalieSelectorSectionProps {
  homePlayers: FloorballPlayerDto[];
  awayPlayers: FloorballPlayerDto[];
  homeGoalieId: string;
  awayGoalieId: string;
  setHomeGoalieId: (id: string) => void;
  setAwayGoalieId: (id: string) => void;
}

const GoalieSelectorSection = ({
  homePlayers,
  awayPlayers,
  homeGoalieId,
  awayGoalieId,
  setHomeGoalieId,
  setAwayGoalieId
}: GoalieSelectorSectionProps) => {
  const homeGoalkeepers = useMemo(() => {
    const gks = homePlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : homePlayers;
  }, [homePlayers]);

  const awayGoalkeepers = useMemo(() => {
    const gks = awayPlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : awayPlayers;
  }, [awayPlayers]);

  return (
    <div className="goalie-selector-section">
      <div className="goalie-dropdowns">
        <div className="goalie-dropdown">
          <select value={homeGoalieId} onChange={(e: ChangeEvent<HTMLSelectElement>) => setHomeGoalieId(e.target.value)}>
            <option value="">SELECT GOALIE</option>
            {homeGoalkeepers.map((gk: FloorballPlayerDto) => (
              <option key={gk.id} value={gk.id}>
                {gk.person.firstName} {gk.person.lastName}
              </option>
            ))}
          </select>
        </div>
        <div className="goalie-dropdown">
          <select value={awayGoalieId} onChange={(e: ChangeEvent<HTMLSelectElement>) => setAwayGoalieId(e.target.value)}>
            <option value="">SELECT GOALIE</option>
            {awayGoalkeepers.map((gk: FloorballPlayerDto) => (
              <option key={gk.id} value={gk.id}>
                {gk.person.firstName} {gk.person.lastName}
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
};

export default GoalieSelectorSection;


