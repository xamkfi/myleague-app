import { useMemo, useState } from 'react';
import './ActivePlayersSelector.scss';
import type { ChangeEvent } from 'react';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import GoalieSelectorSection from './GoalieSelectorSection';

interface ActivePlayersSelectorProps {
  homePlayers: FloorballPlayerDto[];
  awayPlayers: FloorballPlayerDto[];
  homeTeamName?: string;
  awayTeamName?: string;
  // Goalie selection (required to start match)
  homeGoalieId: string;
  awayGoalieId: string;
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

const ActivePlayersSelector = ({ homePlayers, awayPlayers, homeTeamName, awayTeamName, homeGoalieId, awayGoalieId, setHomeGoalieId, setAwayGoalieId, currentMatch, onMatchUpdated, setError }: ActivePlayersSelectorProps) => {
  const activeHomePlayers = useMemo(() => homePlayers.filter((p) => p.isActive), [homePlayers]);
  const activeAwayPlayers = useMemo(() => awayPlayers.filter((p) => p.isActive), [awayPlayers]);

  const [homeSelected, setHomeSelected] = useState<string[]>(['', '', '', '', '']);
  const [awaySelected, setAwaySelected] = useState<string[]>(['', '', '', '', '']);

  const handleChange = (
    team: 'home' | 'away',
    index: number,
    e: ChangeEvent<HTMLSelectElement>
  ) => {
    const value = e.target.value;
    if (team === 'home') {
      const next = [...homeSelected];
      next[index] = value;
      setHomeSelected(next);
    } else {
      const next = [...awaySelected];
      next[index] = value;
      setAwaySelected(next);
    }
  };

  return (
    <div className="active-players-selector">
      <div className="aps-title">ACTIVE PLAYERS</div>
      <div className="team-selects home-team-selects">
        <div className="team-name home">{homeTeamName || 'Home Team'}</div>
        {homeSelected.map((value, idx) => (
          <select
            key={`home-active-${idx}`}
            value={value}
            onChange={(e) => handleChange('home', idx, e)}
            aria-label={`Home active player ${idx + 1}`}
          >
            {renderPlayerOptions(activeHomePlayers)}
          </select>
        ))}
      </div>
      <div className="team-selects away-team-selects">
        <div className="team-name away">{awayTeamName || 'Away Team'}</div>
        {awaySelected.map((value, idx) => (
          <select
            key={`away-active-${idx}`}
            value={value}
            onChange={(e) => handleChange('away', idx, e)}
            aria-label={`Away active player ${idx + 1}`}
          >
            {renderPlayerOptions(activeAwayPlayers)}
          </select>
        ))}
      </div>
      <div className="goalkeeper-row">
        <GoalieSelectorSection
          homePlayers={homePlayers}
          awayPlayers={awayPlayers}
          homeGoalieId={homeGoalieId}
          awayGoalieId={awayGoalieId}
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


