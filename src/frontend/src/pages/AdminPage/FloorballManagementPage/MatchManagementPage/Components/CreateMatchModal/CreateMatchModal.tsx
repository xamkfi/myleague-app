import React, { useState } from 'react';
import type { 
  CreateFloorballMatchRequest
} from '../../../../../../types/floorball/floorballTypes';
import SearchableInfiniteDropdown from '../../../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import { floorballSeasonSearchService } from '../../../../../../api/floorball/floorballTeamSearchService';
import { floorballTeamNameSearchService } from '../../../../../../api/floorball/floorballTeamNameSearchService';
import { floorballRefereeSearchService } from '../../../../../../api/floorball/floorballRefereeSearchService';
import './CreateMatchModal.scss';

interface CreateMatchModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (matchData: CreateFloorballMatchRequest) => Promise<void>;
  loading?: boolean;
}

const CreateMatchModal = ({
  isOpen,
  onClose,
  onSubmit,
  loading = false
}: CreateMatchModalProps) => {
  const [createForm, setCreateForm] = useState<CreateFloorballMatchRequest>({
    seasonId: '',
    homeTeamId: '',
    awayTeamId: '',
    refereeId: '',
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
      // Create a local date object from the user input
      const localDateTime = new Date(`${date}T${hours.padStart(2, '0')}:${minutes.padStart(2, '0')}:00`);
      
      // Convert to ISO string with timezone offset to ensure proper timezone handling
      const isoDateTime = localDateTime.toISOString();
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
        refereeId: '',
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
      refereeId: '',
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
      <div className="modal create-match-modal">
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
          <div className="form-group create-match-form-row">
            <label htmlFor="season">Season *</label>
            <div className="input-wrapper">
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
          </div>
          <div className="form-group create-match-form-row">
            <label htmlFor="homeTeam">Home Team *</label>
            <div className="input-wrapper">
              <SearchableInfiniteDropdown
                placeholder="Select Home Team"
                value={createForm.homeTeamId}
                onChange={(value) => setCreateForm(prev => ({ ...prev, homeTeamId: value }))}
                onSearch={floorballTeamNameSearchService.searchTeams}
                searchPlaceholder="Search teams..."
                emptyMessage="No teams found"
                required
              />
            </div>
          </div>
          <div className="form-group create-match-form-row">
            <label htmlFor="awayTeam">Away Team *</label>
            <div className="input-wrapper">
              <SearchableInfiniteDropdown
                placeholder="Select Away Team"
                value={createForm.awayTeamId}
                onChange={(value) => setCreateForm(prev => ({ ...prev, awayTeamId: value }))}
                onSearch={floorballTeamNameSearchService.searchTeams}
                searchPlaceholder="Search teams..."
                emptyMessage="No teams found"
                required
              />
            </div>
          </div>
          <div className="form-group create-match-form-row">
            <label htmlFor="referee">Referee</label>
            <div className="input-wrapper">
              <SearchableInfiniteDropdown
                placeholder="Select Referee"
                value={createForm.refereeId}
                onChange={(value) => setCreateForm(prev => ({ ...prev, refereeId: value }))}
                onSearch={floorballRefereeSearchService.searchReferees}
                searchPlaceholder="Search referees..."
                emptyMessage="No referees found"
              />
            </div>
          </div>
          <div className="form-group create-match-form-row">
            <label>Date & Time *</label>
            <div className="input-wrapper">
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
          </div>
          <div className="form-group create-match-form-row">
            <label htmlFor="venue">Venue</label>
            <div className="input-wrapper">
              <input
                type="text"
                id="venue"
                value={createForm.venue}
                onChange={(e) => setCreateForm(prev => ({ ...prev, venue: e.target.value }))}
                placeholder="Enter venue (optional)"
              />
            </div>
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