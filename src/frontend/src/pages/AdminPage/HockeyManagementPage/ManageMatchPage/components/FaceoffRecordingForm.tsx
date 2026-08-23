import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import {
  HOCKEY_FACEOFF_SPOTS_BY_ZONE,
  HOCKEY_FACEOFF_ZONES,
  type HockeyFaceoffSpot,
  type HockeyFaceoffZone,
} from '../../../../../types/hockey/hockeyTypes';
import './GoalRecordingForm.scss';
import { formatPlayerOptionLabel, sortPlayersForSelect, type HockeyFormPlayer } from './eventFormHelpers';

interface FaceoffRecordingFormProps {
  showFaceoffForm: boolean;
  homeTeamId: string;
  awayTeamId: string;
  homeTeamName: string;
  awayTeamName: string;
  homePlayers: HockeyFormPlayer[];
  awayPlayers: HockeyFormPlayer[];
  winningMatchTeamId: string;
  zone: HockeyFaceoffZone;
  spot: HockeyFaceoffSpot;
  winningPlayerId: string;
  losingPlayerId: string;
  loading: boolean;
  onWinnerChange: (teamId: string) => void;
  onZoneChange: (zone: HockeyFaceoffZone) => void;
  onSpotChange: (spot: HockeyFaceoffSpot) => void;
  onWinningPlayerChange: (playerId: string) => void;
  onLosingPlayerChange: (playerId: string) => void;
  onRecordFaceoff: () => Promise<void>;
  onClose: () => void;
}

function FaceoffRecordingForm({
  showFaceoffForm,
  homeTeamId,
  awayTeamId,
  homeTeamName,
  awayTeamName,
  homePlayers,
  awayPlayers,
  winningMatchTeamId,
  zone,
  spot,
  winningPlayerId,
  losingPlayerId,
  loading,
  onWinnerChange,
  onZoneChange,
  onSpotChange,
  onWinningPlayerChange,
  onLosingPlayerChange,
  onRecordFaceoff,
  onClose,
}: FaceoffRecordingFormProps) {
  const { t } = useTranslation();
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);
  const canSubmit = Boolean(winningMatchTeamId) && Boolean(zone) && Boolean(spot) && !loading;
  const spots = HOCKEY_FACEOFF_SPOTS_BY_ZONE[zone];
  const winningPlayers = useMemo(
    () => sortPlayersForSelect(winningMatchTeamId === awayTeamId ? awayPlayers : homePlayers),
    [winningMatchTeamId, awayTeamId, awayPlayers, homePlayers],
  );
  const losingPlayers = useMemo(
    () => sortPlayersForSelect(winningMatchTeamId === awayTeamId ? homePlayers : awayPlayers),
    [winningMatchTeamId, awayTeamId, awayPlayers, homePlayers],
  );

  useEffect(() => {
    if (showFaceoffForm) {
      firstFieldRef.current?.focus();
    }
  }, [showFaceoffForm]);

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>): void => {
    if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
      onClose();
      return;
    }
    if (event.key === 'Enter') {
      const target = event.target as HTMLElement;
      if (target?.tagName === 'TEXTAREA') {
        return;
      }
      if (canSubmit) {
        event.preventDefault();
        event.stopPropagation();
        void onRecordFaceoff();
      }
    }
  };

  if (!showFaceoffForm) {
    return null;
  }

  return (
    <div className="goal-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="goal-record-modal"
        onClick={(event) => event.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="faceoff-record-modal-title"
      >
        <div className="goal-record-modal__header">
          <h3 id="faceoff-record-modal-title">{t('hockey.matches.recordFaceoff', 'Record face-off')}</h3>
          <button className="goal-record-modal__close" onClick={onClose} disabled={loading} type="button" aria-label={t('common.close', 'Close')}>×</button>
        </div>
        <div className="goal-record-modal__body">
          <div className="event-form goal-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="faceoff-winner">{t('hockey.matches.faceoffWinner', 'Winning side')}</label>
                <select
                  id="faceoff-winner"
                  ref={firstFieldRef}
                  className={`select-field${winningMatchTeamId ? '' : ' is-placeholder'}`}
                  value={winningMatchTeamId}
                  onChange={(event) => onWinnerChange(event.target.value)}
                >
                  <option value="">{t('hockey.matches.selectSide', 'Select side')}</option>
                  {homeTeamId && <option value={homeTeamId}>{homeTeamName} ({t('hockey.matches.home', 'Home')})</option>}
                  {awayTeamId && <option value={awayTeamId}>{awayTeamName} ({t('hockey.matches.away', 'Away')})</option>}
                </select>
              </div>
              <div className="field">
                <label htmlFor="faceoff-zone">{t('hockey.matches.faceoffZone', 'Zone')}</label>
                <select
                  id="faceoff-zone"
                  className="select-field"
                  value={zone}
                  onChange={(event) => onZoneChange(event.target.value as HockeyFaceoffZone)}
                >
                  {HOCKEY_FACEOFF_ZONES.map((item) => (
                    <option key={item} value={item}>{t(`hockey.matches.faceoffZones.${item}`, item)}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="faceoff-spot">{t('hockey.matches.faceoffSpot', 'Spot')}</label>
                <select
                  id="faceoff-spot"
                  className="select-field"
                  value={spot}
                  onChange={(event) => onSpotChange(event.target.value as HockeyFaceoffSpot)}
                >
                  {spots.map((item) => (
                    <option key={item} value={item}>{t(`hockey.matches.faceoffSpots.${item}`, item)}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="faceoff-winner-player">
                  {t('hockey.matches.faceoffWinnerPlayer', 'Winning player')} <span className="field-hint">({t('hockey.matches.optional', 'optional')})</span>
                </label>
                <select
                  id="faceoff-winner-player"
                  className={`select-field${winningPlayerId ? '' : ' is-placeholder'}`}
                  value={winningPlayerId}
                  onChange={(event) => onWinningPlayerChange(event.target.value)}
                >
                  <option value="">{t('hockey.matches.selectPlayer', 'Select player')}</option>
                  {winningPlayers.map((player) => (
                    <option key={player.id} value={player.id}>{formatPlayerOptionLabel(player)}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="faceoff-loser-player">
                  {t('hockey.matches.faceoffLoserPlayer', 'Losing player')} <span className="field-hint">({t('hockey.matches.optional', 'optional')})</span>
                </label>
                <select
                  id="faceoff-loser-player"
                  className={`select-field${losingPlayerId ? '' : ' is-placeholder'}`}
                  value={losingPlayerId}
                  onChange={(event) => onLosingPlayerChange(event.target.value)}
                >
                  <option value="">{t('hockey.matches.selectPlayer', 'Select player')}</option>
                  {losingPlayers.map((player) => (
                    <option key={player.id} value={player.id}>{formatPlayerOptionLabel(player)}</option>
                  ))}
                </select>
              </div>
            </div>
            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => void onRecordFaceoff()} disabled={!canSubmit} className="submit-btn" type="button">
                {loading ? t('hockey.matches.recording', 'Recording…') : t('hockey.matches.recordFaceoff', 'Record face-off')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default FaceoffRecordingForm;
