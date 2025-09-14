import { useState, useEffect, useCallback } from 'react';
import type { 
  CreateFloorballMatchRequest,
  FloorballMatchDto,
} from '../../../../../../types/floorball/floorballTypes';
import SearchableInfiniteDropdown from '../../../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import { floorballSeasonSearchService } from '../../../../../../api/floorball/floorballTeamSearchService';
import { floorballTeamNameSearchService } from '../../../../../../api/floorball/floorballTeamNameSearchService';
import { floorballRefereeSearchService } from '../../../../../../api/floorball/floorballRefereeSearchService';
import './MatchForm.scss';

type MatchFormMode = 'create' | 'edit';

interface MatchFormProps {
  mode: MatchFormMode;
  initialData?: FloorballMatchDto;
  onSubmit: (matchData: CreateFloorballMatchRequest) => Promise<void>;
  onCancel: () => void;
  onCancelMatch?: (matchId: string) => Promise<void>;
  loading?: boolean;
}

const MatchForm = ({
  mode,
  initialData,
  onSubmit,
  onCancel,
  onCancelMatch,
  loading = false
}: MatchFormProps) => {
  const [formData, setFormData] = useState<CreateFloorballMatchRequest>({
    seasonId: '',
    homeTeamId: '',
    awayTeamId: '',
    refereeId: '',
    scheduledDateTime: '',
    venue: ''
  });
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [hoursInput, setHoursInput] = useState('');
  const [minutesInput, setMinutesInput] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [cancelLoading, setCancelLoading] = useState(false);
  
  // State for pre-loaded dropdown options
  const [initialSeasonOptions, setInitialSeasonOptions] = useState<Array<{id: string, name: string}>>([]);
  const [initialHomeTeamOptions, setInitialHomeTeamOptions] = useState<Array<{id: string, name: string}>>([]);
  const [initialAwayTeamOptions, setInitialAwayTeamOptions] = useState<Array<{id: string, name: string}>>([]);

  // Create initial options from initialData for immediate display
  const createInitialOptions = useCallback(() => {
    if (mode === 'edit' && initialData) {
      // Create season option from initialData (we'll update with real name later)
      const seasonOption = {
        id: initialData.seasonId,
        name: `Season ${initialData.seasonId}` // Placeholder, will be updated by async function
      };
      setInitialSeasonOptions([seasonOption]);

      // Create team options from initialData (we have the real names)
      const homeTeamOption = {
        id: initialData.homeTeamId,
        name: initialData.homeTeamName
      };
      setInitialHomeTeamOptions([homeTeamOption]);

      const awayTeamOption = {
        id: initialData.awayTeamId,
        name: initialData.awayTeamName
      };
      setInitialAwayTeamOptions([awayTeamOption]);
    } else {
      // Clear initial options for create mode
      setInitialSeasonOptions([]);
      setInitialHomeTeamOptions([]);
      setInitialAwayTeamOptions([]);
    }
  }, [mode, initialData]);

  // Pre-load initial options for dropdowns when in edit mode (for better search results)
  const preloadInitialOptions = useCallback(async () => {
    if (mode === 'edit' && initialData) {
      try {
        // Pre-load season options with real names
        const seasonResult = await floorballSeasonSearchService.searchSeasons('', 1);
        const matchingSeason = seasonResult.data.find(season => season.id === initialData.seasonId);
        if (matchingSeason) {
          setInitialSeasonOptions([matchingSeason]);
        }

        // Pre-load home team options with real names
        const homeTeamResult = await floorballTeamNameSearchService.searchTeams('', 1);
        const matchingHomeTeam = homeTeamResult.data.find(team => team.id === initialData.homeTeamId);
        if (matchingHomeTeam) {
          setInitialHomeTeamOptions([matchingHomeTeam]);
        }

        // Pre-load away team options with real names
        const awayTeamResult = await floorballTeamNameSearchService.searchTeams('', 1);
        const matchingAwayTeam = awayTeamResult.data.find(team => team.id === initialData.awayTeamId);
        if (matchingAwayTeam) {
          setInitialAwayTeamOptions([matchingAwayTeam]);
        }
      } catch (error) {
        console.error('Error pre-loading initial options:', error);
      }
    }
  }, [mode, initialData]);

  // Initialize form with initial data when in edit mode
  useEffect(() => {
    if (mode === 'edit' && initialData) {
      const matchDate = new Date(initialData.scheduledDateTime);
      const hoursStr = matchDate.getHours().toString().padStart(2, '0');
      const minutesStr = matchDate.getMinutes().toString().padStart(2, '0');

      setFormData({
        seasonId: initialData.seasonId,
        homeTeamId: initialData.homeTeamId,
        awayTeamId: initialData.awayTeamId,
        refereeId: '', // We don't have referee info in the DTO
        scheduledDateTime: initialData.scheduledDateTime,
        venue: initialData.venue || ''
      });
      setSelectedDate(matchDate);
      setHoursInput(hoursStr);
      setMinutesInput(minutesStr);
      
      // Create initial options immediately for display
      createInitialOptions();
      
      // Pre-load better options asynchronously
      preloadInitialOptions();
    } else {
      // Reset form for create mode
      setFormData({
        seasonId: '',
        homeTeamId: '',
        awayTeamId: '',
        refereeId: '',
        scheduledDateTime: '',
        venue: ''
      });
      setSelectedDate(null);
      setHoursInput('');
      setMinutesInput('');
      
      // Clear initial options
      createInitialOptions();
    }
  }, [mode, initialData, createInitialOptions, preloadInitialOptions]);

  // Custom search functions that include initial options
  const searchSeasonsWithInitial = useCallback(async (query: string, page: number) => {
    // Get all seasons from the search service
    const result = await floorballSeasonSearchService.searchSeasons(query, page);
    
    // If we're in edit mode and this is the first page, try to get the real season name
    if (mode === 'edit' && page === 1 && initialData) {
      try {
        // Find the matching season in the search results to get the real name
        const matchingSeason = result.data.find(season => season.id === initialData.seasonId);
        
        if (matchingSeason) {
          // Update the initial season option with the real name
          const updatedInitialOptions = [{
            id: initialData.seasonId,
            name: matchingSeason.name
          }];
          
          const filteredInitial = updatedInitialOptions.filter(option => 
            option.name.toLowerCase().includes(query.toLowerCase())
          );
          
          return {
            data: [...filteredInitial, ...result.data.filter(item => 
              !updatedInitialOptions.some(initial => initial.id === item.id)
            )],
            pagination: result.pagination
          };
        }
      } catch (error) {
        console.error('Error fetching season name:', error);
      }
    }
    
    // If we're in edit mode and this is the first page, include initial options
    if (mode === 'edit' && page === 1 && initialSeasonOptions.length > 0) {
      const filteredInitial = initialSeasonOptions.filter(option => 
        option.name.toLowerCase().includes(query.toLowerCase())
      );
      return {
        data: [...filteredInitial, ...result.data.filter(item => 
          !initialSeasonOptions.some(initial => initial.id === item.id)
        )],
        pagination: result.pagination
      };
    }
    
    return result;
  }, [mode, initialData, initialSeasonOptions]);

  const searchHomeTeamsWithInitial = useCallback(async (query: string, page: number) => {
    // Get all teams from the search service
    const result = await floorballTeamNameSearchService.searchTeams(query, page);
    
    // If we're in edit mode and this is the first page, include initial options
    if (mode === 'edit' && page === 1 && initialHomeTeamOptions.length > 0) {
      const filteredInitial = initialHomeTeamOptions.filter(option => 
        option.name.toLowerCase().includes(query.toLowerCase())
      );
      return {
        data: [...filteredInitial, ...result.data.filter(item => 
          !initialHomeTeamOptions.some(initial => initial.id === item.id)
        )],
        pagination: result.pagination
      };
    }
    
    return result;
  }, [mode, initialHomeTeamOptions]);

  const searchAwayTeamsWithInitial = useCallback(async (query: string, page: number) => {
    // Get all teams from the search service
    const result = await floorballTeamNameSearchService.searchTeams(query, page);
    
    // If we're in edit mode and this is the first page, include initial options
    if (mode === 'edit' && page === 1 && initialAwayTeamOptions.length > 0) {
      const filteredInitial = initialAwayTeamOptions.filter(option => 
        option.name.toLowerCase().includes(query.toLowerCase())
      );
      return {
        data: [...filteredInitial, ...result.data.filter(item => 
          !initialAwayTeamOptions.some(initial => initial.id === item.id)
        )],
        pagination: result.pagination
      };
    }
    
    return result;
  }, [mode, initialAwayTeamOptions]);

  // Update scheduledDateTime when date or time changes
  const updateScheduledDateTime = (date: Date | null, hours: string, minutes: string) => {
    if (date && hours && minutes) {
      // Create a new date object with the selected date and time
      const localDateTime = new Date(date);
      localDateTime.setHours(parseInt(hours), parseInt(minutes), 0, 0);
      
      // Convert to ISO string with timezone offset to ensure proper timezone handling
      const isoDateTime = localDateTime.toISOString();
      setFormData(prev => ({ ...prev, scheduledDateTime: isoDateTime }));
    } else {
      setFormData(prev => ({ ...prev, scheduledDateTime: '' }));
    }
  };

  const handleDateChange = (date: Date | null) => {
    setSelectedDate(date);
    updateScheduledDateTime(date, hoursInput, minutesInput);
  };

  const handleHoursChange = (value: string) => {
    // Validate hours (0-23)
    const numValue = parseInt(value);
    if (value === '' || (numValue >= 0 && numValue <= 23 && value.length <= 2)) {
      setHoursInput(value);
      updateScheduledDateTime(selectedDate, value, minutesInput);
    }
  };

  const handleMinutesChange = (value: string) => {
    // Validate minutes (0-59)
    const numValue = parseInt(value);
    if (value === '' || (numValue >= 0 && numValue <= 59 && value.length <= 2)) {
      setMinutesInput(value);
      updateScheduledDateTime(selectedDate, hoursInput, value);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      setError(null);

      if (mode === 'create') {
        if (!formData.seasonId || !formData.homeTeamId || !formData.awayTeamId || !formData.scheduledDateTime) {
          setError('Please fill in all required fields');
          return;
        }
  
        if (formData.homeTeamId === formData.awayTeamId) {
          setError('Home team and away team cannot be the same');
          return;
        }
  
        // Validate time inputs
        if (!selectedDate || !hoursInput || !minutesInput) {
          setError('Please enter a valid date and time');
          return;
        }
      }

      await onSubmit(formData);
      
      if (mode === 'create') {
        // Reset form on success
        setFormData({
          seasonId: '',
          homeTeamId: '',
          awayTeamId: '',
          refereeId: '',
          scheduledDateTime: '',
          venue: ''
        });
        setSelectedDate(null);
        setHoursInput('');
        setMinutesInput('');
        
        // Clear initial options
        createInitialOptions();
      }
    } catch (error) {
      console.error(`Error ${mode === 'create' ? 'creating' : 'updating'} match:`, error);
      setError(error instanceof Error ? error.message : `Failed to ${mode} match`);
    }
  };

  const handleCancel = () => {
    setFormData({
      seasonId: '',
      homeTeamId: '',
      awayTeamId: '',
      refereeId: '',
      scheduledDateTime: '',
      venue: ''
    });
    setSelectedDate(null);
    setHoursInput('');
    setMinutesInput('');
    setError(null);
    
    // Clear initial options
    createInitialOptions();
    
    onCancel();
  };

  const handleCancelMatch = async () => {
    if (!initialData || !onCancelMatch) return;
    
    try {
      setCancelLoading(true);
      setError(null);
      
      await onCancelMatch(initialData.id);
      
      // Close the modal after successful cancellation
      handleCancel();
    } catch (error) {
      console.error('Error canceling match:', error);
      setError(error instanceof Error ? error.message : 'Failed to cancel match');
    } finally {
      setCancelLoading(false);
    }
  };

  return (
    <>
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
              value={formData.seasonId}
              onChange={(value) => setFormData(prev => ({ ...prev, seasonId: value }))}
              onSearch={searchSeasonsWithInitial}
              searchPlaceholder="Search seasons..."
              emptyMessage="No seasons found"
              required
              loadInitialDataOnMount={mode === 'edit'}
            />
          </div>
        </div>
        <div className="form-group create-match-form-row">
          <label htmlFor="homeTeam">Home Team *</label>
          <div className="input-wrapper">
            <SearchableInfiniteDropdown
              placeholder="Select Home Team"
              value={formData.homeTeamId}
              onChange={(value) => setFormData(prev => ({ ...prev, homeTeamId: value }))}
              onSearch={searchHomeTeamsWithInitial}
              searchPlaceholder="Search teams..."
              emptyMessage="No teams found"
              required
              loadInitialDataOnMount={mode === 'edit'}
            />
          </div>
        </div>
        <div className="form-group create-match-form-row">
          <label htmlFor="awayTeam">Away Team *</label>
          <div className="input-wrapper">
            <SearchableInfiniteDropdown
              placeholder="Select Away Team"
              value={formData.awayTeamId}
              onChange={(value) => setFormData(prev => ({ ...prev, awayTeamId: value }))}
              onSearch={searchAwayTeamsWithInitial}
              searchPlaceholder="Search teams..."
              emptyMessage="No teams found"
              required
              loadInitialDataOnMount={mode === 'edit'}
            />
          </div>
        </div>
        {mode === 'create' && (
          <div className="form-group create-match-form-row">
            <label htmlFor="referee">Referee</label>
            <div className="input-wrapper">
              <SearchableInfiniteDropdown
                placeholder="Select Referee"
                value={formData.refereeId}
                onChange={(value) => setFormData(prev => ({ ...prev, refereeId: value }))}
                onSearch={floorballRefereeSearchService.searchReferees}
                searchPlaceholder="Search referees..."
                emptyMessage="No referees found"
              />
            </div>
          </div>
        )}
        <div className="form-group create-match-form-row">
          <label>Date & Time *</label>
          <div className="input-wrapper">
            <div className="datetime-input-group">
              <div className="date-input">
                <input
                  type="date"
                  value={selectedDate ? selectedDate.toISOString().split('T')[0] : ''}
                  onChange={(e) => handleDateChange(e.target.value ? new Date(e.target.value) : null)}
                  className="date-picker-input"
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
              value={formData.venue}
              onChange={(e) => setFormData(prev => ({ ...prev, venue: e.target.value }))}
              placeholder="Enter venue"
            />
          </div>
        </div>
        
        <div className="form-actions">
          <button type="button" onClick={handleCancel} className="cancel-button">
            Cancel
          </button>
          {mode === 'edit' && initialData && onCancelMatch && (
            <button 
              type="button" 
              onClick={handleCancelMatch} 
              disabled={cancelLoading || initialData.status === 'Cancelled' || initialData.status === 'Completed'}
              className="cancel-match-button"
            >
              {cancelLoading ? 'Cancelling...' : 'Cancel Match'}
            </button>
          )}
          <button type="submit" disabled={loading} className="submit-button">
            {loading ? (mode === 'create' ? 'Creating...' : 'Updating...') : (mode === 'create' ? 'Create Match' : 'Update Match')}
          </button>
        </div>
      </form>
    </>
  );
};

export default MatchForm;
