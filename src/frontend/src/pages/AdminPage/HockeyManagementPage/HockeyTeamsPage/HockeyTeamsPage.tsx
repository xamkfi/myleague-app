import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import type { HockeyTeamCategory, HockeyTeamDto } from '../../../../types/hockey/hockeyTypes';
import TeamsTable from './components/TeamsTable';
import TeamCategoryFilter from '../../../../components/TeamCategoryFilter/TeamCategoryFilter';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { loadClubNameMap } from '../../../../utils/hockeyLookups';
import './HockeyTeamsPage.scss';

function HockeyTeamsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [clubNames, setClubNames] = useState<Map<string, string>>(new Map());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<HockeyTeamCategory[]>([]);
  const [reloadToken, setReloadToken] = useState(0);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      const nextSearch = searchTerm.trim();
      setDebouncedSearch((previous) => {
        if (previous !== nextSearch) {
          setCurrentPage(1);
        }
        return nextSearch;
      });
    }, 300);
    return () => window.clearTimeout(timeoutId);
  }, [searchTerm]);

  useEffect(() => {
    let cancelled = false;
    const loadTeams = async (): Promise<void> => {
      try {
        setLoading(true);
        setError(null);
        const [teamList, clubs] = await Promise.all([hockeyTeamService.getAll(), loadClubNameMap()]);
        if (cancelled) {
          return;
        }
        setTeams(teamList);
        setClubNames(clubs);
      } catch (err) {
        if (!cancelled) {
          setTeams([]);
          setError(err instanceof Error ? err.message : t('hockey.teams.errors.loadFailed', 'Failed to fetch teams'));
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };
    void loadTeams();
    return () => {
      cancelled = true;
    };
  }, [reloadToken, t]);

  const filtered = useMemo(() => {
    const needle = debouncedSearch.toLowerCase();
    return teams.filter((team) => {
      const categoryMatch = categoryFilter.length === 0 || categoryFilter.includes(team.teamCategory);
      if (!categoryMatch) {
        return false;
      }
      if (!needle) {
        return true;
      }
      return `${team.name} ${clubNames.get(team.clubId) ?? ''}`.toLowerCase().includes(needle);
    });
  }, [teams, debouncedSearch, categoryFilter, clubNames]);

  const totalCount = filtered.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const pagedTeams = filtered.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  useEffect(() => {
    if (currentPage > totalPages) {
      setCurrentPage(totalPages);
    }
  }, [currentPage, totalPages]);

  const handleDelete = async (teamId: string, teamName: string): Promise<void> => {
    if (!window.confirm(t('hockey.teams.confirmDeactivate', 'Deactivate team "{{name}}"?', { name: teamName }))) {
      return;
    }
    try {
      await hockeyTeamService.setActive(teamId, false);
      setSelectedIds((prev) => {
        const updated = new Set(prev);
        updated.delete(teamId);
        return updated;
      });
      setReloadToken((token) => token + 1);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to deactivate team');
    }
  };

  const handleBulkDelete = async (): Promise<void> => {
    if (selectedIds.size === 0) {
      return;
    }
    if (!window.confirm(t('hockey.teams.confirmBulkDeactivate', 'Deactivate {{count}} teams?', { count: selectedIds.size }))) {
      return;
    }
    try {
      for (const id of selectedIds) {
        await hockeyTeamService.setActive(id, false);
      }
      setSelectedIds(new Set());
      setReloadToken((token) => token + 1);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to deactivate teams');
    }
  };

  const toggleSelect = (id: string): void => {
    setSelectedIds((prev) => {
      const updated = new Set(prev);
      if (updated.has(id)) {
        updated.delete(id);
      } else {
        updated.add(id);
      }
      return updated;
    });
  };

  return (
    <PageTemplate title={t('hockey.teams.title', 'Manage Teams')}>
      <div className="floorball-teams-container">
        <h2 className="floorball-teams-title">{t('hockey.teams.title', 'MANAGE TEAMS')}</h2>
        <div className="floorball-teams-header">
          <div className="teams-count">
            <span>{t('hockey.teams.totalCount', { count: totalCount, defaultValue: 'Total: {{count}} teams' })}</span>
          </div>
          <div className="teams-actions">
            <button className="create-team-button" onClick={() => navigate('/admin/hockey/teams/new')}>
              {t('hockey.teams.createNew', 'Create New Team')}
            </button>
          </div>
        </div>
        <div className="teams-search-bar">
          <input
            type="text"
            value={searchTerm}
            onChange={(event) => setSearchTerm(event.target.value)}
            placeholder={t('hockey.teams.searchPlaceholder', 'Search teams by name...')}
            className="teams-search-input"
          />
        </div>
        <div className="teams-category-filter">
          <TeamCategoryFilter
            selected={categoryFilter}
            onChange={(categories) => {
              setCategoryFilter(categories as HockeyTeamCategory[]);
              setCurrentPage(1);
            }}
          />
        </div>
        <ErrorPopup message={error} />
        <TeamsTable
          teams={pagedTeams}
          clubNames={clubNames}
          onEdit={(teamId) => navigate(`/admin/hockey/teams/${teamId}/edit`)}
          onEditRoster={(teamId) => navigate(`/admin/hockey/teams/${teamId}/roster`)}
          onEditLines={(teamId) => navigate(`/admin/hockey/teams/${teamId}/lines`)}
          onDelete={(teamId, teamName) => void handleDelete(teamId, teamName)}
          loading={loading}
          selectedIds={selectedIds}
          onToggleSelect={toggleSelect}
          onSelectAll={() => setSelectedIds(new Set(pagedTeams.map((team) => team.id)))}
          onClearSelection={() => setSelectedIds(new Set())}
          onBulkDelete={() => void handleBulkDelete()}
          pagination={{
            currentPage,
            totalPages,
            totalCount,
            pageSize,
          }}
          onPageChange={setCurrentPage}
          onPageSizeChange={(next) => {
            setPageSize(next);
            setCurrentPage(1);
          }}
        />
      </div>
    </PageTemplate>
  );
}

export default HockeyTeamsPage;
