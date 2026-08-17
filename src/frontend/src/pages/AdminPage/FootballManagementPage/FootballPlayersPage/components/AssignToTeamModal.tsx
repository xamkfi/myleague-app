import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import { FootballPosition } from '../../../../../types/football/footballTypes';
import { footballTeamNameSearchService } from '../../../../../api/football/footballTeamNameSearchService';
import './AssignToTeamModal.scss';

interface AssignToTeamModalProps {
  isOpen: boolean;
  player: FootballPlayerDto | null;
  onConfirm: (teamId: string, position: FootballPosition, jerseyNumber?: number) => Promise<void>;
  onCancel: () => void;
  isAssigning: boolean;
  bulkCount?: number;
}

interface TeamOption {
  id: string;
  name: string;
}

const AssignToTeamModal = ({ isOpen, player, onConfirm, onCancel, isAssigning, bulkCount }: AssignToTeamModalProps) => {
  const { t } = useTranslation();
  const [selectedTeamId, setSelectedTeamId] = useState<string>('');
  const [position, setPosition] = useState<FootballPosition>(FootballPosition.None);
  const [jerseyNumber, setJerseyNumber] = useState<string>('');
  const [teams, setTeams] = useState<TeamOption[]>([]);
  const [loadingTeams, setLoadingTeams] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchTeams = useCallback (async () => {
    try {
      setLoadingTeams(true);
      const response = await footballTeamNameSearchService.getTeamNames('');
      if (response.success && response.data) {
        setTeams(response.data.map(team => ({ id: team.id, name: team.name })));
      }
    } catch (err) {
      console.error('Failed to fetch teams:', err);
      setError(t('football.teams.errors.loadFailed', 'Failed to load teams'));
    } finally {
      setLoadingTeams(false);
    }
  },[t]);

  // Fetch teams when modal opens
  useEffect(() => {
    if (isOpen) {
      fetchTeams();
      // Reset form
      setSelectedTeamId('');
      setPosition(FootballPosition.None);
      setJerseyNumber('');
      setError(null);
    }
  }, [isOpen, fetchTeams]);

  const handleConfirm = async () => {
    if (!selectedTeamId) {
      setError(t('football.teams.errors.selectTeam', 'Please select a team'));
      return;
    }

    try {
      setError(null);
      const jerseyNum = jerseyNumber ? parseInt(jerseyNumber, 10) : undefined;
      
      if (jerseyNumber && (isNaN(jerseyNum as number) || (jerseyNum as number) < 1 || (jerseyNum as number) > 99)) {
        setError(t('football.teams.errors.invalidJerseyNumber', 'Jersey number must be between 1 and 99'));
        return;
      }

      await onConfirm(selectedTeamId, position, jerseyNum);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to assign player to team');
    }
  };

  if (!isOpen) return null;

  const isBulkMode = bulkCount !== undefined && bulkCount > 0;

  return (
    <div className="modal-overlay">
      <div className="modal-content assign-team-modal">
        <h3 className="modal-title">
          {isBulkMode 
            ? t('football.teams.assignPlayersToTeam', 'Assign Players to Team')
            : t('football.teams.assignPlayerToTeam', 'Assign Player to Team')
          }
        </h3>

        <div className="modal-body">
          {isBulkMode ? (
            <p className="player-info">
              <strong>{t('football.players.selectedPlayers', '{{count}} players selected', { count: bulkCount })}</strong>
            </p>
          ) : player && (
            <p className="player-info">
              <strong>{player.person.fullName || `${player.person.firstName} ${player.person.lastName}`}</strong>
            </p>
          )}

            {error && <div className="error-message">{error}</div>}

            <div className="form-group">
              <label htmlFor="team-select">
                {t('football.teams.selectTeam', 'Select Team')} *
              </label>
              <select
                id="team-select"
                value={selectedTeamId}
                onChange={(e) => setSelectedTeamId(e.target.value)}
                disabled={loadingTeams || isAssigning}
                className="form-select"
              >
                <option value="">
                  {loadingTeams 
                    ? t('common.loading', 'Loading...') 
                    : t('football.teams.chooseTeam', 'Choose a team...')
                  }
                </option>
                {teams.map((team) => (
                  <option key={team.id} value={team.id}>
                    {team.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="position-select">
                {t('football.players.position', 'Position')}
              </label>
              <select
                id="position-select"
                value={position}
                onChange={(e) => setPosition(e.target.value as FootballPosition)}
                disabled={isAssigning}
                className="form-select"
              >
                <option value={FootballPosition.None}>{t('football.positions.none', 'None')}</option>
                <option value={FootballPosition.Goalkeeper}>{t('football.positions.goalkeeper', 'Goalkeeper')}</option>
                <option value={FootballPosition.Defender}>{t('football.positions.defender', 'Defender')}</option>
                <option value={FootballPosition.Midfielder}>{t('football.positions.midfielder', 'Midfielder')}</option>
                <option value={FootballPosition.Forward}>{t('football.positions.forward', 'Forward')}</option>
              </select>
            </div>

            {!isBulkMode && (
              <div className="form-group">
                <label htmlFor="jersey-number">
                  {t('football.players.jerseyNumber', 'Jersey Number')}
                </label>
                <input
                  id="jersey-number"
                  type="number"
                  min="1"
                  max="99"
                  value={jerseyNumber}
                  onChange={(e) => setJerseyNumber(e.target.value)}
                  disabled={isAssigning}
                  className="form-input"
                  placeholder="1-99"
                />
              </div>
            )}
        </div>

        <div className="modal-actions">
          <button
            type="button"
            onClick={onCancel}
            disabled={isAssigning}
            className="btn-cancel"
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            onClick={handleConfirm}
            disabled={isAssigning || !selectedTeamId || loadingTeams}
            className="btn-confirm"
          >
            {isAssigning 
              ? t('common.saving', 'Saving...') 
              : t('football.teams.addToRoster', 'Add to Roster')
            }
          </button>
        </div>
      </div>
    </div>
  );
};

export default AssignToTeamModal;

