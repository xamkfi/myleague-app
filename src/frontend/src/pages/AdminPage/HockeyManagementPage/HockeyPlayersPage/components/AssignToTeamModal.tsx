import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HOCKEY_POSITIONS, type HockeyPosition } from '../../../../../types/hockey/hockeyTypes';
import { hockeyTeamService } from '../../../../../api/hockey/hockeyTeamService';
import './AssignToTeamModal.scss';
import type { HockeyPlayerListRow } from './PlayersTable';

interface AssignToTeamModalProps {
  isOpen: boolean;
  player: HockeyPlayerListRow | null;
  onConfirm: (teamId: string, position: HockeyPosition, jerseyNumber?: number) => Promise<void>;
  onCancel: () => void;
  isAssigning: boolean;
}

function AssignToTeamModal({ isOpen, player, onConfirm, onCancel, isAssigning }: AssignToTeamModalProps) {
  const { t } = useTranslation();
  const [selectedTeamId, setSelectedTeamId] = useState('');
  const [position, setPosition] = useState<HockeyPosition>('Center');
  const [jerseyNumber, setJerseyNumber] = useState('');
  const [teams, setTeams] = useState<Array<{ id: string; name: string }>>([]);
  const [loadingTeams, setLoadingTeams] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchTeams = useCallback(async (): Promise<void> => {
    try {
      setLoadingTeams(true);
      const list = await hockeyTeamService.getAll();
      setTeams(list.map((team) => ({ id: team.id, name: team.name })));
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.teams.errors.loadFailed', 'Failed to load teams'));
    } finally {
      setLoadingTeams(false);
    }
  }, [t]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }
    setSelectedTeamId('');
    setPosition((player?.position as HockeyPosition) || 'Center');
    setJerseyNumber('');
    setError(null);
    void fetchTeams();
  }, [isOpen, player, fetchTeams]);

  const handleConfirm = async (): Promise<void> => {
    if (!selectedTeamId) {
      setError(t('hockey.teams.errors.selectTeam', 'Please select a team'));
      return;
    }
    const jerseyNum = jerseyNumber ? parseInt(jerseyNumber, 10) : undefined;
    if (jerseyNumber && (jerseyNum === undefined || Number.isNaN(jerseyNum) || jerseyNum < 1 || jerseyNum > 99)) {
      setError(t('hockey.teams.errors.invalidJerseyNumber', 'Jersey number must be between 1 and 99'));
      return;
    }
    try {
      setError(null);
      await onConfirm(selectedTeamId, position, jerseyNum);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to assign player to team');
    }
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className="modal-overlay">
      <div className="modal-content assign-team-modal">
        <h3 className="modal-title">
          {t('hockey.teams.assignPlayerToTeam', 'Assign Player to Team')}
        </h3>
        <div className="modal-body">
          {player && (
            <p className="player-info">
              <strong>{player.name}</strong>
            </p>
          )}
          {error && <div className="error-message">{error}</div>}
          <div className="form-group">
            <label htmlFor="hockey-team-select">
              {t('hockey.teams.selectTeam', 'Select Team')} *
            </label>
            <select
              id="hockey-team-select"
              value={selectedTeamId}
              onChange={(event) => setSelectedTeamId(event.target.value)}
              disabled={loadingTeams || isAssigning}
              className="form-select"
            >
              <option value="">
                {loadingTeams
                  ? t('common.loading', 'Loading...')
                  : t('hockey.teams.chooseTeam', 'Choose a team...')}
              </option>
              {teams.map((team) => (
                <option key={team.id} value={team.id}>{team.name}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="hockey-position-select">
              {t('hockey.roster.position', 'Position')}
            </label>
            <select
              id="hockey-position-select"
              value={position}
              onChange={(event) => setPosition(event.target.value as HockeyPosition)}
              disabled={isAssigning}
              className="form-select"
            >
              {HOCKEY_POSITIONS.map((item) => (
                <option key={item} value={item}>{item}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="hockey-jersey-number">
              {t('hockey.roster.jersey', 'Jersey Number')}
            </label>
            <input
              id="hockey-jersey-number"
              type="number"
              min={1}
              max={99}
              value={jerseyNumber}
              onChange={(event) => setJerseyNumber(event.target.value)}
              disabled={isAssigning}
              className="form-input"
              placeholder="1-99"
            />
          </div>
        </div>
        <div className="modal-actions">
          <button type="button" onClick={onCancel} disabled={isAssigning} className="btn-cancel">
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            onClick={() => void handleConfirm()}
            disabled={isAssigning || !selectedTeamId || loadingTeams}
            className="btn-confirm"
          >
            {isAssigning
              ? t('common.saving', 'Saving...')
              : t('hockey.teams.addToRoster', 'Add to Roster')}
          </button>
        </div>
      </div>
    </div>
  );
}

export default AssignToTeamModal;
