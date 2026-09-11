import { useEffect, useMemo, useRef } from 'react';
import './CardRecordingForm.scss';
import { FootballCardType, type FootballMatchDto, type FootballTeam } from '../../../../../types/football/footballTypes';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import type { CardForm } from './types';
import { formatPlayerOptionLabel, sortPlayersForSelect } from './eventFormHelpers';

interface CardRecordingFormProps {
  showCardForm: boolean;
  cardForm: CardForm;
  setCardForm: React.Dispatch<React.SetStateAction<CardForm>>;
  currentMatch: FootballMatchDto;
  homeTeam: FootballTeam | null;
  awayTeam: FootballTeam | null;
  loading: boolean;
  getPlayersForTeam: (teamId: string) => FootballPlayerDto[];
  onRecordCard: () => Promise<void>;
  onClose: () => void;
}

const CARD_TYPE_OPTIONS: ReadonlyArray<{ value: FootballCardType; label: string }> = [
  { value: FootballCardType.Yellow, label: 'Yellow' },
  { value: FootballCardType.SecondYellow, label: 'Second Yellow' },
  { value: FootballCardType.DirectRed, label: 'Direct Red' },
];

const DESCRIPTION_MAX_LENGTH: number = 280;

const clampInt = (raw: string, min: number, max: number): number => {
  const parsed: number = parseInt(raw, 10);
  if (Number.isNaN(parsed)) return min;
  return Math.max(min, Math.min(max, parsed));
};

const CardRecordingForm = ({
  showCardForm,
  cardForm,
  setCardForm,
  currentMatch,
  homeTeam,
  awayTeam,
  loading,
  getPlayersForTeam,
  onRecordCard,
  onClose,
}: CardRecordingFormProps) => {
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);

  const sortedPlayers: FootballPlayerDto[] = useMemo(
    () => (cardForm.teamId ? sortPlayersForSelect(getPlayersForTeam(cardForm.teamId)) : []),
    [cardForm.teamId, getPlayersForTeam],
  );

  const selectedPlayer: FootballPlayerDto | undefined = sortedPlayers.find((p) => p.id === cardForm.playerId);
  const missingJersey: boolean = !!(cardForm.playerId && selectedPlayer?.jerseyNumber === undefined);
  const canSubmit: boolean =
    !!cardForm.playerId && cardForm.cardType !== null && !missingJersey && !loading;

  const selectedTeamName: string | undefined =
    cardForm.teamId === currentMatch.homeTeamId ? homeTeam?.name : awayTeam?.name;

  const cardTypeValue: string =
    cardForm.cardType === null || cardForm.cardType === undefined ? '' : String(cardForm.cardType);

  useEffect(() => {
    if (showCardForm) {
      firstFieldRef.current?.focus();
    }
  }, [showCardForm]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (e.key === 'Escape') {
      e.preventDefault();
      e.stopPropagation();
      onClose();
      return;
    }
    if (e.key === 'Enter') {
      const target = e.target as HTMLElement;
      if (target?.tagName === 'TEXTAREA') return;
      if (canSubmit) {
        e.preventDefault();
        e.stopPropagation();
        void onRecordCard();
      }
    }
  };

  if (!showCardForm) return null;

  return (
    <div className="card-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="card-record-modal"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="card-record-modal-title"
      >
        <div className="card-record-modal__header">
          <h3 id="card-record-modal-title">Record card for {selectedTeamName ?? 'team'}</h3>
          <button
            className="card-record-modal__close"
            onClick={onClose}
            disabled={loading}
            type="button"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <div className="card-record-modal__body">
          <div className="event-form card-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="card-player">Receiving player</label>
                <select
                  id="card-player"
                  ref={firstFieldRef}
                  className={`select-field${cardForm.playerId ? '' : ' is-placeholder'}`}
                  value={cardForm.playerId}
                  onChange={(e) => setCardForm((prev) => ({ ...prev, playerId: e.target.value }))}
                >
                  <option value="">Select player</option>
                  {sortedPlayers.map((player) => (
                    <option key={player.id} value={player.id}>
                      {formatPlayerOptionLabel(player)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="card-type">Card type</label>
                <select
                  id="card-type"
                  className={`select-field${cardTypeValue ? '' : ' is-placeholder'}`}
                  value={cardTypeValue}
                  onChange={(e) => {
                    const next: string = e.target.value;
                    setCardForm((prev) => ({
                      ...prev,
                      cardType: next === '' ? null : (Number(next) as FootballCardType),
                    }));
                  }}
                >
                  <option value="">Select card</option>
                  {CARD_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field field--time">
                <label htmlFor="card-time-minutes">Time</label>
                <div className="time-input-group">
                  <input
                    id="card-time-minutes"
                    type="number"
                    className="time-input time-input-minutes"
                    value={cardForm.timeMinutes}
                    onChange={(e) =>
                      setCardForm((prev) => ({ ...prev, timeMinutes: clampInt(e.target.value, 0, 99) }))
                    }
                    min={0}
                    max={99}
                    placeholder="MM"
                    aria-label="Minutes"
                  />
                  <span className="time-separator" aria-hidden="true">
                    :
                  </span>
                  <input
                    id="card-time-seconds"
                    type="number"
                    className="time-input time-input-seconds"
                    value={cardForm.timeSeconds}
                    onChange={(e) =>
                      setCardForm((prev) => ({ ...prev, timeSeconds: clampInt(e.target.value, 0, 59) }))
                    }
                    min={0}
                    max={59}
                    placeholder="SS"
                    aria-label="Seconds"
                  />
                </div>
              </div>
            </div>

            <div className="field field--description">
              <label htmlFor="card-description">
                Description <span className="field-hint">(optional)</span>
              </label>
              <textarea
                id="card-description"
                value={cardForm.description}
                onChange={(e) => setCardForm((prev) => ({ ...prev, description: e.target.value }))}
                placeholder="E.g. unsporting behaviour, dissent, serious foul play…"
                className="description-input"
                maxLength={DESCRIPTION_MAX_LENGTH}
                rows={3}
              />
              <div className="description-counter" aria-live="polite">
                {cardForm.description.length}/{DESCRIPTION_MAX_LENGTH}
              </div>
            </div>

            {missingJersey && (
              <div className="field-error" role="alert">
                Selected player has no jersey number. Assign a jersey before recording the card.
              </div>
            )}

            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
                Cancel
              </button>
              <button
                onClick={onRecordCard}
                disabled={!canSubmit}
                className="submit-btn"
                type="button"
              >
                {loading ? 'Recording…' : missingJersey ? 'Missing jersey' : 'Record Card'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CardRecordingForm;
