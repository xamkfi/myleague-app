import { useState, useEffect, useCallback, useMemo } from 'react';
import type { ChangeEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import Pagination from '../../../../../components/Pagination';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import { floorballTournamentService } from '../../../../../api/floorball/floorballTournamentService';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import type {
  FloorballTournamentDto,
  CreateFloorballTournamentRequest,
} from '../../../../../types/floorball/tournamentTypes';
import { type FloorballTeam, TeamCategory } from '../../../../../types/floorball/floorballTypes';
import '../../FloorballSeasonsPage/EditSeasonPage/EditSeasonPage.scss';

type TournamentTab = 'details' | 'groups' | 'teams';

const EditTournamentPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { competitionId } = useParams<{ competitionId: string }>();

  const [tournament, setTournament] = useState<FloorballTournamentDto | null>(null);
  const [loadingTournament, setLoadingTournament] = useState(true);
  const [formData, setFormData] = useState<CreateFloorballTournamentRequest>({
    name: '',
    startDate: '',
    endDate: '',
    venue: '',
    contentHtml: '',
    groupStageNumberOfPeriods: 2,
    groupStagePeriodDurationMinutes: 15,
    groupStageAllowOvertime: false,
    groupStageOvertimeDurationMinutes: 5,
    groupStageAllowShootout: false,
    playoffNumberOfPeriods: 3,
    playoffPeriodDurationMinutes: 20,
    playoffAllowOvertime: true,
    playoffOvertimeDurationMinutes: 10,
    playoffAllowShootout: true,
    teamsAdvancingPerGroup: 2,
    hasPlayoffStage: true,
    hasThirdPlaceMatch: false,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TournamentTab>('details');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  // Group CRUD state
  const [newGroupName, setNewGroupName] = useState('');
  const [addingGroup, setAddingGroup] = useState(false);
  const [removingGroupId, setRemovingGroupId] = useState<string | null>(null);

  // Teams tab state — mirrors EditSeasonPage Teams tab layout
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [availableTeams, setAvailableTeams] = useState<FloorballTeam[]>([]);
  const [loadingTeams, setLoadingTeams] = useState(false);
  const [teamsPagination, setTeamsPagination] = useState({
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  });
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [teamCategory, setTeamCategory] = useState<TeamCategory | ''>('');
  const [selectedTeamIds, setSelectedTeamIds] = useState<Set<string>>(new Set());
  const [teamOperationLoading, setTeamOperationLoading] = useState(false);
  const [removingTeamKey, setRemovingTeamKey] = useState<string | null>(null);

  // Lifecycle actions
  const [lifecycleLoading, setLifecycleLoading] = useState(false);

  useEffect(() => {
    return () => {
      if (successTimeoutId) clearTimeout(successTimeoutId);
    };
  }, [successTimeoutId]);

  const showSuccess = useCallback(
    (message: string) => {
      if (successTimeoutId) clearTimeout(successTimeoutId);
      setSuccessMessage(message);
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
      }, 2500);
      setSuccessTimeoutId(timeoutId);
    },
    [successTimeoutId]
  );

  const loadTournament = useCallback(async () => {
    if (!competitionId) return;
    try {
      setLoadingTournament(true);
      const response = await floorballTournamentService.getById(competitionId);
      const data = response.data;
      setTournament(data);
      setFormData({
        name: data.name,
        startDate: data.startDate.split('T')[0],
        endDate: data.endDate.split('T')[0],
        venue: data.venue ?? '',
        contentHtml: data.contentHtml ?? '',
        groupStageNumberOfPeriods: data.tournamentRules?.groupStageMatchRules?.numberOfPeriods ?? 2,
        groupStagePeriodDurationMinutes: data.tournamentRules?.groupStageMatchRules?.periodDurationMinutes ?? 15,
        groupStageAllowOvertime: data.tournamentRules?.groupStageMatchRules?.allowOvertime ?? false,
        groupStageOvertimeDurationMinutes: data.tournamentRules?.groupStageMatchRules?.overtimeDurationMinutes ?? 5,
        groupStageAllowShootout: data.tournamentRules?.groupStageMatchRules?.allowShootout ?? false,
        playoffNumberOfPeriods: data.tournamentRules?.playoffMatchRules?.numberOfPeriods ?? 3,
        playoffPeriodDurationMinutes: data.tournamentRules?.playoffMatchRules?.periodDurationMinutes ?? 20,
        playoffAllowOvertime: data.tournamentRules?.playoffMatchRules?.allowOvertime ?? true,
        playoffOvertimeDurationMinutes: data.tournamentRules?.playoffMatchRules?.overtimeDurationMinutes ?? 10,
        playoffAllowShootout: data.tournamentRules?.playoffMatchRules?.allowShootout ?? true,
        teamsAdvancingPerGroup: data.tournamentRules?.teamsAdvancingPerGroup ?? 2,
        hasPlayoffStage: data.tournamentRules?.hasPlayoffStage ?? true,
        hasThirdPlaceMatch: data.tournamentRules?.hasThirdPlaceMatch ?? false,
      });
    } catch {
      setError(t('floorball.tournaments.errors.loadFailed', 'Failed to load tournament'));
    } finally {
      setLoadingTournament(false);
    }
  }, [competitionId, t]);

  useEffect(() => {
    loadTournament();
  }, [loadTournament]);

  // Auto-select first group when tournament loads or when teams tab opens
  useEffect(() => {
    if (tournament?.groups && tournament.groups.length > 0 && !selectedGroupId) {
      setSelectedGroupId(tournament.groups[0].id);
    }
  }, [tournament, selectedGroupId]);

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearchTerm(searchTerm), 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Reset to page 1 when search/filter changes
  useEffect(() => {
    setTeamsPagination((prev) => ({ ...prev, currentPage: 1 }));
  }, [debouncedSearchTerm, teamCategory]);

  // Load paginated teams when Teams tab active
  const loadAvailableTeams = useCallback(async () => {
    if (activeTab !== 'teams' || !tournament) return;
    try {
      setLoadingTeams(true);
      const response = await floorballTeamService.getAllWithoutRoster({
        page: teamsPagination.currentPage,
        pageSize: teamsPagination.pageSize,
        searchTerm: debouncedSearchTerm || undefined,
        teamCategory: teamCategory || undefined,
      });
      if (response?.data && Array.isArray(response.data)) {
        setAvailableTeams(response.data);
        if (response.pagination) {
          setTeamsPagination((prev) => ({
            ...prev,
            totalCount: response.pagination.totalCount,
            totalPages: response.pagination.totalPages,
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
  }, [activeTab, tournament, teamsPagination.currentPage, teamsPagination.pageSize, debouncedSearchTerm, teamCategory]);

  useEffect(() => {
    loadAvailableTeams();
  }, [loadAvailableTeams]);

  const parseApiError = (err: unknown): string => {
    const msg = err instanceof Error ? err.message : String(err);
    if (msg?.includes('Failed to fetch') || msg?.includes('NetworkError'))
      return t('floorball.tournaments.errors.networkError', 'Network error. Please check your connection.');
    if (msg?.includes('HTTP 400'))
      return t('floorball.tournaments.errors.validationError', 'Invalid data. Please check your input.');
    if (msg?.includes('HTTP 404'))
      return t('floorball.tournaments.errors.notFound', 'Not found. It may have been deleted.');
    if (msg?.includes('HTTP 500'))
      return t('floorball.tournaments.errors.serverError', 'Server error. Please try again later.');
    return msg || t('floorball.tournaments.errors.operationFailed', 'Operation failed. Please try again.');
  };

  const handleInputChange = (e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!competitionId) return;
    setLoading(true);
    setError(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('floorball.tournaments.validation.nameRequired', 'Tournament name is required'));
      }
      if (!formData.startDate) {
        throw new Error(t('floorball.tournaments.validation.startDateRequired', 'Start date is required'));
      }
      if (!formData.endDate) {
        throw new Error(t('floorball.tournaments.validation.endDateRequired', 'End date is required'));
      }

      await floorballTournamentService.update(competitionId, formData);
      showSuccess(t('floorball.tournaments.updated', 'Tournament updated successfully!'));
      await loadTournament();
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  // ── Group CRUD ──
  const handleAddGroup = async () => {
    if (!competitionId || !newGroupName.trim()) return;
    setAddingGroup(true);
    setError(null);
    try {
      await floorballTournamentService.addGroup(competitionId, newGroupName.trim());
      setNewGroupName('');
      await loadTournament();
      showSuccess(t('floorball.tournaments.groupAdded', 'Group added!'));
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setAddingGroup(false);
    }
  };

  const handleRemoveGroup = async (groupId: string) => {
    if (!competitionId) return;
    setRemovingGroupId(groupId);
    setError(null);
    try {
      await floorballTournamentService.removeGroup(competitionId, groupId);
      if (selectedGroupId === groupId) setSelectedGroupId(null);
      await loadTournament();
      showSuccess(t('floorball.tournaments.groupRemoved', 'Group removed!'));
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setRemovingGroupId(null);
    }
  };

  // ── Team-to-group management ──
  const toggleTeamSelection = (teamId: string) => {
    setSelectedTeamIds((prev) => {
      const next = new Set(prev);
      if (next.has(teamId)) next.delete(teamId);
      else next.add(teamId);
      return next;
    });
  };

  const selectAllAvailable = () => {
    const notInTournament = availableTeamsNotInTournament;
    if (selectedTeamIds.size === notInTournament.length) {
      setSelectedTeamIds(new Set());
    } else {
      setSelectedTeamIds(new Set(notInTournament.map((team) => team.id)));
    }
  };

  const handleAddSelectedTeams = async () => {
    if (!competitionId || !selectedGroupId || selectedTeamIds.size === 0) return;
    setTeamOperationLoading(true);
    setError(null);
    let successCount = 0;
    let failCount = 0;
    for (const teamId of selectedTeamIds) {
      try {
        await floorballTournamentService.addTeamToGroup(competitionId, selectedGroupId, teamId);
        successCount++;
      } catch {
        failCount++;
      }
    }
    setSelectedTeamIds(new Set());
    await loadTournament();
    await loadAvailableTeams();
    if (failCount === 0) {
      showSuccess(t('floorball.tournaments.teamsAdded', '{{count}} team(s) added!', { count: successCount }));
    } else {
      setError(t('floorball.tournaments.someTeamsFailed', '{{success}} added, {{fail}} failed.', { success: successCount, fail: failCount }));
    }
    setTeamOperationLoading(false);
  };

  const handleAddSingleTeam = async (teamId: string) => {
    if (!competitionId || !selectedGroupId) return;
    setTeamOperationLoading(true);
    setError(null);
    try {
      await floorballTournamentService.addTeamToGroup(competitionId, selectedGroupId, teamId);
      await loadTournament();
      await loadAvailableTeams();
      showSuccess(t('floorball.tournaments.teamAdded', 'Team added!'));
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setTeamOperationLoading(false);
    }
  };

  const handleRemoveTeamFromGroup = async (groupId: string, teamId: string) => {
    if (!competitionId) return;
    const key = `${groupId}-${teamId}`;
    setRemovingTeamKey(key);
    setError(null);
    try {
      await floorballTournamentService.removeTeamFromGroup(competitionId, groupId, teamId);
      await loadTournament();
      await loadAvailableTeams();
      showSuccess(t('floorball.tournaments.teamRemoved', 'Team removed!'));
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setRemovingTeamKey(null);
    }
  };

  // ── Lifecycle actions ──
  const handleLifecycleAction = async (action: 'startGroupStage' | 'startPlayoffStage' | 'complete' | 'cancel') => {
    if (!competitionId) return;
    setLifecycleLoading(true);
    setError(null);
    try {
      await floorballTournamentService[action](competitionId);
      await loadTournament();
      showSuccess(t(`floorball.tournaments.lifecycle.${action}Success`, 'Action completed successfully!'));
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLifecycleLoading(false);
    }
  };

  const getStatusLabel = (status: string): string => {
    switch (status) {
      case 'Draft': return t('floorball.tournaments.status.draft', 'Draft');
      case 'GroupStage': return t('floorball.tournaments.status.groupStage', 'Group Stage');
      case 'PlayoffStage': return t('floorball.tournaments.status.playoffStage', 'Playoff Stage');
      case 'Completed': return t('floorball.tournaments.status.completed', 'Completed');
      case 'Cancelled': return t('floorball.tournaments.status.cancelled', 'Cancelled');
      default: return status;
    }
  };

  // ── Computed ──
  const allTournamentTeamIds = useMemo(() => {
    const ids = new Set<string>();
    tournament?.groups?.forEach((group) => {
      group.teams.forEach((team) => ids.add(team.teamId));
    });
    return ids;
  }, [tournament]);

  const selectedGroup = useMemo(() => {
    if (!selectedGroupId) return null;
    return tournament?.groups?.find((group) => group.id === selectedGroupId) ?? null;
  }, [tournament, selectedGroupId]);

  const teamsInSelectedGroup = useMemo(() => {
    return selectedGroup?.teams ?? [];
  }, [selectedGroup]);

  const availableTeamsNotInTournament = useMemo(() => {
    return availableTeams.filter((team) => !allTournamentTeamIds.has(team.id));
  }, [availableTeams, allTournamentTeamIds]);

  const totalTeamCount = allTournamentTeamIds.size;

  if (loadingTournament) {
    return (
      <PageTemplate title={t('floorball.tournaments.editTitle', 'Edit Tournament')}>
        <div className="edit-season-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!tournament) {
    return (
      <PageTemplate title={t('floorball.tournaments.editTitle', 'Edit Tournament')}>
        <ErrorPopup message={t('floorball.tournaments.errors.notFound', 'Tournament not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.tournaments.editTitle', 'Edit Tournament')}>
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}

      <div className="edit-season-container">
        <div className="edit-season-back">
          <button
            type="button"
            className="back-button"
            onClick={() => navigate('/admin/floorball/tournaments')}
          >
            <span aria-hidden="true">&larr;</span>{' '}
            {t('floorball.tournaments.backToList', 'Back to Tournaments')}
          </button>
        </div>
        {/* Tab Navigation */}
        <div className="tab-navigation">
          <button
            className={`tab-button ${activeTab === 'details' ? 'active' : ''}`}
            onClick={() => setActiveTab('details')}
          >
            {t('floorball.tournaments.tabs.details', 'Tournament Details')}
          </button>
          <button
            className={`tab-button ${activeTab === 'groups' ? 'active' : ''}`}
            onClick={() => setActiveTab('groups')}
          >
            {t('floorball.tournaments.tabs.groups', 'Manage Groups')} ({tournament.groups?.length ?? 0})
          </button>
          <button
            className={`tab-button ${activeTab === 'teams' ? 'active' : ''}`}
            onClick={() => setActiveTab('teams')}
          >
            {t('floorball.tournaments.tabs.teams', 'Manage Teams')} ({totalTeamCount})
          </button>
        </div>

        <div className="edit-season-content">
          {/* ─── Details Tab ─── */}
          {activeTab === 'details' && (
            <>
              {/* Lifecycle bar */}
              <div style={{ padding: '16px 24px', borderBottom: '1px solid #e5e7eb', background: '#f9fafb' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 600, fontSize: '13px', color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.04em', marginRight: '8px' }}>
                    {t('floorball.tournaments.statusLabel', 'Status')}: {getStatusLabel(tournament.tournamentStatus)}
                  </span>
                  {tournament.tournamentStatus === 'Draft' && (
                    <button
                      type="button"
                      style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#f59e0b', color: '#fff' }}
                      onClick={() => handleLifecycleAction('startGroupStage')}
                      disabled={lifecycleLoading}
                    >
                      {t('floorball.tournaments.lifecycle.startGroupStage', 'Start Group Stage')}
                    </button>
                  )}
                  {tournament.tournamentStatus === 'GroupStage' && (
                    <button
                      type="button"
                      style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#f59e0b', color: '#fff' }}
                      onClick={() => handleLifecycleAction('startPlayoffStage')}
                      disabled={lifecycleLoading}
                    >
                      {t('floorball.tournaments.lifecycle.startPlayoffStage', 'Start Playoff')}
                    </button>
                  )}
                  {(tournament.tournamentStatus === 'GroupStage' || tournament.tournamentStatus === 'PlayoffStage') && (
                    <button
                      type="button"
                      style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#10b981', color: '#fff' }}
                      onClick={() => handleLifecycleAction('complete')}
                      disabled={lifecycleLoading}
                    >
                      {t('floorball.tournaments.lifecycle.complete', 'Complete Tournament')}
                    </button>
                  )}
                  {tournament.tournamentStatus !== 'Completed' && tournament.tournamentStatus !== 'Cancelled' && (
                    <button
                      type="button"
                      style={{ padding: '6px 14px', fontSize: '13px', border: 'none', borderRadius: '6px', cursor: 'pointer', background: '#dc2626', color: '#fff' }}
                      onClick={() => handleLifecycleAction('cancel')}
                      disabled={lifecycleLoading}
                    >
                      {t('floorball.tournaments.lifecycle.cancel', 'Cancel Tournament')}
                    </button>
                  )}
                </div>
              </div>

              <form onSubmit={handleSubmit} className="edit-season-form">
                <ErrorPopup message={error} />

                {/* Basic Information */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-info-circle"></i>
                    {t('floorball.tournaments.sections.basicInfo', 'Basic Information')}
                  </h3>
                  <div className="form-group">
                    <label htmlFor="edit-name">{t('floorball.tournaments.fields.name', 'Name')} *</label>
                    <input type="text" id="edit-name" name="name" value={formData.name} onChange={handleInputChange} required disabled={loading} />
                  </div>
                  <div className="form-group">
                    <label htmlFor="edit-venue">{t('floorball.tournaments.fields.venue', 'Venue')}</label>
                    <input type="text" id="edit-venue" name="venue" value={formData.venue ?? ''} onChange={handleInputChange} disabled={loading} />
                  </div>
                </div>

                {/* Schedule */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-calendar-alt"></i>
                    {t('floorball.tournaments.sections.schedule', 'Schedule')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-startDate">{t('floorball.tournaments.fields.startDate', 'Start Date')} *</label>
                      <input type="date" id="edit-startDate" name="startDate" value={formData.startDate} onChange={handleInputChange} required disabled={loading} />
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-endDate">{t('floorball.tournaments.fields.endDate', 'End Date')} *</label>
                      <input type="date" id="edit-endDate" name="endDate" value={formData.endDate} onChange={handleInputChange} required disabled={loading} min={formData.startDate} />
                    </div>
                  </div>
                </div>

                {/* Content */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-file-alt"></i>
                    {t('floorball.tournaments.sections.content', 'Description')}
                  </h3>
                  <div className="form-group">
                    <label htmlFor="edit-contentHtml">{t('floorball.tournaments.fields.contentHtml', 'Content (HTML)')}</label>
                    <textarea
                      id="edit-contentHtml"
                      name="contentHtml"
                      value={formData.contentHtml ?? ''}
                      onChange={handleInputChange}
                      disabled={loading}
                      rows={5}
                      style={{ width: '100%', resize: 'vertical', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '6px', fontFamily: 'inherit', fontSize: '14px', boxSizing: 'border-box' }}
                    />
                  </div>
                </div>

                {/* Group Stage Match Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-gavel"></i>
                    {t('floorball.tournaments.sections.groupStageRules', 'Group Stage Match Rules')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-groupStageNumberOfPeriods">{t('floorball.tournaments.fields.numberOfPeriods', 'Number of Periods')}</label>
                      <select id="edit-groupStageNumberOfPeriods" name="groupStageNumberOfPeriods" value={formData.groupStageNumberOfPeriods} onChange={handleInputChange} disabled={loading}>
                        {[1, 2, 3, 4, 5].map((n) => <option key={n} value={n}>{n}</option>)}
                      </select>
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-groupStagePeriodDurationMinutes">{t('floorball.tournaments.fields.periodDuration', 'Period Duration (min)')}</label>
                      <input type="number" id="edit-groupStagePeriodDurationMinutes" name="groupStagePeriodDurationMinutes" value={formData.groupStagePeriodDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                    </div>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowOvertime', 'Allow Overtime')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageAllowOvertime ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageAllowOvertime: !p.groupStageAllowOvertime }))} disabled={loading} aria-pressed={formData.groupStageAllowOvertime}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.groupStageAllowOvertime && (
                    <div className="form-group form-group--indented">
                      <label htmlFor="edit-groupStageOvertimeDurationMinutes">{t('floorball.tournaments.fields.overtimeDuration', 'Overtime Duration (min)')}</label>
                      <input type="number" id="edit-groupStageOvertimeDurationMinutes" name="groupStageOvertimeDurationMinutes" value={formData.groupStageOvertimeDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                    </div>
                  )}
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowShootout', 'Allow Shootout')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageAllowShootout ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageAllowShootout: !p.groupStageAllowShootout }))} disabled={loading} aria-pressed={formData.groupStageAllowShootout}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                </div>

                {/* Playoff Match Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-trophy"></i>
                    {t('floorball.tournaments.sections.playoffRules', 'Playoff Match Rules')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-playoffNumberOfPeriods">{t('floorball.tournaments.fields.numberOfPeriods', 'Number of Periods')}</label>
                      <select id="edit-playoffNumberOfPeriods" name="playoffNumberOfPeriods" value={formData.playoffNumberOfPeriods} onChange={handleInputChange} disabled={loading}>
                        {[1, 2, 3, 4, 5].map((n) => <option key={n} value={n}>{n}</option>)}
                      </select>
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-playoffPeriodDurationMinutes">{t('floorball.tournaments.fields.periodDuration', 'Period Duration (min)')}</label>
                      <input type="number" id="edit-playoffPeriodDurationMinutes" name="playoffPeriodDurationMinutes" value={formData.playoffPeriodDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                    </div>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowOvertime', 'Allow Overtime')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffAllowOvertime ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffAllowOvertime: !p.playoffAllowOvertime }))} disabled={loading} aria-pressed={formData.playoffAllowOvertime}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.playoffAllowOvertime && (
                    <div className="form-group form-group--indented">
                      <label htmlFor="edit-playoffOvertimeDurationMinutes">{t('floorball.tournaments.fields.overtimeDuration', 'Overtime Duration (min)')}</label>
                      <input type="number" id="edit-playoffOvertimeDurationMinutes" name="playoffOvertimeDurationMinutes" value={formData.playoffOvertimeDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                    </div>
                  )}
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.allowShootout', 'Allow Shootout')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffAllowShootout ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffAllowShootout: !p.playoffAllowShootout }))} disabled={loading} aria-pressed={formData.playoffAllowShootout}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                </div>

                {/* Tournament Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-cogs"></i>
                    {t('floorball.tournaments.sections.tournamentRules', 'Tournament Rules')}
                  </h3>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('floorball.tournaments.fields.hasPlayoffStage', 'Has Playoff Stage')}</label>
                    <button type="button" className={`toggle-switch ${formData.hasPlayoffStage ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, hasPlayoffStage: !p.hasPlayoffStage }))} disabled={loading} aria-pressed={formData.hasPlayoffStage}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.hasPlayoffStage && (
                    <>
                      <div className="form-group form-group--indented">
                        <label htmlFor="edit-teamsAdvancingPerGroup">{t('floorball.tournaments.fields.teamsAdvancingPerGroup', 'Teams Advancing Per Group')}</label>
                        <input type="number" id="edit-teamsAdvancingPerGroup" name="teamsAdvancingPerGroup" value={formData.teamsAdvancingPerGroup} onChange={handleInputChange} min={1} max={8} disabled={loading} />
                      </div>
                      <div className="toggle-container">
                        <label className="toggle-label">{t('floorball.tournaments.fields.hasThirdPlaceMatch', 'Has Third Place Match')}</label>
                        <button type="button" className={`toggle-switch ${formData.hasThirdPlaceMatch ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, hasThirdPlaceMatch: !p.hasThirdPlaceMatch }))} disabled={loading} aria-pressed={formData.hasThirdPlaceMatch}>
                          <span className="toggle-switch__slider" />
                        </button>
                      </div>
                    </>
                  )}
                </div>

                <div className="form-actions">
                  <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/floorball/tournaments')} disabled={loading}>
                    {t('common.cancel', 'Cancel')}
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? (<><i className="fas fa-spinner fa-spin"></i> {t('common.saving', 'Saving...')}</>) : t('common.save', 'Save')}
                  </button>
                </div>
              </form>
            </>
          )}

          {/* ─── Groups Tab (CRUD only) ─── */}
          {activeTab === 'groups' && (
            <div className="divisions-management">
              <ErrorPopup message={error} />

              {/* Add new group */}
              <div className="current-divisions-section">
                <h4>{t('floorball.tournaments.addGroup', 'Add Group')}</h4>
                <div style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
                  <input
                    type="text"
                    value={newGroupName}
                    onChange={(e) => setNewGroupName(e.target.value)}
                    placeholder={t('floorball.tournaments.placeholders.groupName', 'e.g. Group A')}
                    disabled={addingGroup}
                    style={{ flex: 1, padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '6px', fontSize: '14px' }}
                  />
                  <button
                    type="button"
                    className="btn btn-primary"
                    onClick={handleAddGroup}
                    disabled={addingGroup || !newGroupName.trim()}
                  >
                    {addingGroup ? (<><i className="fas fa-spinner fa-spin"></i> {t('common.adding', 'Adding...')}</>) : (<><i className="fas fa-plus"></i> {t('common.add', 'Add')}</>)}
                  </button>
                </div>
              </div>

              {/* Existing groups */}
              <div className="available-divisions-section">
                <h4>
                  {t('floorball.tournaments.currentGroups', 'Current Groups')} ({tournament.groups?.length ?? 0})
                </h4>

                {(!tournament.groups || tournament.groups.length === 0) ? (
                  <p className="no-divisions">{t('floorball.tournaments.noGroupsYet', 'No groups yet. Add one above.')}</p>
                ) : (
                  <div className="divisions-list">
                    {tournament.groups.map((group) => (
                      <div key={group.id} className="division-item">
                        <div className="division-info">
                          <span className="division-name">{group.name}</span>
                          <span className="division-team-count">
                            {t('floorball.tournaments.teamCount', '{{count}} team(s)', { count: group.teams.length })}
                          </span>
                        </div>
                        <button
                          type="button"
                          className="btn btn-danger btn-sm"
                          onClick={() => handleRemoveGroup(group.id)}
                          disabled={removingGroupId === group.id || addingGroup}
                        >
                          {removingGroupId === group.id ? (
                            <><i className="fas fa-spinner fa-spin"></i> {t('common.removing', 'Removing...')}</>
                          ) : (
                            <><i className="fas fa-trash-alt"></i> {t('common.remove', 'Remove')}</>
                          )}
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}

          {/* ─── Teams Tab (mirrors EditSeasonPage Teams tab) ─── */}
          {activeTab === 'teams' && (
            <div className="teams-management">
              <ErrorPopup message={error} />

              {(!tournament.groups || tournament.groups.length === 0) ? (
                <div className="tm-empty-state">
                  <i className="fas fa-layer-group"></i>
                  <h4>{t('floorball.tournaments.addGroupsFirst', 'Add groups first')}</h4>
                  <p>{t('floorball.tournaments.addGroupsFirstDesc', 'You need at least one group in this tournament before you can manage teams.')}</p>
                  <button type="button" className="btn btn-primary" onClick={() => setActiveTab('groups')}>
                    <i className="fas fa-plus"></i> {t('floorball.tournaments.goToGroups', 'Go to Manage Groups')}
                  </button>
                </div>
              ) : (
                <>
                  {/* Group selector pills */}
                  <div className="tm-division-selector">
                    <span className="tm-division-selector__label">
                      {t('floorball.tournaments.addingTeamsTo', 'Managing teams for:')}
                    </span>
                    <div className="tm-division-selector__options">
                      {tournament.groups.map((group) => {
                        const count = group.teams.length;
                        const isActive = selectedGroupId === group.id;
                        return (
                          <button
                            key={group.id}
                            type="button"
                            className={`tm-division-pill ${isActive ? 'active' : ''}`}
                            onClick={() => { setSelectedGroupId(group.id); setSelectedTeamIds(new Set()); }}
                          >
                            {group.name}
                            <span className="tm-division-pill__badge">{count}</span>
                          </button>
                        );
                      })}
                    </div>
                  </div>

                  {selectedGroupId && selectedGroup && (
                    <>
                      {/* Current teams in group */}
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-users"></i>
                            {t('floorball.tournaments.teamsInGroup', 'Teams in {{group}}', { group: selectedGroup.name })}
                          </h4>
                          <span className="tm-section__count">
                            {teamsInSelectedGroup.length} {t('floorball.tournaments.teams', 'teams')}
                          </span>
                        </div>

                        {teamsInSelectedGroup.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>{t('floorball.tournaments.noTeamsInGroup', 'No teams in this group yet. Use the table below to add teams.')}</p>
                          </div>
                        ) : (
                          <div className="tm-team-grid">
                            {teamsInSelectedGroup.map((team) => (
                              <div key={team.id} className="tm-team-chip">
                                <div className="tm-team-chip__info">
                                  <span className="tm-team-chip__name">{team.teamName}</span>
                                </div>
                                <button
                                  type="button"
                                  className="tm-team-chip__remove"
                                  onClick={() => handleRemoveTeamFromGroup(selectedGroup.id, team.teamId)}
                                  disabled={removingTeamKey === `${selectedGroup.id}-${team.teamId}`}
                                  title={t('common.remove', 'Remove')}
                                >
                                  {removingTeamKey === `${selectedGroup.id}-${team.teamId}` ? (
                                    <i className="fas fa-spinner fa-spin"></i>
                                  ) : (
                                    <i className="fas fa-times"></i>
                                  )}
                                </button>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>

                      {/* Available teams (paginated table) */}
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-plus-circle"></i>
                            {t('floorball.tournaments.addTeams', 'Add Teams')}
                          </h4>
                        </div>

                        {/* Filters */}
                        <div className="tm-filters">
                          <div className="tm-filters__search">
                            <i className="fas fa-search"></i>
                            <input
                              type="text"
                              placeholder={t('floorball.tournaments.searchTeams', 'Search teams by name...')}
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
                            <option value="">{t('floorball.tournaments.allCategories', 'All Categories')}</option>
                            <option value={TeamCategory.Adult}>{t('floorball.teams.category.adult', 'Adult')}</option>
                            <option value={TeamCategory.Youth}>{t('floorball.teams.category.youth', 'Youth')}</option>
                            <option value={TeamCategory.Women}>{t('floorball.teams.category.women', 'Women')}</option>
                          </select>
                        </div>

                        {/* Multi-select action bar */}
                        {selectedTeamIds.size > 0 && (
                          <div className="tm-action-bar">
                            <span>
                              {t('floorball.tournaments.selectedCount', '{{count}} team(s) selected', { count: selectedTeamIds.size })}
                            </span>
                            <button
                              type="button"
                              className="btn btn-primary btn-sm"
                              onClick={handleAddSelectedTeams}
                              disabled={teamOperationLoading}
                            >
                              {teamOperationLoading ? (
                                <><i className="fas fa-spinner fa-spin"></i> {t('common.adding', 'Adding...')}</>
                              ) : (
                                <><i className="fas fa-plus"></i> {t('floorball.tournaments.addSelectedToGroup', 'Add to {{group}}', { group: selectedGroup.name })}</>
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
                        ) : availableTeamsNotInTournament.length === 0 && availableTeams.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>
                              {searchTerm || teamCategory
                                ? t('floorball.tournaments.noMatchingTeams', 'No teams match your search criteria.')
                                : t('floorball.tournaments.noAvailableTeams', 'No teams available.')}
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
                                        checked={availableTeamsNotInTournament.length > 0 && selectedTeamIds.size === availableTeamsNotInTournament.length}
                                        onChange={selectAllAvailable}
                                        disabled={availableTeamsNotInTournament.length === 0}
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
                                  {availableTeams.map((team) => {
                                    const isInTournament = allTournamentTeamIds.has(team.id);
                                    const isSelected = selectedTeamIds.has(team.id);
                                    return (
                                      <tr key={team.id} className={`${isSelected ? 'selected' : ''} ${isInTournament ? 'in-season' : ''}`}>
                                        <td className="tm-table__checkbox">
                                          <input
                                            type="checkbox"
                                            checked={isSelected}
                                            onChange={() => toggleTeamSelection(team.id)}
                                            disabled={isInTournament || teamOperationLoading}
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
                                          {isInTournament ? (
                                            <span className="tm-status-badge tm-status-badge--added">
                                              <i className="fas fa-check"></i> {t('floorball.tournaments.inTournament', 'In tournament')}
                                            </span>
                                          ) : null}
                                        </td>
                                        <td className="tm-table__actions">
                                          {!isInTournament && (
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

                            <Pagination
                              currentPage={teamsPagination.currentPage}
                              totalPages={teamsPagination.totalPages}
                              totalCount={teamsPagination.totalCount}
                              pageSize={teamsPagination.pageSize}
                              onPageChange={(page) => setTeamsPagination((prev) => ({ ...prev, currentPage: page }))}
                              onPageSizeChange={(size) => setTeamsPagination((prev) => ({ ...prev, pageSize: size, currentPage: 1 }))}
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

export default EditTournamentPage;
