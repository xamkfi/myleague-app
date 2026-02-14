import type { ChangeEvent } from 'react';
import './GoalieSelectorSection.scss';
import { useMemo, useState, useCallback } from 'react';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import { FloorballPosition } from '../../../../../types/floorball/floorballTypes';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import ConfirmationDialog from './ConfirmationDialog';

interface GoalieSelectorSectionProps {
  leftPlayers: FloorballPlayerDto[];
  rightPlayers: FloorballPlayerDto[];
  leftGoalieId: string;
  rightGoalieId: string;
  leftTeamSide: 'home' | 'away';
  rightTeamSide: 'home' | 'away';
  setHomeGoalieId: (id: string) => void;
  setAwayGoalieId: (id: string) => void;
  currentMatch: FloorballMatchDto;
  onMatchUpdated: (match: FloorballMatchDto) => void;
  setError: (error: string | null) => void;
}

const GoalieSelectorSection = ({
  leftPlayers,
  rightPlayers,
  leftGoalieId,
  rightGoalieId,
  leftTeamSide,
  rightTeamSide,
  setHomeGoalieId,
  setAwayGoalieId,
  currentMatch,
  onMatchUpdated,
  setError
}: GoalieSelectorSectionProps) => {
  const leftGoalkeepers = useMemo(() => {
    const gks = leftPlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : leftPlayers;
  }, [leftPlayers]);

  const rightGoalkeepers = useMemo(() => {
    const gks = rightPlayers.filter((p: FloorballPlayerDto) => p.position === FloorballPosition.Goalkeeper && p.isActive);
    return gks.length > 0 ? gks : rightPlayers;
  }, [rightPlayers]);

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
        const newGoalie = (leftTeamSide === 'home' ? leftPlayers : rightPlayers).find(p => p.id === selectedId);
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
        const newGoalie = (leftTeamSide === 'away' ? leftPlayers : rightPlayers).find(p => p.id === selectedId);
        setPendingGoalieChange({
          team: 'away',
          goalieId: selectedId,
          goalieName: newGoalie ? `${newGoalie.person.firstName} ${newGoalie.person.lastName}` : 'Unknown Player'
        });
      } else {
        changeGoalie('away', selectedId);
      }
    }
  }, [setHomeGoalieId, setAwayGoalieId, currentMatch.status, currentMatch.homeActiveGoalieId, currentMatch.awayActiveGoalieId, leftPlayers, rightPlayers, leftTeamSide, changeGoalie]);

  return (
    <div className="goalie-selector-section">
      <div className="goalie-dropdowns">
        <div className="goalie-dropdown">
          <div className="goalie-header">GOALKEEPER</div>
          <select
            value={leftGoalieId}
            onChange={(e: ChangeEvent<HTMLSelectElement>) => handleSelectChange(leftTeamSide, e)}
          >
            <option value="">SELECT GOALIE</option>
            {leftGoalkeepers.map((gk: FloorballPlayerDto) => (
              <option key={gk.id} value={gk.id}>
                {gk.person.firstName} {gk.person.lastName}
              </option>
            ))}
          </select>
        </div>
        <div className="goalie-dropdown">
          <div className="goalie-header">GOALKEEPER</div>
          <select
            value={rightGoalieId}
            onChange={(e: ChangeEvent<HTMLSelectElement>) => handleSelectChange(rightTeamSide, e)}
          >
            <option value="">SELECT GOALIE</option>
            {rightGoalkeepers.map((gk: FloorballPlayerDto) => (
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


