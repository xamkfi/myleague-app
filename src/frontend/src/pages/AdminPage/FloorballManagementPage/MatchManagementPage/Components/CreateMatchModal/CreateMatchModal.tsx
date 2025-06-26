import React, { useState } from 'react';
import type { 
  FloorballTeam,
  CreateFloorballMatchRequest
} from '../../../../../../types/floorball/floorballTypes';
import type { FloorballSeasonDto } from '../../../../../../api/floorball/floorballSeasonService';
import { formatSeasonDisplayName } from '../../utils/matchFormatters';
import SearchableInfiniteDropdown from '../../../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import { floorballTeamSearchService, floorballSeasonSearchService } from '../../../../../../api/floorball/floorballTeamSearchService';
import './CreateMatchModal.scss';

interface CreateMatchModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (matchData: CreateFloorballMatchRequest) => Promise<void>;
  loading?: boolean;
}

const CreateMatchModal: React.FC<CreateMatchModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  loading = false
}) => {
  const [createForm, setCreateForm] = useState<CreateFloorballMatchRequest>({
    seasonId: '',
    homeTeamId: '',
    awayTeamId: '',
    scheduledDateTime: '',
    venue: ''
  });
  const [dateInput, setDateInput] = useState('');
  const [hoursInput, setHoursInput] = useState('');
  const [minutesInput, setMinutesInput] = useState('');
  const [error, setError] = useState<string | null>(null);

  // Update scheduledDateTime when date or time changes
  const updateScheduledDateTime = (date: string, hours: string, minutes: string) => {
    if (date && hours && minutes) {
      // Date input already provides yyyy-mm-dd format
      const isoDateTime = `${date}T${hours.padStart(2, '0')}:${minutes.padStart(2, '0')}:00`;
      setCreateForm(prev => ({ ...prev, scheduledDateTime: isoDateTime }));
    } else {
      setCreateForm(prev => ({ ...prev, scheduledDateTime: '' }));
    }
  };

  const handleDateChange = (value: string) => {
    setDateInput(value);
    updateScheduledDateTime(value, hoursInput, minutesInput);
  };

  const handleHoursChange = (value: string) => {
    // Validate hours (0-23)
    const numValue = parseInt(value);
    if (value === '' || (numValue >= 0 && numValue <= 23 && value.length <= 2)) {
      setHoursInput(value);
      updateScheduledDateTime(dateInput, value, minutesInput);
    }
  };

  const handleMinutesChange = (value: string) => {
    // Validate minutes (0-59)
    const numValue = parseInt(value);
    if (value === '' || (numValue >= 0 && numValue <= 59 && value.length <= 2)) {
      setMinutesInput(value);
      updateScheduledDateTime(dateInput, hoursInput, value);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!createForm.seasonId || !createForm.homeTeamId || !createForm.awayTeamId || !createForm.scheduledDateTime) {
      setError('Please fill in all required fields');
      return;
    }

    if (createForm.homeTeamId === createForm.awayTeamId) {
      setError('Home team and away team cannot be the same');
      return;
    }

    // Validate time inputs
    if (!dateInput || !hoursInput || !minutesInput) {
      setError('Please enter a valid date and time');
      return;
    }

    try {
      setError(null);
      await onSubmit(createForm);
      
      // Reset form on success
      setCreateForm({
        seasonId: '',
        homeTeamId: '',
        awayTeamId: '',
        scheduledDateTime: '',
        venue: ''
      });
      setDateInput('');
      setHoursInput('');
      setMinutesInput('');
    } catch (error) {
      console.error('Error creating match:', error);
      setError(error instanceof Error ? error.message : 'Failed to create match');
    }
  };

  const handleClose = () => {
    setCreateForm({
      seasonId: '',
      homeTeamId: '',
      awayTeamId: '',
      scheduledDateTime: '',
      venue: ''
    });
    setDateInput('');
    setHoursInput('');
    setMinutesInput('');
    setError(null);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal">
        <div className="modal-header">
          <h2>Create New Match</h2>
          <button onClick={handleClose} className="modal-close">×</button>
        </div>
        
        {error && (
          <div className="error-alert">
            <span className="error-icon">⚠️</span>
            <span className="error-text">{error}</span>
            <button onClick={() => setError(null)} className="error-close">×</button>
          </div>
        )}
        
        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label htmlFor="season">Season *</label>
            <SearchableInfiniteDropdown
              placeholder="Select Season"
              value={createForm.seasonId}
              onChange={(value) => setCreateForm(prev => ({ ...prev, seasonId: value }))}
              onSearch={floorballSeasonSearchService.searchSeasons}
              searchPlaceholder="Search seasons..."
              emptyMessage="No seasons found"
              required
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="homeTeam">Home Team *</label>
            <SearchableInfiniteDropdown
              placeholder="Select Home Team"
              value={createForm.homeTeamId}
              onChange={(value) => setCreateForm(prev => ({ ...prev, homeTeamId: value }))}
              onSearch={floorballTeamSearchService.searchTeams}
              searchPlaceholder="Search teams..."
              emptyMessage="No teams found"
              required
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="awayTeam">Away Team *</label>
            <SearchableInfiniteDropdown
              placeholder="Select Away Team"
              value={createForm.awayTeamId}
              onChange={(value) => setCreateForm(prev => ({ ...prev, awayTeamId: value }))}
              onSearch={floorballTeamSearchService.searchTeams}
              searchPlaceholder="Search teams..."
              emptyMessage="No teams found"
              required
            />
          </div>
          
          <div className="form-group">
            <label>Date & Time *</label>
            <div className="datetime-input-group">
              <div className="date-input">
                <input
                  type="date"
                  value={dateInput}
                  onChange={(e) => handleDateChange(e.target.value)}
                  required
                />
              </div>
              <div className="time-input-group">
                <input
                  type="number"
                  placeholder="HH"
                  value={hoursInput}
                  onChange={(e) => handleHoursChange(e.target.value)}
                  min="0"
                  max="23"
                  className="time-input hours"
                  required
                />
                <span className="time-separator">:</span>
                <input
                  type="number"
                  placeholder="MM"
                  value={minutesInput}
                  onChange={(e) => handleMinutesChange(e.target.value)}
                  min="0"
                  max="59"
                  className="time-input minutes"
                  required
                />
              </div>
            </div>
          </div>
          
          <div className="form-group">
            <label htmlFor="venue">Venue</label>
            <input
              type="text"
              id="venue"
              value={createForm.venue}
              onChange={(e) => setCreateForm(prev => ({ ...prev, venue: e.target.value }))}
              placeholder="Enter venue (optional)"
            />
          </div>
          
          <div className="modal-actions">
            <button type="button" onClick={handleClose} className="cancel-button">
              Cancel
            </button>
            <button type="submit" disabled={loading} className="submit-button">
              {loading ? 'Creating...' : 'Create Match'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateMatchModal; 