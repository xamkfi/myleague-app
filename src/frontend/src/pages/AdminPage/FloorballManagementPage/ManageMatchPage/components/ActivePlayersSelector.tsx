import { useMemo, useState } from 'react';
import './ActivePlayersSelector.scss';
import type { ChangeEvent } from 'react';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import GoalieSelectorSection from './GoalieSelectorSection';

interface ActivePlayersSelectorProps {
  leftPlayers: FloorballPlayerDto[];
  rightPlayers: FloorballPlayerDto[];
  leftTeamName?: string;
  rightTeamName?: string;
  leftTeamSide: 'home' | 'away';
  rightTeamSide: 'home' | 'away';
  // Goalie selection (required to start match)
  leftGoalieId: string;
  rightGoalieId: string;
  setHomeGoalieId: (id: string) => void;
  setAwayGoalieId: (id: string) => void;
  currentMatch: FloorballMatchDto;
  onMatchUpdated: (match: FloorballMatchDto) => void;
  setError: (error: string | null) => void;
}

const renderPlayerOptions = (players: FloorballPlayerDto[]) => (
  <>
    <option value="">Select active player</option>
    {players.map((p) => (
      <option key={p.id} value={p.id}>
        {p.person.firstName} {p.person.lastName}
      </option>
    ))}
  </>
);

const ActivePlayersSelector = ({
  leftPlayers,
  rightPlayers,
  leftTeamName,
  rightTeamName,
  leftTeamSide,
  rightTeamSide,
  leftGoalieId,
  rightGoalieId,
  setHomeGoalieId,
  setAwayGoalieId,
  currentMatch,
  onMatchUpdated,
  setError
}: ActivePlayersSelectorProps) => {
  const activeLeftPlayers = useMemo(() => leftPlayers.filter((p) => p.isActive), [leftPlayers]);
  const activeRightPlayers = useMemo(() => rightPlayers.filter((p) => p.isActive), [rightPlayers]);

  const [leftSelected, setLeftSelected] = useState<string[]>(['', '', '', '', '']);
  const [rightSelected, setRightSelected] = useState<string[]>(['', '', '', '', '']);

  const handleChange = (
    side: 'left' | 'right',
    index: number,
    e: ChangeEvent<HTMLSelectElement>
  ) => {
    const value = e.target.value;
    if (side === 'left') {
      const next = [...leftSelected];
      next[index] = value;
      setLeftSelected(next);
    } else {
      const next = [...rightSelected];
      next[index] = value;
      setRightSelected(next);
    }
  };

  return (
    <div className="active-players-selector">
      <div className="aps-title">ACTIVE PLAYERS</div>
      <div className="team-selects left-team-selects">
        <div className="team-name left">{leftTeamName || 'Left Team'}</div>
        {leftSelected.map((value, idx) => (
          <select
            key={`left-active-${idx}`}
            value={value}
            onChange={(e) => handleChange('left', idx, e)}
            aria-label={`Left active player ${idx + 1}`}
          >
            {renderPlayerOptions(activeLeftPlayers)}
          </select>
        ))}
      </div>
      <div className="team-selects right-team-selects">
        <div className="team-name right">{rightTeamName || 'Right Team'}</div>
        {rightSelected.map((value, idx) => (
          <select
            key={`right-active-${idx}`}
            value={value}
            onChange={(e) => handleChange('right', idx, e)}
            aria-label={`Right active player ${idx + 1}`}
          >
            {renderPlayerOptions(activeRightPlayers)}
          </select>
        ))}
      </div>
      <div className="goalkeeper-row">
        <GoalieSelectorSection
          leftPlayers={leftPlayers}
          rightPlayers={rightPlayers}
          leftGoalieId={leftGoalieId}
          rightGoalieId={rightGoalieId}
          leftTeamSide={leftTeamSide}
          rightTeamSide={rightTeamSide}
          setHomeGoalieId={setHomeGoalieId}
          setAwayGoalieId={setAwayGoalieId}
          currentMatch={currentMatch}
          onMatchUpdated={onMatchUpdated}
          setError={setError}
        />
      </div>
    </div>
  );
};

export default ActivePlayersSelector;


