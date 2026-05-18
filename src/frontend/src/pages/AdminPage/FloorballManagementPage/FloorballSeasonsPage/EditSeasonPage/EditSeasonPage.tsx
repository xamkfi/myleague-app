import { useState, useEffect, useCallback, useMemo } from 'react';
import type { ChangeEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import Pagination from '../../../../../components/Pagination';
import type { 
  FloorballSeasonDto, 
  UpdateFloorballSeasonRequest
} from '../../../../../api/floorball/floorballSeasonService';
import { floorballSeasonService } from '../../../../../api/floorball/floorballSeasonService';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import { type FloorballTeam, TeamCategory } from '../../../../../types/floorball/floorballTypes';
import { useDivisions } from '../../../../../hooks/useDivisions';
import './EditSeasonPage.scss';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';

const EditSeasonPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { competitionId } = useParams<{ competitionId: string }>();
  const { divisions } = useDivisions();
  
  const [season, setSeason] = useState<FloorballSeasonDto | null>(null);
  const [loadingSeason, setLoadingSeason] = useState(true);
  const [formData, setFormData] = useState<UpdateFloorballSeasonRequest>({
    name: '',
    startDate: '',
    endDate: '',
    numberOfPeriods: 2,
    periodDurationMinutes: 15,
    allowOvertime: true,
    overtimeDurationMinutes: 5,
    allowShootout: true
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'details' | 'divisions' | 'teams'>('details');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);
  
  // Division management state
  const [addingDivision, setAddingDivision] = useState(false);
  const [removingDivision, setRemovingDivision] = useState<string | null>(null);
  
  // ── Team management state (redesigned from scratch) ──
  const [selectedDivisionId, setSelectedDivisionId] = useState<string | null>(null);
  
  // Available teams (paginated from API)
  const [availableTeams, setAvailableTeams] = useState<FloorballTeam[]>([]);
  const [loadingTeams, setLoadingTeams] = useState(false);
  const [teamsPagination, setTeamsPagination] = useState({
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  });
  
  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [teamCategory, setTeamCategory] = useState<TeamCategory | ''>('');
  
  // Multi-select
  const [selectedTeamIds, setSelectedTeamIds] = useState<Set<string>>(new Set());
  const [teamOperationLoading, setTeamOperationLoading] = useState(false);

  // ── Load season ──
  const loadSeason = useCallback(async () => {
    if (!competitionId) return;
    try {
      setLoadingSeason(true);
      const seasonData = await floorballSeasonService.getById(competitionId);
      setSeason(seasonData.data);
      setFormData({
        name: seasonData.data.name,
        startDate: seasonData.data.startDate.split('T')[0],
        endDate: seasonData.data.endDate.split('T')[0],
        numberOfPeriods: seasonData.data.matchRules?.numberOfPeriods ?? 2,
        periodDurationMinutes: seasonData.data.matchRules?.periodDurationMinutes ?? 15,
        allowOvertime: seasonData.data.matchRules?.allowOvertime ?? true,
        overtimeDurationMinutes: seasonData.data.matchRules?.overtimeDurationMinutes ?? 5,
        allowShootout: seasonData.data.matchRules?.allowShootout ?? true
      });
    } catch {
      setError(t('floorball.seasons.errors.loadFailed', 'Failed to load season data'));
    } finally {
      setLoadingSeason(false);
    }
  }, [competitionId, t]);

  useEffect(() => {
    if (competitionId) {
      loadSeason();
    }
  }, [competitionId, loadSeason]);

  // Auto-select first division when season loads
  useEffect(() => {
    if (season?.seasonDivisions && season.seasonDivisions.length > 0 && !selectedDivisionId) {
      setSelectedDivisionId(season.seasonDivisions[0].divisionId);
    }
  }, [season, selectedDivisionId]);

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Reset to page 1 when search/filter changes
  useEffect(() => {
    setTeamsPagination(prev => ({ ...prev, currentPage: 1 }));
  }, [debouncedSearchTerm, teamCategory]);

  // Load available teams (paginated) when teams tab is active
  const loadAvailableTeams = useCallback(async () => {
    if (activeTab !== 'teams' || !season) return;
    try {
      setLoadingTeams(true);
      const response = await floorballTeamService.getAllWithoutRoster({
        page: teamsPagination.currentPage,
        pageSize: teamsPagination.pageSize,
        searchTerm: debouncedSearchTerm || undefined,
        teamCategory: teamCategory || undefined
      });
      if (response?.data && Array.isArray(response.data)) {
        setAvailableTeams(response.data);
        if (response.pagination) {
          setTeamsPagination(prev => ({
            ...prev,
            totalCount: response.pagination.totalCount,
            totalPages: response.pagination.totalPages
          }));
        }
      } else {
        setAvailableTeams([]);
      }
    } catch {
      setAvailableTeams([]);
    } finally {
      setLoadingTeams(false);
    }
  }, [activeTab, season, teamsPagination.currentPage, teamsPagination.pageSize, debouncedSearchTerm, teamCategory]);

  useEffect(() => {
    loadAvailableTeams();
  }, [loadAvailableTeams]);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  // ── Helpers ──
  const showSuccess = useCallback((message: string, autoNavigate = false) => {
    if (successTimeoutId) clearTimeout(successTimeoutId);
    setSuccessMessage(message);
    const timeoutId = setTimeout(() => {
      setSuccessMessage(null);
      setSuccessTimeoutId(null);
      if (autoNavigate) navigate('/admin/floorball/seasons');
    }, autoNavigate ? 3000 : 2000);
    setSuccessTimeoutId(timeoutId);
  }, [navigate, successTimeoutId]);

  const getDivisionName = useCallback((divisionId: string): string => {
    return divisions.find(d => d.id === divisionId)?.name ?? divisionId;
  }, [divisions]);

  const handleInputChange = (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value
    }));
  };

  const parseApiError = (err: unknown): string => {
    const msg = err instanceof Error ? err.message : String(err);
    if (msg?.includes('Failed to fetch') || msg?.includes('NetworkError'))
      return t('floorball.seasons.errors.networkError', 'Network error. Please check your connection.');
    if (msg?.includes('HTTP 400'))
      return t('floorball.seasons.errors.validationError', 'Invalid data. Please check your input.');
    if (msg?.includes('HTTP 404'))
      return t('floorball.seasons.errors.notFound', 'Not found. It may have been deleted.');
    if (msg?.includes('HTTP 409'))
      return t('floorball.seasons.errors.conflictError', 'A season with overlapping dates already exists.');
    if (msg?.includes('HTTP 500'))
      return t('floorball.seasons.errors.serverError', 'Server error. Please try again later.');
    return msg || t('floorball.seasons.errors.updateFailed', 'Operation failed. Please try again.');
  };

  // ── Season Details submit ──
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!competitionId) return;
    setLoading(true);
    setError(null);
    setSuccessMessage(null);
    try {
      if (!formData.name.trim()) throw new Error(t('floorball.seasons.validation.nameRequired', 'Season name is required'));
      if (formData.name.trim().length > 100) throw new Error(t('floorball.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters'));
      if (!formData.startDate) throw new Error(t('floorball.seasons.validation.startDateRequired', 'Start date is required'));
      if (!formData.endDate) throw new Error(t('floorball.seasons.validation.endDateRequired', 'End date is required'));
      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);
      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) throw new Error(t('floorball.seasons.validation.invalidDate', 'Please enter valid dates'));
      if (endDate <= startDate) throw new Error(t('floorball.seasons.validation.endDateAfterStart', 'End date must be after start date'));
      const maxDuration = 2 * 365 * 24 * 60 * 60 * 1000;
      if (endDate.getTime() - startDate.getTime() > maxDuration) throw new Error(t('floorball.seasons.validation.seasonTooLong', 'Season duration cannot exceed 2 years'));

      await floorballSeasonService.update(competitionId, formData);
      showSuccess(t('floorball.seasons.seasonUpdated', 'Season "{{seasonName}}" has been updated successfully!', { seasonName: formData.name }), true);
      await loadSeason();
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  // ── Division Management ──
  const handleAddDivision = async (divisionId: string) => {
    if (!competitionId) return;
    setAddingDivision(true);
    setError(null);
    try {
      await floorballSeasonService.addDivisionToSeason(competitionId, divisionId);
      await loadSeason();
      showSuccess(t('floorball.seasons.divisionAdded', 'Division added successfully!'));
    } catch (err) { setError(parseApiError(err)); }
    finally { setAddingDivision(false); }
  };

  const handleRemoveDivision = async (divisionId: string) => {
    if (!competitionId) return;
    setRemovingDivision(divisionId);
    setError(null);
    try {
      await floorballSeasonService.removeDivisionFromSeason(competitionId, divisionId);
      if (selectedDivisionId === divisionId) setSelectedDivisionId(null);
      await loadSeason();
      showSuccess(t('floorball.seasons.divisionRemoved', 'Division removed successfully!'));
    } catch (err) { setError(parseApiError(err)); }
    finally { setRemovingDivision(null); }
  };

  // ── Team Management (immediate save, multi-select) ──
  const toggleTeamSelection = (teamId: string) => {
    setSelectedTeamIds(prev => {
      const next = new Set(prev);
      if (next.has(teamId)) next.delete(teamId);
      else next.add(teamId);
      return next;
    });
  };

  const selectAllAvailable = () => {
    const notInSeason = availableTeamsNotInSeason;
    if (selectedTeamIds.size === notInSeason.length) {
      setSelectedTeamIds(new Set());
    } else {
      setSelectedTeamIds(new Set(notInSeason.map(t => t.id)));
    }
  };

  const handleAddSelectedTeams = async () => {
    if (!competitionId || !selectedDivisionId || selectedTeamIds.size === 0) return;
    setTeamOperationLoading(true);
    setError(null);
    let successCount = 0;
    let failCount = 0;
    for (const teamId of selectedTeamIds) {
      try {
        await floorballSeasonService.addTeamToSeasonDivision(competitionId, selectedDivisionId, teamId);
        successCount++;
      } catch {
        failCount++;
      }
    }
    setSelectedTeamIds(new Set());
    await loadSeason();
    await loadAvailableTeams();
    if (failCount === 0) {
      showSuccess(t('floorball.seasons.teamsAdded', '{{count}} team(s) added successfully!', { count: successCount }));
    } else {
      setError(t('floorball.seasons.someTeamsFailed', '{{success}} added, {{fail}} failed.', { success: successCount, fail: failCount }));
    }
    setTeamOperationLoading(false);
  };

  const handleAddSingleTeam = async (teamId: string) => {
    if (!competitionId || !selectedDivisionId) return;
    setTeamOperationLoading(true);
    setError(null);
    try {
      await floorballSeasonService.addTeamToSeasonDivision(competitionId, selectedDivisionId, teamId);
      await loadSeason();
      await loadAvailableTeams();
      showSuccess(t('floorball.seasons.teamAdded', 'Team added successfully!'));
    } catch (err) { setError(parseApiError(err)); }
    finally { setTeamOperationLoading(false); }
  };

  const handleRemoveTeam = async (teamId: string) => {
    if (!competitionId || !selectedDivisionId) return;
    setTeamOperationLoading(true);
    setError(null);
    try {
      await floorballSeasonService.removeTeamFromSeasonDivision(competitionId, selectedDivisionId, teamId);
      await loadSeason();
      showSuccess(t('floorball.seasons.teamRemoved', 'Team removed successfully!'));
    } catch (err) { setError(parseApiError(err)); }
    finally { setTeamOperationLoading(false); }
  };

  // ── Computed data ──
  
  // All team IDs across ALL season divisions (a team can only be in one)
  const allSeasonTeamIds = useMemo(() => {
    const ids = new Set<string>();
    season?.seasonDivisions?.forEach(sd => {
      sd.teamIds?.forEach(id => ids.add(id));
    });
    return ids;
  }, [season]);

  // Team IDs in the selected division (from the DTO)
  const selectedDivisionTeamIds = useMemo(() => {
    if (!selectedDivisionId || !season?.seasonDivisions) return new Set<string>();
    const sd = season.seasonDivisions.find(d => d.divisionId === selectedDivisionId);
    return new Set<string>(sd?.teamIds ?? []);
  }, [season, selectedDivisionId]);

  // Teams in selected division (from season.teams matched against division teamIds)
  const teamsInSelectedDivision = useMemo(() => {
    if (!season?.teams) return [];
    return season.teams.filter(t => selectedDivisionTeamIds.has(t.id));
  }, [season, selectedDivisionTeamIds]);

  // Available teams from API that are NOT in any season division
  const availableTeamsNotInSeason = useMemo(() => {
    return availableTeams.filter(t => !allSeasonTeamIds.has(t.id));
  }, [availableTeams, allSeasonTeamIds]);

  // Divisions not yet in season
  const availableDivisions = useMemo(() => {
    const seasonDivisionIds = season?.seasonDivisions?.map(sd => sd.divisionId) ?? [];
    return divisions.filter(div => !seasonDivisionIds.includes(div.id));
  }, [divisions, season]);

  const totalTeamCount = allSeasonTeamIds.size;

  // ── Render ──
  if (loadingSeason) {
    return (
      <PageTemplate title={t('floorball.seasons.edit.title', 'Edit Season')}>
        <div className="edit-season-loading"><p>{t('common.loading', 'Loading...')}</p></div>
      </PageTemplate>
    );
  }

  if (!season) {
    return (
      <PageTemplate title={t('floorball.seasons.edit.title', 'Edit Season')}>
        <ErrorPopup message={t('floorball.seasons.errors.notFound', 'Season not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.seasons.edit.title', 'Edit Season')}>
      {successMessage && (
        <div className="success-toast"><p>{successMessage}</p></div>
      )}

      <div className="edit-season-container">
        <div className="edit-season-back">
          <button
            type="button"
            className="back-button"
            onClick={() => navigate('/admin/floorball/seasons')}
          >
            <span aria-hidden="true">&larr;</span>{' '}
            {t('floorball.seasons.backToList', 'Back to Seasons')}
          </button>
        </div>
        {/* Tab Navigation */}
        <div className="tab-navigation">
          <button className={`tab-button ${activeTab === 'details' ? 'active' : ''}`} onClick={() => setActiveTab('details')}>
            {t('floorball.seasons.seasonDetails', 'Season Details')}
          </button>
          <button className={`tab-button ${activeTab === 'divisions' ? 'active' : ''}`} onClick={() => setActiveTab('divisions')}>
            {t('floorball.seasons.manageDivisions', 'Manage Divisions')} ({season?.seasonDivisions?.length || 0})
          </button>
          <button className={`tab-button ${activeTab === 'teams' ? 'active' : ''}`} onClick={() => setActiveTab('teams')}>
            {t('floorball.seasons.manageTeams', 'Manage Teams')} ({totalTeamCount})
          </button>
        </div>

        <div className="edit-season-content">
          {/* ─── Season Details Tab ─── */}
          {activeTab === 'details' && (
            <form onSubmit={handleSubmit} className="edit-season-form">
              <ErrorPopup message={error} />

              <div className="form-section">
                <h3 className="form-section__title">
                  <i className="fas fa-info-circle"></i>
                  {t('floorball.seasons.sections.basicInfo', 'Basic Information')}
                </h3>
                <div className="form-group">
                  <label htmlFor="edit-name">{t('floorball.seasons.fields.name', 'Name')} *</label>
                  <input type="text" id="edit-name" name="name" value={formData.name} onChange={handleInputChange} required disabled={loading} placeholder={t('floorball.seasons.placeholders.name', 'Enter season name')} />
                </div>
              </div>

              <div className="form-section">
                <h3 className="form-section__title">
                  <i className="fas fa-calendar-alt"></i>
                  {t('floorball.seasons.sections.schedule', 'Schedule')}
                </h3>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="edit-startDate">{t('floorball.seasons.fields.startDate', 'Start Date')} *</label>
                    <input type="date" id="edit-startDate" name="startDate" value={formData.startDate} onChange={handleInputChange} required disabled={loading} />
                  </div>
                  <div className="form-group">
                    <label htmlFor="edit-endDate">{t('floorball.seasons.fields.endDate', 'End Date')} *</label>
                    <input type="date" id="edit-endDate" name="endDate" value={formData.endDate} onChange={handleInputChange} required disabled={loading} min={formData.startDate} />
                  </div>
                </div>
              </div>

              <div className="form-section">
                <h3 className="form-section__title">
                  <i className="fas fa-gavel"></i>
                  {t('floorball.seasons.fields.matchRules', 'Match Rules')}
                </h3>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="edit-numberOfPeriods">{t('floorball.seasons.fields.numberOfPeriods', 'Number of Periods')}</label>
                    <select id="edit-numberOfPeriods" name="numberOfPeriods" value={formData.numberOfPeriods} onChange={handleInputChange} disabled={loading}>
                      {[1,2,3,4,5].map(n => <option key={n} value={n}>{n}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label htmlFor="edit-periodDurationMinutes">{t('floorball.seasons.fields.periodDurationMinutes', 'Period Duration (min)')}</label>
                    <input type="number" id="edit-periodDurationMinutes" name="periodDurationMinutes" value={formData.periodDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                  </div>
                </div>
                <div className="toggle-container">
                  <label className="toggle-label">{t('floorball.seasons.fields.allowOvertime', 'Allow Overtime')}</label>
                  <button type="button" className={`toggle-switch ${formData.allowOvertime ? 'active' : ''}`} onClick={() => setFormData(prev => ({ ...prev, allowOvertime: !prev.allowOvertime }))} disabled={loading} aria-pressed={formData.allowOvertime}>
                    <span className="toggle-switch__slider" />
                  </button>
                </div>
                {formData.allowOvertime && (
                  <div className="form-group form-group--indented">
                    <label htmlFor="edit-overtimeDurationMinutes">{t('floorball.seasons.fields.overtimeDurationMinutes', 'Overtime Duration (min)')}</label>
                    <input type="number" id="edit-overtimeDurationMinutes" name="overtimeDurationMinutes" value={formData.overtimeDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                  </div>
                )}
                <div className="toggle-container">
                  <label className="toggle-label">{t('floorball.seasons.fields.allowShootout', 'Allow Shootout')}</label>
                  <button type="button" className={`toggle-switch ${formData.allowShootout ? 'active' : ''}`} onClick={() => setFormData(prev => ({ ...prev, allowShootout: !prev.allowShootout }))} disabled={loading} aria-pressed={formData.allowShootout}>
                    <span className="toggle-switch__slider" />
                  </button>
                </div>
              </div>

              <div className="form-actions">
                <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/floorball/seasons')} disabled={loading}>{t('common.cancel', 'Cancel')}</button>
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? (<><i className="fas fa-spinner fa-spin"></i>{t('common.saving', 'Saving...')}</>) : t('common.save', 'Save')}
                </button>
              </div>
            </form>
          )}

          {/* ─── Divisions Tab ─── */}
          {activeTab === 'divisions' && (
            <div className="divisions-management">
              <ErrorPopup message={error} />
              <div className="current-divisions-section">
                <h4>{t('floorball.seasons.currentDivisions', 'Current Divisions')} ({season?.seasonDivisions?.length || 0})</h4>
                {season?.seasonDivisions && season.seasonDivisions.length === 0 ? (
                  <p className="no-divisions">{t('floorball.seasons.noDivisions', 'No divisions in this season')}</p>
                ) : (
                  <div className="divisions-list">
                    {season?.seasonDivisions?.map(sd => (
                      <div key={sd.divisionId} className="division-item">
                        <div className="division-info">
                          <span className="division-name">{getDivisionName(sd.divisionId)}</span>
                          <span className="division-team-count">{t('floorball.seasons.teamCount', '{{count}} team(s)', { count: sd.teamCount })}</span>
                        </div>
                        <button type="button" className="btn btn-danger btn-sm" onClick={() => handleRemoveDivision(sd.divisionId)} disabled={removingDivision === sd.divisionId || addingDivision}>
                          {removingDivision === sd.divisionId ? (<><i className="fas fa-spinner fa-spin"></i>{t('common.removing', 'Removing...')}</>) : (<><i className="fas fa-trash-alt"></i>{t('common.remove', 'Remove')}</>)}
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
              <div className="available-divisions-section">
                <h4>{t('floorball.seasons.availableDivisions', 'Available Divisions')} ({availableDivisions.length})</h4>
                {availableDivisions.length === 0 ? (
                  <p className="no-divisions">{t('floorball.seasons.allDivisionsAdded', 'All divisions have been added to this season')}</p>
                ) : (
                  <div className="divisions-list">
                    {availableDivisions.map(div => (
                      <div key={div.id} className="division-item">
                        <div className="division-info"><span className="division-name">{div.name}</span></div>
                        <button type="button" className="btn btn-primary btn-sm" onClick={() => handleAddDivision(div.id)} disabled={addingDivision}>
                          {addingDivision ? (<><i className="fas fa-spinner fa-spin"></i>{t('common.adding', 'Adding...')}</>) : (<><i className="fas fa-plus"></i>{t('common.add', 'Add')}</>)}
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}

          {/* ─── Teams Tab (rebuilt from scratch) ─── */}
          {activeTab === 'teams' && (
            <div className="teams-management">
              <ErrorPopup message={error} />

              {(!season?.seasonDivisions || season.seasonDivisions.length === 0) ? (
                <div className="tm-empty-state">
                  <i className="fas fa-layer-group"></i>
                  <h4>{t('floorball.seasons.addDivisionsFirst', 'Add divisions first')}</h4>
                  <p>{t('floorball.seasons.addDivisionsFirstDesc', 'You need at least one division in this season before you can manage teams.')}</p>
                  <button type="button" className="btn btn-primary" onClick={() => setActiveTab('divisions')}>
                    <i className="fas fa-plus"></i> {t('floorball.seasons.goToDivisions', 'Go to Manage Divisions')}
                  </button>
                </div>
              ) : (
                <>
                  {/* Division Selector */}
                  <div className="tm-division-selector">
                    <span className="tm-division-selector__label">
                      {t('floorball.seasons.addingTeamsTo', 'Managing teams for:')}
                    </span>
                    <div className="tm-division-selector__options">
                      {season.seasonDivisions.map(sd => {
                        const count = sd.teamIds?.length ?? 0;
                        const isActive = selectedDivisionId === sd.divisionId;
                        return (
                          <button
                            key={sd.divisionId}
                            type="button"
                            className={`tm-division-pill ${isActive ? 'active' : ''}`}
                            onClick={() => { setSelectedDivisionId(sd.divisionId); setSelectedTeamIds(new Set()); }}
                          >
                            {getDivisionName(sd.divisionId)}
                            <span className="tm-division-pill__badge">{count}</span>
                          </button>
                        );
                      })}
                    </div>
                  </div>

                  {selectedDivisionId && (
                    <>
                      {/* ── Current Teams in Division (grid) ── */}
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-users"></i>
                            {t('floorball.seasons.teamsInDivision', 'Teams in {{division}}', { division: getDivisionName(selectedDivisionId) })}
                          </h4>
                          <span className="tm-section__count">{teamsInSelectedDivision.length} {t('floorball.seasons.teams', 'teams')}</span>
                        </div>

                        {teamsInSelectedDivision.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>{t('floorball.seasons.noTeamsInDivision', 'No teams in this division yet. Use the table below to add teams.')}</p>
                          </div>
                        ) : (
                          <div className="tm-team-grid">
                            {teamsInSelectedDivision.map(team => (
                              <div key={team.id} className="tm-team-chip">
                                <div className="tm-team-chip__info">
                                  <span className="tm-team-chip__name">{team.name}</span>
                                  <span className="tm-team-chip__club">{team.club?.name}</span>
                                </div>
                                <button
                                  type="button"
                                  className="tm-team-chip__remove"
                                  onClick={() => handleRemoveTeam(team.id)}
                                  disabled={teamOperationLoading}
                                  title={t('common.remove', 'Remove')}
                                >
                                  <i className="fas fa-times"></i>
                                </button>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>

                      {/* ── Available Teams (paginated table) ── */}
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-plus-circle"></i>
                            {t('floorball.seasons.addTeams', 'Add Teams')}
                          </h4>
                        </div>

                        {/* Filters row */}
                        <div className="tm-filters">
                          <div className="tm-filters__search">
                            <i className="fas fa-search"></i>
                            <input
                              type="text"
                              placeholder={t('floorball.seasons.searchTeams', 'Search teams by name...')}
                              value={searchTerm}
                              onChange={(e) => setSearchTerm(e.target.value)}
                            />
                            {searchTerm && (
                              <button type="button" className="tm-filters__clear" onClick={() => setSearchTerm('')}>
                                <i className="fas fa-times"></i>
                              </button>
                            )}
                          </div>
                          <select
                            value={teamCategory}
                            onChange={(e) => setTeamCategory(e.target.value as TeamCategory | '')}
                            className="tm-filters__category"
                          >
                            <option value="">{t('floorball.seasons.allCategories', 'All Categories')}</option>
                            <option value={TeamCategory.Adult}>{t('floorball.teams.category.adult', 'Adult')}</option>
                            <option value={TeamCategory.Youth}>{t('floorball.teams.category.youth', 'Youth')}</option>
                            <option value={TeamCategory.Women}>{t('floorball.teams.category.women', 'Women')}</option>
                          </select>
                        </div>

                        {/* Multi-select action bar */}
                        {selectedTeamIds.size > 0 && (
                          <div className="tm-action-bar">
                            <span>{t('floorball.seasons.selectedCount', '{{count}} team(s) selected', { count: selectedTeamIds.size })}</span>
                            <button
                              type="button"
                              className="btn btn-primary btn-sm"
                              onClick={handleAddSelectedTeams}
                              disabled={teamOperationLoading}
                            >
                              {teamOperationLoading ? (
                                <><i className="fas fa-spinner fa-spin"></i> {t('common.adding', 'Adding...')}</>
                              ) : (
                                <><i className="fas fa-plus"></i> {t('floorball.seasons.addSelectedToDivision', 'Add to {{division}}', { division: getDivisionName(selectedDivisionId) })}</>
                              )}
                            </button>
                            <button
                              type="button"
                              className="tm-action-bar__clear"
                              onClick={() => setSelectedTeamIds(new Set())}
                            >
                              {t('common.clearSelection', 'Clear')}
                            </button>
                          </div>
                        )}

                        {/* Teams table */}
                        {loadingTeams ? (
                          <div className="tm-loading">
                            <i className="fas fa-spinner fa-spin"></i>
                            <p>{t('common.loading', 'Loading...')}</p>
                          </div>
                        ) : availableTeamsNotInSeason.length === 0 && availableTeams.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>
                              {searchTerm || teamCategory
                                ? t('floorball.seasons.noMatchingTeams', 'No teams match your search criteria.')
                                : t('floorball.seasons.noAvailableTeams', 'No teams available.')
                              }
                            </p>
                          </div>
                        ) : (
                          <>
                            <div className="tm-table-wrapper">
                              <table className="tm-table">
                                <thead>
                                  <tr>
                                    <th className="tm-table__checkbox">
                                      <input
                                        type="checkbox"
                                        checked={availableTeamsNotInSeason.length > 0 && selectedTeamIds.size === availableTeamsNotInSeason.length}
                                        onChange={selectAllAvailable}
                                        disabled={availableTeamsNotInSeason.length === 0}
                                        title={t('common.selectAll', 'Select all')}
                                      />
                                    </th>
                                    <th>{t('floorball.teams.name', 'Team')}</th>
                                    <th>{t('floorball.teams.club', 'Club')}</th>
                                    <th>{t('floorball.teams.category.label', 'Category')}</th>
                                    <th className="tm-table__status">{t('common.status', 'Status')}</th>
                                    <th className="tm-table__actions"></th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {availableTeams.map(team => {
                                    const isInSeason = allSeasonTeamIds.has(team.id);
                                    const isSelected = selectedTeamIds.has(team.id);
                                    return (
                                      <tr key={team.id} className={`${isSelected ? 'selected' : ''} ${isInSeason ? 'in-season' : ''}`}>
                                        <td className="tm-table__checkbox">
                                          <input
                                            type="checkbox"
                                            checked={isSelected}
                                            onChange={() => toggleTeamSelection(team.id)}
                                            disabled={isInSeason || teamOperationLoading}
                                          />
                                        </td>
                                        <td>
                                          <span className="tm-table__team-name">{team.name}</span>
                                          {team.shortName && <span className="tm-table__short-name">({team.shortName})</span>}
                                        </td>
                                        <td className="tm-table__club">{team.club?.name ?? '-'}</td>
                                        <td>
                                          {team.teamCategory && (
                                            <span className="tm-category-badge">{team.teamCategory}</span>
                                          )}
                                        </td>
                                        <td className="tm-table__status">
                                          {isInSeason ? (
                                            <span className="tm-status-badge tm-status-badge--added">
                                              <i className="fas fa-check"></i> {t('floorball.seasons.inSeason', 'In season')}
                                            </span>
                                          ) : null}
                                        </td>
                                        <td className="tm-table__actions">
                                          {!isInSeason && (
                                            <button
                                              type="button"
                                              className="btn btn-primary btn-sm"
                                              onClick={() => handleAddSingleTeam(team.id)}
                                              disabled={teamOperationLoading}
                                            >
                                              <i className="fas fa-plus"></i> {t('common.add', 'Add')}
                                            </button>
                                          )}
                                        </td>
                                      </tr>
                                    );
                                  })}
                                </tbody>
                              </table>
                            </div>

                            {/* Pagination */}
                            <Pagination
                              currentPage={teamsPagination.currentPage}
                              totalPages={teamsPagination.totalPages}
                              totalCount={teamsPagination.totalCount}
                              pageSize={teamsPagination.pageSize}
                              onPageChange={(page) => setTeamsPagination(prev => ({ ...prev, currentPage: page }))}
                              onPageSizeChange={(size) => setTeamsPagination(prev => ({ ...prev, pageSize: size, currentPage: 1 }))}
                              pageSizeOptions={[10, 25, 50]}
                              className="compact"
                            />
                          </>
                        )}
                      </div>
                    </>
                  )}
                </>
              )}
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
};

export default EditSeasonPage;
