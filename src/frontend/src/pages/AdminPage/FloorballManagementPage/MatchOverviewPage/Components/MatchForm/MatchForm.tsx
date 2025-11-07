import { useState, useEffect, useCallback, useRef } from 'react';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import type { 
  CreateFloorballMatchRequest,
  FloorballMatchDto,
} from '../../../../../../types/floorball/floorballTypes';
import SearchableInfiniteDropdown from '../../../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import { floorballSeasonSearchService } from '../../../../../../api/floorball/floorballTeamSearchService';
import { floorballTeamNameSearchService } from '../../../../../../api/floorball/floorballTeamNameSearchService';
import { floorballRefereeSearchService } from '../../../../../../api/floorball/floorballRefereeSearchService';
import './MatchForm.scss';
import ErrorPopup from '../../../../../../components/ErrorPopup/ErrorPopup';

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
  const [dateError, setDateError] = useState<string | null>(null);
  const lastKeyIsBackspaceRef = useRef(false);
  
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
    setDateError(null);
  };

  // TODO: If needed, extract date input logic into a reusable DateField utility to use elsewhere.

  // Parse DD/MM/YYYY safely
  const parseDdMmYyyy = (value: string): Date | null => {
    const parts = value.split('/');
    if (parts.length !== 3) return null;
    const [ddStr, mmStr, yyyyStr] = parts;
    const dd = parseInt(ddStr, 10);
    const mm = parseInt(mmStr, 10);
    const yyyy = parseInt(yyyyStr, 10);
    if (!Number.isFinite(dd) || !Number.isFinite(mm) || !Number.isFinite(yyyy)) return null;
    if (yyyyStr.length !== 4) return null;
    if (dd < 1 || mm < 1 || mm > 12) return null;
    const d = new Date(yyyy, mm - 1, dd);
    if (d.getFullYear() !== yyyy || d.getMonth() !== mm - 1 || d.getDate() !== dd) return null;
    return d;
  };

  const handleDateChangeRaw = (e: unknown) => {
    // react-datepicker may call onChangeRaw with keyboard/mouse events or undefined during calendar selection
    const inputEl = (e as React.ChangeEvent<HTMLInputElement>)?.target as HTMLInputElement | undefined;
    if (!inputEl || typeof inputEl.value !== 'string') {
      // Event did not originate from the text input (e.g., calendar click). Ignore.
      return;
    }
    let value = inputEl.value.replace(/[^0-9/]/g, '');

    // Auto-insert slashes exactly after DD and MM (only when typing forward)
    if (!lastKeyIsBackspaceRef.current) {
      if (value.length === 2 && value.indexOf('/') === -1) {
        value = value + '/';
      }
      if (value.length === 5 && value[2] === '/' && value.lastIndexOf('/') === 2) {
        value = value + '/';
      }
    }

    // Limit to DD/MM/YYYY length
    if (value.length > 10) {
      value = value.slice(0, 10);
    }

    // Reflect possibly reformatted value back to the input
    inputEl.value = value;

    if (value === '') {
      setSelectedDate(null);
      updateScheduledDateTime(null, hoursInput, minutesInput);
      setDateError(null);
      return;
    }
    // Allow user to type; validate when pattern matches fully
    if (/^\d{2}\/\d{2}\/\d{4}$/.test(value)) {
      const parsed = parseDdMmYyyy(value);
      if (parsed) {
        setSelectedDate(parsed);
        updateScheduledDateTime(parsed, hoursInput, minutesInput);
        setDateError(null);
      } else {
        setDateError('Invalid date. Use DD/MM/YYYY');
      }
    }
  };

  const handleDateKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    lastKeyIsBackspaceRef.current = e.key === 'Backspace';
    const input = e.currentTarget;
    const { selectionStart, selectionEnd, value } = input;
    if (e.key === 'Backspace' && selectionStart === selectionEnd && selectionStart && value[selectionStart - 1] === '/') {
      // Remove the slash and move cursor one position left
      e.preventDefault();
      const newVal = value.slice(0, selectionStart - 1) + value.slice(selectionStart);
      input.value = newVal;
      // Move caret
      requestAnimationFrame(() => {
        input.setSelectionRange((selectionStart as number) - 1, (selectionStart as number) - 1);
      });
    }
  };

  const handleDateBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    const value = e.target.value;
    if (value === '') {
      setDateError(null);
      return;
    }
    const parsed = parseDdMmYyyy(value);
    if (!parsed) {
      setDateError('Invalid date. Use DD/MM/YYYY');
    }
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
        <ErrorPopup message={error} />

        // <div className="error-alert">
        //   <span className="error-icon">⚠️</span>
        //   <span className="error-text">{error}</span>
        //   <button onClick={() => setError(null)} className="error-close">×</button>
        // </div>
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
                <DatePicker
                  selected={selectedDate}
                  onChange={(date) => handleDateChange(date)}
                  dateFormat="dd/MM/yyyy"
                  placeholderText="DD/MM/YYYY"
                  isClearable
                  onChangeRaw={(e) => handleDateChangeRaw(e as unknown as React.ChangeEvent<HTMLInputElement>)}
                  onBlur={handleDateBlur}
                  onKeyDown={(e) => handleDateKeyDown(e as React.KeyboardEvent<HTMLInputElement>)}
                  shouldCloseOnSelect
                  autoComplete="off"
                  className="date-picker-input"
                />
              </div>
              {dateError && (
                <div className="field-error" role="alert">{dateError}</div>
              )}
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
