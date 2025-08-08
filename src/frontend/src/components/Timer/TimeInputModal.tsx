import { useState, useEffect } from 'react';
import './TimeInputModal.scss';

interface TimeInputModalProps {
  isOpen: boolean;
  currentTime: string;
  onSetTime: (timeInSeconds: number) => void;
  onClose: () => void;
  loading?: boolean;
}

export const TimeInputModal = ({ isOpen, currentTime, onSetTime, onClose, loading = false }: TimeInputModalProps) => {
  const [minutes, setMinutes] = useState<string>('');
  const [seconds, setSeconds] = useState<string>('');
  const [error, setError] = useState<string>('');

  // Parse current time when modal opens
  useEffect(() => {
    if (isOpen && currentTime) {
      const parts = currentTime.split(':');
      if (parts.length >= 2) {
        // Handle both MM:SS and HH:MM:SS formats
        const mins = parts.length === 3 ? parts[1] : parts[0];
        const secs = parts.length === 3 ? parts[2] : parts[1];
        setMinutes(mins);
        setSeconds(secs);
      }
    }
  }, [isOpen, currentTime]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const minutesNum = parseInt(minutes, 10);
    const secondsNum = parseInt(seconds, 10);

    // Validation
    if (isNaN(minutesNum) || isNaN(secondsNum)) {
      setError('Please enter valid numbers');
      return;
    }

    if (minutesNum < 0 || secondsNum < 0) {
      setError('Time cannot be negative');
      return;
    }

    if (secondsNum >= 60) {
      setError('Seconds must be less than 60');
      return;
    }

    if (minutesNum > 999) {
      setError('Minutes cannot exceed 999');
      return;
    }

    const totalSeconds = minutesNum * 60 + secondsNum;
    onSetTime(totalSeconds);
  };

  const handleClose = () => {
    setError('');
    onClose();
  };

  const handleMinutesChange = (value: string) => {
    // Allow only numbers
    if (value === '' || /^\d+$/.test(value)) {
      setMinutes(value);
    }
  };

  const handleSecondsChange = (value: string) => {
    // Allow only numbers, max 2 digits
    if (value === '' || (/^\d{1,2}$/.test(value) && parseInt(value, 10) < 60)) {
      setSeconds(value);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="time-input-modal-overlay" onClick={handleClose}>
      <div className="time-input-modal" onClick={e => e.stopPropagation()}>
        <div className="time-input-modal-header">
          <h3>Set Timer</h3>
          <button className="close-button" onClick={handleClose} disabled={loading}>
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="time-input-form">
          <div className="time-input-section">
            <label>Current Time: <span className="current-time">{currentTime}</span></label>
          </div>

          <div className="time-input-fields">
            <div className="time-field">
              <label htmlFor="minutes">Minutes</label>
              <input
                id="minutes"
                type="text"
                value={minutes}
                onChange={(e) => handleMinutesChange(e.target.value)}
                placeholder="00"
                disabled={loading}
                autoFocus
                autoComplete="off"
              />
            </div>
            
            <div className="time-separator">:</div>
            
            <div className="time-field">
              <label htmlFor="seconds">Seconds</label>
              <input
                id="seconds"
                type="text"
                value={seconds}
                onChange={(e) => handleSecondsChange(e.target.value)}
                placeholder="00"
                disabled={loading}
                maxLength={2}
                autoComplete="off"
              />
            </div>
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          <div className="time-input-actions">
            <button 
              type="button" 
              onClick={handleClose} 
              className="cancel-button"
              disabled={loading}
            >
              Cancel
            </button>
            <button 
              type="submit" 
              className="set-time-button"
              disabled={loading}
            >
              {loading ? 'Setting...' : 'Set Time'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
