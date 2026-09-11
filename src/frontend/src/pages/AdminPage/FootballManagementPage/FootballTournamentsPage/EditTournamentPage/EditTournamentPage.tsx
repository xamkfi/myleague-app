import { useState, useEffect, useCallback, useMemo } from 'react';
import type { ChangeEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import Pagination from '../../../../../components/Pagination';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import RichTextEditor from '../../../../../components/RichTextEditor';
import { footballTournamentService } from '../../../../../api/football/footballTournamentService';
import { footballTeamService } from '../../../../../api/football/footballTeamService';
import { footballMatchService } from '../../../../../api/football/footballMatchService';
import {
  FOOTBALL_GROUP_STAGE_RULE_DEFAULTS,
  FOOTBALL_PLAYOFF_RULE_DEFAULTS,
  type FootballTournamentDto,
  type CreateFootballTournamentRequest,
  type FootballPlayoffBracketDto,
} from '../../../../../types/football/tournamentTypes';
import { type FootballTeam, TeamCategory, type FootballMatchDto } from '../../../../../types/football/footballTypes';
import TournamentBracket from '../../../../../pages/FootballTournamentPage/components/FootballTournamentBracket';
import FootballAssignTeamsDialog from '../../Components/FootballAssignTeamsDialog';
import TournamentLifecycleBar, { type LifecycleAction, type LifecycleMoreAction } from './components/TournamentLifecycleBar';
import TournamentLifecycleConfirmModal from './components/TournamentLifecycleConfirmModal';
import TournamentMatchesTab from './components/TournamentMatchesTab';
import '../../FootballSeasonsPage/EditSeasonPage/EditSeasonPage.scss';

interface PendingTeamRemoval {
  groupId: string;
  groupName: string;
  teamId: string;
  teamName: string;
}

type TournamentTab = 'details' | 'groups' | 'teams' | 'matches' | 'bracket';

const VALID_TOURNAMENT_TABS: ReadonlyArray<TournamentTab> = ['details', 'groups', 'teams', 'matches', 'bracket'];

const EditTournamentPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { competitionId } = useParams<{ competitionId: string }>();
  const [searchParams, setSearchParams] = useSearchParams();

  const [tournament, setTournament] = useState<FootballTournamentDto | null>(null);
  const [loadingTournament, setLoadingTournament] = useState(true);
  const [formData, setFormData] = useState<CreateFootballTournamentRequest>({
    name: '',
    startDate: '',
    endDate: '',
    venue: '',
    contentHtml: '',
    ...FOOTBALL_GROUP_STAGE_RULE_DEFAULTS,
    ...FOOTBALL_PLAYOFF_RULE_DEFAULTS,
    teamsAdvancingPerGroup: 2,
    hasPlayoffStage: true,
    hasThirdPlaceMatch: false,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const tabParam = searchParams.get('tab');
  const activeTab: TournamentTab = VALID_TOURNAMENT_TABS.includes(tabParam as TournamentTab)
    ? (tabParam as TournamentTab)
    : 'details';
  const setActiveTab = useCallback((tab: TournamentTab): void => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (tab === 'details') {
          next.delete('tab');
        } else {
          next.set('tab', tab);
        }
        return next;
      },
      // Replace history entry so switching tabs doesn't pollute the back stack;
      // the last-active tab still survives forward navigations + browser back, because
      // the URL becomes part of the history entry that React Router pushes when navigating
      // to another page (e.g. clicking a match card in the bracket).
      { replace: true }
    );
  }, [setSearchParams]);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  // Group CRUD state
  const [newGroupName, setNewGroupName] = useState('');
  const [addingGroup, setAddingGroup] = useState(false);
  const [removingGroupId, setRemovingGroupId] = useState<string | null>(null);

  // Teams tab state — mirrors EditSeasonPage Teams tab layout
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [availableTeams, setAvailableTeams] = useState<FootballTeam[]>([]);
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
  const [pendingTeamRemoval, setPendingTeamRemoval] = useState<PendingTeamRemoval | null>(null);

  // Lifecycle actions
  const [lifecycleLoading, setLifecycleLoading] = useState(false);

  // Tournament matches — used by the lifecycle bar to compute readiness of
  // "Start Playoff" / "Complete" actions (group-stage matches done, all matches done).
  const [tournamentMatches, setTournamentMatches] = useState<FootballMatchDto[]>([]);
  const [matchesLoading, setMatchesLoading] = useState<boolean>(false);
  const [matchesError, setMatchesError] = useState<string | null>(null);

  // Bracket tab state — read-only summary for admin verification.
  const [bracket, setBracket] = useState<FootballPlayoffBracketDto | null>(null);
  const [bracketLoading, setBracketLoading] = useState(false);
  const [bracketError, setBracketError] = useState<string | null>(null);
  // Holds the match the admin chose to edit via the inline "Assign teams" affordance on
  // the bracket. Null when the dialog is closed. We resolve the full FootballMatchDto on
  // demand because the bracket only carries a thin summary.
  const [assignTeamsTarget, setAssignTeamsTarget] = useState<FootballMatchDto | null>(null);
  const [assignTeamsLoading, setAssignTeamsLoading] = useState<boolean>(false);
  const [assignTeamsError, setAssignTeamsError] = useState<string | null>(null);

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
      const response = await footballTournamentService.getById(competitionId);
      const data = response.data;
      setTournament(data);
      const groupRules = data.tournamentRules?.groupStageMatchRules;
      const playoffRules = data.tournamentRules?.playoffMatchRules;
      setFormData({
        name: data.name,
        startDate: data.startDate.split('T')[0],
        endDate: data.endDate.split('T')[0],
        venue: data.venue ?? '',
        contentHtml: data.contentHtml ?? '',
        groupStageNumberOfHalves: groupRules?.numberOfHalves ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageNumberOfHalves,
        groupStageHalfDurationMinutes: groupRules?.halfDurationMinutes ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageHalfDurationMinutes,
        groupStagePlayersOnField: groupRules?.playersOnField ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStagePlayersOnField,
        groupStageRequireGoalkeeper: groupRules?.requireGoalkeeper ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageRequireGoalkeeper,
        groupStageMaxSubstitutions: groupRules?.maxSubstitutions ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageMaxSubstitutions,
        groupStageRequireOfficialsToStart: groupRules?.requireOfficialsToStart ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageRequireOfficialsToStart,
        groupStageAllowExtraTime: groupRules?.allowExtraTime ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageAllowExtraTime,
        groupStageExtraTimeHalfCount: groupRules?.extraTimeHalfCount ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageExtraTimeHalfCount,
        groupStageExtraTimeHalfDurationMinutes: groupRules?.extraTimeHalfDurationMinutes ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageExtraTimeHalfDurationMinutes,
        groupStageAllowPenaltyShootout: groupRules?.allowPenaltyShootout ?? FOOTBALL_GROUP_STAGE_RULE_DEFAULTS.groupStageAllowPenaltyShootout,
        playoffNumberOfHalves: playoffRules?.numberOfHalves ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffNumberOfHalves,
        playoffHalfDurationMinutes: playoffRules?.halfDurationMinutes ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffHalfDurationMinutes,
        playoffPlayersOnField: playoffRules?.playersOnField ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffPlayersOnField,
        playoffRequireGoalkeeper: playoffRules?.requireGoalkeeper ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffRequireGoalkeeper,
        playoffMaxSubstitutions: playoffRules?.maxSubstitutions ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffMaxSubstitutions,
        playoffRequireOfficialsToStart: playoffRules?.requireOfficialsToStart ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffRequireOfficialsToStart,
        playoffAllowExtraTime: playoffRules?.allowExtraTime ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffAllowExtraTime,
        playoffExtraTimeHalfCount: playoffRules?.extraTimeHalfCount ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffExtraTimeHalfCount,
        playoffExtraTimeHalfDurationMinutes: playoffRules?.extraTimeHalfDurationMinutes ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffExtraTimeHalfDurationMinutes,
        playoffAllowPenaltyShootout: playoffRules?.allowPenaltyShootout ?? FOOTBALL_PLAYOFF_RULE_DEFAULTS.playoffAllowPenaltyShootout,
        teamsAdvancingPerGroup: data.tournamentRules?.teamsAdvancingPerGroup ?? 2,
        hasPlayoffStage: data.tournamentRules?.hasPlayoffStage ?? true,
        hasThirdPlaceMatch: data.tournamentRules?.hasThirdPlaceMatch ?? false,
      });
    } catch {
      setError(t('football.tournaments.errors.loadFailed', 'Failed to load tournament'));
    } finally {
      setLoadingTournament(false);
    }
  }, [competitionId, t]);

  useEffect(() => {
    loadTournament();
  }, [loadTournament]);

  // Load all matches for this tournament so the lifecycle bar can derive readiness.
  // The endpoint returns both group-stage and playoff matches; the bar discriminates
  // via the `tournamentGroupId` field. Refetched whenever `loadTournament` is called
  // (status changes, group changes, etc.).
  const loadTournamentMatches = useCallback(async () => {
    if (!competitionId) return;
    try {
      setMatchesLoading(true);
      setMatchesError(null);
      const response = await footballMatchService.getBySeason(competitionId);
      setTournamentMatches(Array.isArray(response.data) ? response.data : []);
    } catch (err) {
      setTournamentMatches([]);
      setMatchesError(err instanceof Error ? err.message : 'Failed to load matches');
    } finally {
      setMatchesLoading(false);
    }
  }, [competitionId]);

  useEffect(() => {
    if (activeTab !== 'details') return;
    loadTournamentMatches();
  }, [activeTab, loadTournamentMatches, tournament?.tournamentStatus]);

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
      const response = await footballTeamService.getAllWithoutRoster({
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

  // Load playoff bracket on demand (admin Bracket tab) once playoffs are running or completed.
  const showBracketTab = useMemo<boolean>(() => {
    if (!tournament) return false;
    if (!tournament.tournamentRules?.hasPlayoffStage) return false;
    return (
      tournament.tournamentStatus === 'PlayoffStage' ||
      tournament.tournamentStatus === 'Completed'
    );
  }, [tournament]);

  useEffect(() => {
    if (!competitionId) return;
    if (activeTab !== 'bracket') return;
    if (!showBracketTab) return;
    let cancelled = false;
    const load = async () => {
      try {
        setBracketLoading(true);
        setBracketError(null);
        const response = await footballTournamentService.getPlayoffBracket(competitionId);
        if (!cancelled) {
          setBracket(response.data);
        }
      } catch (err) {
        if (!cancelled) {
          setBracketError(err instanceof Error ? err.message : 'Failed to load playoff bracket');
        }
      } finally {
        if (!cancelled) {
          setBracketLoading(false);
        }
      }
    };
    load();
    return () => {
      cancelled = true;
    };
  }, [competitionId, activeTab, showBracketTab]);

  const parseApiError = (err: unknown): string => {
    const msg = err instanceof Error ? err.message : String(err);
    if (msg?.includes('Failed to fetch') || msg?.includes('NetworkError'))
      return t('football.tournaments.errors.networkError', 'Network error. Please check your connection.');
    if (msg?.includes('HTTP 400'))
      return t('football.tournaments.errors.validationError', 'Invalid data. Please check your input.');
    if (msg?.includes('HTTP 404'))
      return t('football.tournaments.errors.notFound', 'Not found. It may have been deleted.');
    if (msg?.includes('HTTP 500'))
      return t('football.tournaments.errors.serverError', 'Server error. Please try again later.');
    return msg || t('football.tournaments.errors.operationFailed', 'Operation failed. Please try again.');
  };

  const handleInputChange = (e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleContentChange = useCallback((html: string): void => {
    setFormData((prev) => ({ ...prev, contentHtml: html }));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!competitionId) return;
    setLoading(true);
    setError(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('football.tournaments.validation.nameRequired', 'Tournament name is required'));
      }
      if (!formData.startDate) {
        throw new Error(t('football.tournaments.validation.startDateRequired', 'Start date is required'));
      }
      if (!formData.endDate) {
        throw new Error(t('football.tournaments.validation.endDateRequired', 'End date is required'));
      }

      await footballTournamentService.update(competitionId, formData);
      showSuccess(t('football.tournaments.updated', 'Tournament updated successfully!'));
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
      await footballTournamentService.addGroup(competitionId, newGroupName.trim());
      setNewGroupName('');
      await loadTournament();
      showSuccess(t('football.tournaments.groupAdded', 'Group added!'));
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
      await footballTournamentService.removeGroup(competitionId, groupId);
      if (selectedGroupId === groupId) setSelectedGroupId(null);
      await loadTournament();
      showSuccess(t('football.tournaments.groupRemoved', 'Group removed!'));
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
        await footballTournamentService.addTeamToGroup(competitionId, selectedGroupId, teamId);
        successCount++;
      } catch {
        failCount++;
      }
    }
    setSelectedTeamIds(new Set());
    await loadTournament();
    await loadAvailableTeams();
    if (failCount === 0) {
      showSuccess(t('football.tournaments.teamsAdded', '{{count}} team(s) added!', { count: successCount }));
    } else {
      setError(t('football.tournaments.someTeamsFailed', '{{success}} added, {{fail}} failed.', { success: successCount, fail: failCount }));
    }
    setTeamOperationLoading(false);
  };

  const handleAddSingleTeam = async (teamId: string) => {
    if (!competitionId || !selectedGroupId) return;
    setTeamOperationLoading(true);
    setError(null);
    try {
      await footballTournamentService.addTeamToGroup(competitionId, selectedGroupId, teamId);
      await loadTournament();
      await loadAvailableTeams();
      showSuccess(t('football.tournaments.teamAdded', 'Team added!'));
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setTeamOperationLoading(false);
    }
  };

  const requestRemoveTeamFromGroup = (
    groupId: string,
    groupName: string,
    teamId: string,
    teamName: string
  ): void => {
    if (tournament?.tournamentStatus !== 'Draft') return;
    setError(null);
    setPendingTeamRemoval({ groupId, groupName, teamId, teamName });
  };

  const cancelRemoveTeamFromGroup = (): void => {
    if (removingTeamKey) return;
    setPendingTeamRemoval(null);
  };

  const confirmRemoveTeamFromGroup = async (): Promise<void> => {
    if (!competitionId || !pendingTeamRemoval) return;
    if (tournament?.tournamentStatus !== 'Draft') {
      setPendingTeamRemoval(null);
      return;
    }
    const { groupId, teamId } = pendingTeamRemoval;
    const key = `${groupId}-${teamId}`;
    setRemovingTeamKey(key);
    setError(null);
    try {
      await footballTournamentService.removeTeamFromGroup(competitionId, groupId, teamId);
      await loadTournament();
      await loadAvailableTeams();
      setPendingTeamRemoval(null);
      showSuccess(
        t('football.tournaments.teamGroup.teamRemovedSuccess', 'Team removed from group.')
      );
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setRemovingTeamKey(null);
    }
  };

  // ── Lifecycle actions ──
  const handleLifecycleAction = async (action: LifecycleAction): Promise<void> => {
    if (!competitionId) return;
    setLifecycleLoading(true);
    setError(null);
    try {
      await footballTournamentService[action](competitionId);
      await loadTournament();
      await loadTournamentMatches();
      showSuccess(t(`football.tournaments.lifecycle.${action}Success`, 'Action completed successfully!'));
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLifecycleLoading(false);
    }
  };

  // ── Non-lifecycle "more" actions (delete) ──
  // Kept separate from `handleLifecycleAction` because deleting hits a different API and the
  // happy path navigates the user back to the tournaments list rather than re-fetching the
  // (now non-existent) tournament.
  const handleLifecycleMoreAction = async (action: LifecycleMoreAction): Promise<void> => {
    if (!competitionId) return;
    if (action !== 'delete') return;
    setLifecycleLoading(true);
    setError(null);
    try {
      await footballTournamentService.delete(competitionId);
      navigate('/admin/football/tournaments');
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLifecycleLoading(false);
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

  // Mirrors the backend rule: teams should only be mutated while the tournament
  // is still in Draft. Once the group stage starts (or later), the chip-level
  // remove control disables itself and surfaces the reason via tooltip.
  const canModifyTeams: boolean = tournament?.tournamentStatus === 'Draft';

  if (loadingTournament) {
    return (
      <PageTemplate title={t('football.tournaments.editTitle', 'Edit Tournament')}>
        <div className="edit-season-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!tournament) {
    return (
      <PageTemplate title={t('football.tournaments.editTitle', 'Edit Tournament')}>
        <ErrorPopup message={t('football.tournaments.errors.notFound', 'Tournament not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('football.tournaments.editTitle', 'Edit Tournament')}>
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
            onClick={() => navigate('/admin/football/tournaments')}
          >
            <span aria-hidden="true">&larr;</span>{' '}
            {t('football.tournaments.backToList', 'Back to Tournaments')}
          </button>
        </div>
        {/* Tab Navigation */}
        <div className="tab-navigation">
          <button
            className={`tab-button ${activeTab === 'details' ? 'active' : ''}`}
            onClick={() => setActiveTab('details')}
          >
            {t('football.tournaments.tabs.details', 'Tournament Details')}
          </button>
          <button
            className={`tab-button ${activeTab === 'groups' ? 'active' : ''}`}
            onClick={() => setActiveTab('groups')}
          >
            {t('football.tournaments.tabs.groups', 'Manage Groups')} ({tournament.groups?.length ?? 0})
          </button>
          <button
            className={`tab-button ${activeTab === 'teams' ? 'active' : ''}`}
            onClick={() => setActiveTab('teams')}
          >
            {t('football.tournaments.tabs.teams', 'Manage Teams')} ({totalTeamCount})
          </button>
          <button
            className={`tab-button ${activeTab === 'matches' ? 'active' : ''}`}
            onClick={() => setActiveTab('matches')}
          >
            {t('football.tournaments.tabs.matches', 'Matches')}
          </button>
          {showBracketTab && (
            <button
              className={`tab-button ${activeTab === 'bracket' ? 'active' : ''}`}
              onClick={() => setActiveTab('bracket')}
            >
              {t('football.tournaments.tabs.bracket', 'Pudotuspelikaavio')}
            </button>
          )}
        </div>

        <div className="edit-season-content">
          {/* ─── Details Tab ─── */}
          {activeTab === 'details' && (
            <>
              <TournamentLifecycleBar
                tournament={tournament}
                matches={tournamentMatches}
                matchesLoading={matchesLoading}
                matchesError={matchesError}
                loading={lifecycleLoading}
                onAction={handleLifecycleAction}
                onMoreAction={handleLifecycleMoreAction}
              />

              <form onSubmit={handleSubmit} className="edit-season-form">
                <ErrorPopup message={error} />

                {/* Basic Information */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-info-circle"></i>
                    {t('football.tournaments.sections.basicInfo', 'Basic Information')}
                  </h3>
                  <div className="form-group">
                    <label htmlFor="edit-name">{t('football.tournaments.fields.name', 'Name')} *</label>
                    <input type="text" id="edit-name" name="name" value={formData.name} onChange={handleInputChange} required disabled={loading} />
                  </div>
                  <div className="form-group">
                    <label htmlFor="edit-venue">{t('football.tournaments.fields.venue', 'Venue')}</label>
                    <input type="text" id="edit-venue" name="venue" value={formData.venue ?? ''} onChange={handleInputChange} disabled={loading} />
                  </div>
                </div>

                {/* Schedule */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-calendar-alt"></i>
                    {t('football.tournaments.sections.schedule', 'Schedule')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-startDate">{t('football.tournaments.fields.startDate', 'Start Date')} *</label>
                      <input type="date" id="edit-startDate" name="startDate" value={formData.startDate} onChange={handleInputChange} required disabled={loading} />
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-endDate">{t('football.tournaments.fields.endDate', 'End Date')} *</label>
                      <input type="date" id="edit-endDate" name="endDate" value={formData.endDate} onChange={handleInputChange} required disabled={loading} min={formData.startDate} />
                    </div>
                  </div>
                </div>

                {/* Content */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-file-alt"></i>
                    {t('football.tournaments.sections.content', 'Description')}
                  </h3>
                  <div className="form-group">
                    <label htmlFor="edit-contentHtml">{t('football.tournaments.fields.contentHtml', 'Content (HTML)')}</label>
                    <RichTextEditor
                      id="edit-contentHtml"
                      value={formData.contentHtml ?? ''}
                      onChange={handleContentChange}
                      readOnly={loading}
                      variant="compact"
                      showMatchInsert={false}
                      placeholder={t('football.tournaments.placeholders.content', 'Tournament description...')}
                    />
                  </div>
                </div>

                {/* Group Stage Match Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-gavel"></i>
                    {t('football.tournaments.sections.groupStageRules', 'Group Stage Match Rules')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-groupStageNumberOfHalves">{t('football.tournaments.fields.numberOfHalves', 'Number of Halves')}</label>
                      <select id="edit-groupStageNumberOfHalves" name="groupStageNumberOfHalves" value={formData.groupStageNumberOfHalves} onChange={handleInputChange} disabled={loading}>
                        {[1, 2].map((n) => <option key={n} value={n}>{n}</option>)}
                      </select>
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-groupStageHalfDurationMinutes">{t('football.tournaments.fields.halfDurationMinutes', 'Half Duration (min)')}</label>
                      <input type="number" id="edit-groupStageHalfDurationMinutes" name="groupStageHalfDurationMinutes" value={formData.groupStageHalfDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                    </div>
                  </div>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-groupStagePlayersOnField">{t('football.tournaments.fields.playersOnField', 'Players on Field')}</label>
                      <input type="number" id="edit-groupStagePlayersOnField" name="groupStagePlayersOnField" value={formData.groupStagePlayersOnField} onChange={handleInputChange} min={5} max={11} disabled={loading} />
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-groupStageMaxSubstitutions">{t('football.tournaments.fields.maxSubstitutions', 'Max Substitutions')}</label>
                      <input type="number" id="edit-groupStageMaxSubstitutions" name="groupStageMaxSubstitutions" value={formData.groupStageMaxSubstitutions} onChange={handleInputChange} min={0} max={99} disabled={loading} />
                    </div>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.requireGoalkeeper', 'Require Goalkeeper')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageRequireGoalkeeper ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageRequireGoalkeeper: !p.groupStageRequireGoalkeeper }))} disabled={loading} aria-pressed={formData.groupStageRequireGoalkeeper}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.requireOfficialsToStart', 'Require Officials to Start')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageRequireOfficialsToStart ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageRequireOfficialsToStart: !p.groupStageRequireOfficialsToStart }))} disabled={loading} aria-pressed={formData.groupStageRequireOfficialsToStart}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.allowExtraTime', 'Allow Extra Time')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageAllowExtraTime ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageAllowExtraTime: !p.groupStageAllowExtraTime }))} disabled={loading} aria-pressed={formData.groupStageAllowExtraTime}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.groupStageAllowExtraTime && (
                    <div className="form-row">
                      <div className="form-group form-group--indented">
                        <label htmlFor="edit-groupStageExtraTimeHalfCount">{t('football.tournaments.fields.extraTimeHalfCount', 'Extra Time Halves')}</label>
                        <input type="number" id="edit-groupStageExtraTimeHalfCount" name="groupStageExtraTimeHalfCount" value={formData.groupStageExtraTimeHalfCount} onChange={handleInputChange} min={1} max={4} disabled={loading} />
                      </div>
                      <div className="form-group form-group--indented">
                        <label htmlFor="edit-groupStageExtraTimeHalfDurationMinutes">{t('football.tournaments.fields.extraTimeHalfDurationMinutes', 'Extra Time Half Duration (min)')}</label>
                        <input type="number" id="edit-groupStageExtraTimeHalfDurationMinutes" name="groupStageExtraTimeHalfDurationMinutes" value={formData.groupStageExtraTimeHalfDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                      </div>
                    </div>
                  )}
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.allowPenaltyShootout', 'Allow Penalty Shootout')}</label>
                    <button type="button" className={`toggle-switch ${formData.groupStageAllowPenaltyShootout ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, groupStageAllowPenaltyShootout: !p.groupStageAllowPenaltyShootout }))} disabled={loading} aria-pressed={formData.groupStageAllowPenaltyShootout}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                </div>

                {/* Playoff Match Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-trophy"></i>
                    {t('football.tournaments.sections.playoffRules', 'Playoff Match Rules')}
                  </h3>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-playoffNumberOfHalves">{t('football.tournaments.fields.numberOfHalves', 'Number of Halves')}</label>
                      <select id="edit-playoffNumberOfHalves" name="playoffNumberOfHalves" value={formData.playoffNumberOfHalves} onChange={handleInputChange} disabled={loading}>
                        {[1, 2].map((n) => <option key={n} value={n}>{n}</option>)}
                      </select>
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-playoffHalfDurationMinutes">{t('football.tournaments.fields.halfDurationMinutes', 'Half Duration (min)')}</label>
                      <input type="number" id="edit-playoffHalfDurationMinutes" name="playoffHalfDurationMinutes" value={formData.playoffHalfDurationMinutes} onChange={handleInputChange} min={1} max={60} disabled={loading} />
                    </div>
                  </div>
                  <div className="form-row">
                    <div className="form-group">
                      <label htmlFor="edit-playoffPlayersOnField">{t('football.tournaments.fields.playersOnField', 'Players on Field')}</label>
                      <input type="number" id="edit-playoffPlayersOnField" name="playoffPlayersOnField" value={formData.playoffPlayersOnField} onChange={handleInputChange} min={5} max={11} disabled={loading} />
                    </div>
                    <div className="form-group">
                      <label htmlFor="edit-playoffMaxSubstitutions">{t('football.tournaments.fields.maxSubstitutions', 'Max Substitutions')}</label>
                      <input type="number" id="edit-playoffMaxSubstitutions" name="playoffMaxSubstitutions" value={formData.playoffMaxSubstitutions} onChange={handleInputChange} min={0} max={99} disabled={loading} />
                    </div>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.requireGoalkeeper', 'Require Goalkeeper')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffRequireGoalkeeper ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffRequireGoalkeeper: !p.playoffRequireGoalkeeper }))} disabled={loading} aria-pressed={formData.playoffRequireGoalkeeper}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.requireOfficialsToStart', 'Require Officials to Start')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffRequireOfficialsToStart ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffRequireOfficialsToStart: !p.playoffRequireOfficialsToStart }))} disabled={loading} aria-pressed={formData.playoffRequireOfficialsToStart}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.allowExtraTime', 'Allow Extra Time')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffAllowExtraTime ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffAllowExtraTime: !p.playoffAllowExtraTime }))} disabled={loading} aria-pressed={formData.playoffAllowExtraTime}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.playoffAllowExtraTime && (
                    <div className="form-row">
                      <div className="form-group form-group--indented">
                        <label htmlFor="edit-playoffExtraTimeHalfCount">{t('football.tournaments.fields.extraTimeHalfCount', 'Extra Time Halves')}</label>
                        <input type="number" id="edit-playoffExtraTimeHalfCount" name="playoffExtraTimeHalfCount" value={formData.playoffExtraTimeHalfCount} onChange={handleInputChange} min={1} max={4} disabled={loading} />
                      </div>
                      <div className="form-group form-group--indented">
                        <label htmlFor="edit-playoffExtraTimeHalfDurationMinutes">{t('football.tournaments.fields.extraTimeHalfDurationMinutes', 'Extra Time Half Duration (min)')}</label>
                        <input type="number" id="edit-playoffExtraTimeHalfDurationMinutes" name="playoffExtraTimeHalfDurationMinutes" value={formData.playoffExtraTimeHalfDurationMinutes} onChange={handleInputChange} min={1} max={30} disabled={loading} />
                      </div>
                    </div>
                  )}
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.allowPenaltyShootout', 'Allow Penalty Shootout')}</label>
                    <button type="button" className={`toggle-switch ${formData.playoffAllowPenaltyShootout ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, playoffAllowPenaltyShootout: !p.playoffAllowPenaltyShootout }))} disabled={loading} aria-pressed={formData.playoffAllowPenaltyShootout}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                </div>

                {/* Tournament Rules */}
                <div className="form-section">
                  <h3 className="form-section__title">
                    <i className="fas fa-cogs"></i>
                    {t('football.tournaments.sections.tournamentRules', 'Tournament Rules')}
                  </h3>
                  <div className="toggle-container">
                    <label className="toggle-label">{t('football.tournaments.fields.hasPlayoffStage', 'Has Playoff Stage')}</label>
                    <button type="button" className={`toggle-switch ${formData.hasPlayoffStage ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, hasPlayoffStage: !p.hasPlayoffStage }))} disabled={loading} aria-pressed={formData.hasPlayoffStage}>
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                  {formData.hasPlayoffStage && (
                    <>
                      <div className="form-group form-group--indented">
                        <label htmlFor="edit-teamsAdvancingPerGroup">{t('football.tournaments.fields.teamsAdvancingPerGroup', 'Teams Advancing Per Group')}</label>
                        <input type="number" id="edit-teamsAdvancingPerGroup" name="teamsAdvancingPerGroup" value={formData.teamsAdvancingPerGroup} onChange={handleInputChange} min={1} max={8} disabled={loading} />
                      </div>
                      <div className="toggle-container">
                        <label className="toggle-label">{t('football.tournaments.fields.hasThirdPlaceMatch', 'Has Third Place Match')}</label>
                        <button type="button" className={`toggle-switch ${formData.hasThirdPlaceMatch ? 'active' : ''}`} onClick={() => setFormData((p) => ({ ...p, hasThirdPlaceMatch: !p.hasThirdPlaceMatch }))} disabled={loading} aria-pressed={formData.hasThirdPlaceMatch}>
                          <span className="toggle-switch__slider" />
                        </button>
                      </div>
                    </>
                  )}
                </div>

                <div className="form-actions">
                  <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/football/tournaments')} disabled={loading}>
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
                <h4>{t('football.tournaments.addGroup', 'Add Group')}</h4>
                <div style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
                  <input
                    type="text"
                    value={newGroupName}
                    onChange={(e) => setNewGroupName(e.target.value)}
                    placeholder={t('football.tournaments.placeholders.groupName', 'e.g. Group A')}
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
                  {t('football.tournaments.currentGroups', 'Current Groups')} ({tournament.groups?.length ?? 0})
                </h4>

                {(!tournament.groups || tournament.groups.length === 0) ? (
                  <p className="no-divisions">{t('football.tournaments.noGroupsYet', 'No groups yet. Add one above.')}</p>
                ) : (
                  <div className="divisions-list">
                    {tournament.groups.map((group) => (
                      <div key={group.id} className="division-item">
                        <div className="division-info">
                          <span className="division-name">{group.name}</span>
                          <span className="division-team-count">
                            {t('football.tournaments.teamCount', '{{count}} team(s)', { count: group.teams.length })}
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
                  <h4>{t('football.tournaments.addGroupsFirst', 'Add groups first')}</h4>
                  <p>{t('football.tournaments.addGroupsFirstDesc', 'You need at least one group in this tournament before you can manage teams.')}</p>
                  <button type="button" className="btn btn-primary" onClick={() => setActiveTab('groups')}>
                    <i className="fas fa-plus"></i> {t('football.tournaments.goToGroups', 'Go to Manage Groups')}
                  </button>
                </div>
              ) : (
                <>
                  {/* Group selector pills */}
                  <div className="tm-division-selector">
                    <span className="tm-division-selector__label">
                      {t('football.tournaments.addingTeamsTo', 'Managing teams for:')}
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
                            {t('football.tournaments.teamsInGroup', 'Teams in {{group}}', { group: selectedGroup.name })}
                          </h4>
                          <span className="tm-section__count">
                            {teamsInSelectedGroup.length} {t('football.tournaments.teams', 'teams')}
                          </span>
                        </div>

                        {teamsInSelectedGroup.length === 0 ? (
                          <div className="tm-section__empty">
                            <p>{t('football.tournaments.noTeamsInGroup', 'No teams in this group yet. Use the table below to add teams.')}</p>
                          </div>
                        ) : (
                          <div className="tm-team-grid">
                            {teamsInSelectedGroup.map((team) => {
                              const removeKey: string = `${selectedGroup.id}-${team.teamId}`;
                              const isRemovingThisTeam: boolean = removingTeamKey === removeKey;
                              const removeDisabled: boolean = !canModifyTeams || isRemovingThisTeam;
                              const removeTooltip: string = canModifyTeams
                                ? t(
                                    'football.tournaments.teamGroup.removeTeamTooltip',
                                    'Remove this team from the group'
                                  )
                                : t(
                                    'football.tournaments.teamGroup.removeTeamDisabledReason',
                                    'Teams cannot be removed once the group stage has started.'
                                  );
                              return (
                                <div key={team.id} className="tm-team-chip">
                                  <div className="tm-team-chip__info">
                                    <span className="tm-team-chip__name">{team.teamName}</span>
                                  </div>
                                  <button
                                    type="button"
                                    className="tm-team-chip__remove"
                                    onClick={(): void => requestRemoveTeamFromGroup(
                                      selectedGroup.id,
                                      selectedGroup.name,
                                      team.teamId,
                                      team.teamName
                                    )}
                                    disabled={removeDisabled}
                                    title={removeTooltip}
                                    aria-label={`${t(
                                      'football.tournaments.teamGroup.removeTeam',
                                      'Remove from group'
                                    )}: ${team.teamName}`}
                                  >
                                    {isRemovingThisTeam ? (
                                      <i className="fas fa-spinner fa-spin" aria-hidden="true"></i>
                                    ) : (
                                      <i className="fas fa-times" aria-hidden="true"></i>
                                    )}
                                  </button>
                                </div>
                              );
                            })}
                          </div>
                        )}
                      </div>

                      {/* Available teams (paginated table) */}
                      <div className="tm-section">
                        <div className="tm-section__header">
                          <h4>
                            <i className="fas fa-plus-circle"></i>
                            {t('football.tournaments.addTeams', 'Add Teams')}
                          </h4>
                        </div>

                        {/* Filters */}
                        <div className="tm-filters">
                          <div className="tm-filters__search">
                            <i className="fas fa-search"></i>
                            <input
                              type="text"
                              placeholder={t('football.tournaments.searchTeams', 'Search teams by name...')}
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
                            <option value="">{t('football.tournaments.allCategories', 'All Categories')}</option>
                            <option value={TeamCategory.Adult}>{t('football.teams.category.adult', 'Adult')}</option>
                            <option value={TeamCategory.Youth}>{t('football.teams.category.youth', 'Youth')}</option>
                            <option value={TeamCategory.Women}>{t('football.teams.category.women', 'Women')}</option>
                          </select>
                        </div>

                        {/* Multi-select action bar */}
                        {selectedTeamIds.size > 0 && (
                          <div className="tm-action-bar">
                            <span>
                              {t('football.tournaments.selectedCount', '{{count}} team(s) selected', { count: selectedTeamIds.size })}
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
                                <><i className="fas fa-plus"></i> {t('football.tournaments.addSelectedToGroup', 'Add to {{group}}', { group: selectedGroup.name })}</>
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
                                ? t('football.tournaments.noMatchingTeams', 'No teams match your search criteria.')
                                : t('football.tournaments.noAvailableTeams', 'No teams available.')}
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
                                    <th>{t('football.teams.name', 'Team')}</th>
                                    <th>{t('football.teams.club', 'Club')}</th>
                                    <th>{t('football.teams.category.label', 'Category')}</th>
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
                                              <i className="fas fa-check"></i> {t('football.tournaments.inTournament', 'In tournament')}
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

          {pendingTeamRemoval && (
            <TournamentLifecycleConfirmModal
              isOpen={true}
              variant="default"
              title={t(
                'football.tournaments.teamGroup.confirmRemoveTitle',
                'Remove team from group'
              )}
              description={`${t(
                'football.tournaments.teamGroup.confirmRemoveBody',
                'Remove {{teamName}} from {{groupName}}?',
                {
                  teamName: pendingTeamRemoval.teamName,
                  groupName: pendingTeamRemoval.groupName,
                }
              )} ${t(
                'football.tournaments.teamGroup.confirmRemoveDetail',
                'The team will become available again in the list below.'
              )}`}
              prerequisites={[]}
              confirmLabel={t(
                'football.tournaments.teamGroup.confirmButton',
                'Remove from group'
              )}
              loading={removingTeamKey !== null}
              onConfirm={confirmRemoveTeamFromGroup}
              onCancel={cancelRemoveTeamFromGroup}
            />
          )}

          {activeTab === 'matches' && (
            <TournamentMatchesTab
              tournament={tournament}
              onTournamentUpdated={(updated): void => setTournament(updated)}
            />
          )}

          {activeTab === 'bracket' && showBracketTab && (
            <div className="bracket-management" style={{ padding: '24px' }}>
              <ErrorPopup message={error} />
              {bracketLoading && (
                <div style={{ padding: '24px', textAlign: 'center', color: '#6b7280' }}>
                  {t('common.loading', 'Ladataan...')}
                </div>
              )}
              {bracketError && !bracketLoading && (
                <div style={{ padding: '16px', background: '#fef2f2', border: '1px solid #fecaca', borderRadius: '8px', color: '#b91c1c', marginBottom: '16px' }}>
                  {bracketError}
                </div>
              )}
              {!bracketLoading && !bracketError && bracket && bracket.rounds.length > 0 && (
                <TournamentBracket
                  bracket={bracket}
                  compact
                  linkMode="admin"
                  /* Keep the user inside the tournament edit context when they Close the */
                  /* match-management view they just opened from the bracket.            */
                  adminReturnTo={`/admin/football/tournaments/${competitionId}/edit?tab=bracket`}
                  onAssignTeams={async (matchId: string) => {
                    // The bracket DTO doesn't carry the full FootballMatchDto, so fetch
                    // it on demand before opening the dialog. We surface fetch errors via
                    // the dedicated assign-teams error state instead of the page-level
                    // bracket error so the user can still see the bracket below.
                    try {
                      setAssignTeamsLoading(true);
                      setAssignTeamsError(null);
                      const response = await footballMatchService.getById(matchId);
                      if (response.success && response.data) {
                        setAssignTeamsTarget(response.data);
                      } else {
                        setAssignTeamsError(response.message ?? 'Failed to load match');
                      }
                    } catch (err: unknown) {
                      setAssignTeamsError(err instanceof Error ? err.message : 'Failed to load match');
                    } finally {
                      setAssignTeamsLoading(false);
                    }
                  }}
                />
              )}
              {assignTeamsLoading && (
                <div style={{ padding: '12px', color: '#6b7280' }}>
                  {t('common.loading', 'Ladataan...')}
                </div>
              )}
              {assignTeamsError && (
                <div style={{ padding: '12px', color: '#b91c1c' }}>
                  {assignTeamsError}
                </div>
              )}
              {assignTeamsTarget && (
                <FootballAssignTeamsDialog
                  isOpen={true}
                  match={assignTeamsTarget}
                  onClose={() => setAssignTeamsTarget(null)}
                  onSaved={async () => {
                    setAssignTeamsTarget(null);
                    // Refresh the bracket so any propagated downstream slot updates show up
                    // immediately. The handler ran a SaveChanges on the backend, so a re-fetch
                    // is the simplest path to a consistent view.
                    if (competitionId) {
                      try {
                        const refreshed = await footballTournamentService.getPlayoffBracket(competitionId);
                        setBracket(refreshed.data);
                      } catch (err: unknown) {
                        setBracketError(err instanceof Error ? err.message : 'Failed to refresh bracket');
                      }
                    }
                  }}
                />
              )}
              {!bracketLoading && !bracketError && (!bracket || bracket.rounds.length === 0) && (
                <div style={{ padding: '24px', textAlign: 'center', color: '#6b7280' }}>
                  {t(
                    'football.tournaments.bracket.empty',
                    'Pudotuspelikaaviota ei ole vielä luotu. Käynnistä pudotuspelivaihe Tiedot-välilehdeltä.'
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
};

export default EditTournamentPage;
