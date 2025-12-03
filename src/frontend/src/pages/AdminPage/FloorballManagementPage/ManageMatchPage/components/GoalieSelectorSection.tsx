import type { ChangeEvent } from 'react';
import './GoalieSelectorSection.scss';
import { useMemo, useState, useCallback } from 'react';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import { FloorballPosition } from '../../../../../types/floorball/floorballTypes';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import ConfirmationDialog from './ConfirmationDialog';

interface GoalieSelectorSectionProps {
  homePlayers: FloorballPlayerDto[];
  awayPlayers: FloorballPlayerDto[];
  homeGoalieId: string;
  awayGoalieId: string;
  setHomeGoalieId: (id: string) => void;
  setAwayGoalieId: (id: string) => void;
  currentMatch: FloorballMatchDto;
  onMatchUpdated: (match: FloorballMatchDto) => void;
  setError: (error: string | null) => void;
}

const GoalieSelectorSection = ({
  homePlayers,
  awayPlayers,
  homeGoalieId,
  awayGoalieId,
  setHomeGoalieId,
  setAwayGoalieId,
  currentMatch,
  onMatchUpdated,
  setError
}: GoalieSelectorSectionProps) => {
  const homeGoalkeepers = useMemo(() => {
    const gks = homePlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : homePlayers;
  }, [homePlayers]);

  const awayGoalkeepers = useMemo(() => {
    const gks = awayPlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : awayPlayers;
  }, [awayPlayers]);

  const [pendingGoalieChange, setPendingGoalieChange] = useState<{ team: 'home' | 'away'; goalieId: string; goalieName: string } | null>(null);

  const changeGoalie = useCallback(async (team: 'home' | 'away', goalieId: string) => {
    try {
      setError(null);
      const teamId = team === 'home' ? currentMatch.homeTeamId : currentMatch.awayTeamId;
      if (!currentMatch.id || !teamId || !goalieId) return;
      const response = await floorballMatchService.changeGoalie(currentMatch.id, teamId, goalieId);
      if (response.success && response.data) {
        onMatchUpdated(response.data);
      } else {
        throw new Error(response.errors?.join(', ') || 'Failed to change goalie');
      }
    } catch (error) {
      console.error(`Error setting ${team} goalie:`, error);
      setError(error instanceof Error ? error.message : `Failed to set ${team} goalie`);
    }
  }, [currentMatch.id, currentMatch.homeTeamId, currentMatch.awayTeamId, onMatchUpdated, setError]);

  const handleSelectChange = useCallback((team: 'home' | 'away', e: ChangeEvent<HTMLSelectElement>) => {
    const selectedId = e.target.value;
    if (team === 'home') {
      setHomeGoalieId(selectedId);
      if (!selectedId) return;
      if (currentMatch.status === 'InProgress' && currentMatch.homeActiveGoalieId !== selectedId) {
        const newGoalie = homePlayers.find(p => p.id === selectedId);
        setPendingGoalieChange({
          team: 'home',
          goalieId: selectedId,
          goalieName: newGoalie ? `${newGoalie.person.firstName} ${newGoalie.person.lastName}` : 'Unknown Player'
        });
      } else {
        changeGoalie('home', selectedId);
      }
    } else {
      setAwayGoalieId(selectedId);
      if (!selectedId) return;
      if (currentMatch.status === 'InProgress' && currentMatch.awayActiveGoalieId !== selectedId) {
        const newGoalie = awayPlayers.find(p => p.id === selectedId);
        setPendingGoalieChange({
          team: 'away',
          goalieId: selectedId,
          goalieName: newGoalie ? `${newGoalie.person.firstName} ${newGoalie.person.lastName}` : 'Unknown Player'
        });
      } else {
        changeGoalie('away', selectedId);
      }
    }
  }, [setHomeGoalieId, setAwayGoalieId, currentMatch.status, currentMatch.homeActiveGoalieId, currentMatch.awayActiveGoalieId, homePlayers, awayPlayers, changeGoalie]);

  return (
    <div className="goalie-selector-section">
      <div className="goalie-dropdowns">
        <div className="goalie-dropdown">
          <div className="goalie-header">GOALKEEPER</div>
          <select value={homeGoalieId} onChange={(e: ChangeEvent<HTMLSelectElement>) => handleSelectChange('home', e)}>
            <option value="">SELECT GOALIE</option>
            {homeGoalkeepers.map((gk: FloorballPlayerDto) => (
              <option key={gk.id} value={gk.id}>
                {gk.person.firstName} {gk.person.lastName}
              </option>
            ))}
          </select>
        </div>
        <div className="goalie-dropdown">
          <div className="goalie-header">GOALKEEPER</div>
          <select value={awayGoalieId} onChange={(e: ChangeEvent<HTMLSelectElement>) => handleSelectChange('away', e)}>
            <option value="">SELECT GOALIE</option>
            {awayGoalkeepers.map((gk: FloorballPlayerDto) => (
              <option key={gk.id} value={gk.id}>
                {gk.person.firstName} {gk.person.lastName}
              </option>
            ))}
          </select>
        </div>
      </div>
      <ConfirmationDialog
        isOpen={!!pendingGoalieChange}
        icon="🔄"
        title="Confirm Goalie Change"
        message={`Are you sure you want to change the ${pendingGoalieChange?.team === 'home' ? 'home' : 'away'} goalkeeper to ${pendingGoalieChange?.goalieName}?`}
        confirmText="Confirm Change"
        isLoading={false}
        onConfirm={async () => {
          if (!pendingGoalieChange) return;
          await changeGoalie(pendingGoalieChange.team, pendingGoalieChange.goalieId);
          setPendingGoalieChange(null);
        }}
        onCancel={() => {
          if (pendingGoalieChange?.team === 'home') {
            setHomeGoalieId(currentMatch.homeActiveGoalieId || '');
          } else {
            setAwayGoalieId(currentMatch.awayActiveGoalieId || '');
          }
          setPendingGoalieChange(null);
        }}
      />
    </div>
  );
};

export default GoalieSelectorSection;


