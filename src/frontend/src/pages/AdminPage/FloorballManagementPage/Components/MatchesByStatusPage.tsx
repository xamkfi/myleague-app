import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import PaginationControls from '../FloorballTeamsPage/components/PaginationControls';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import BackButton from '../../../../components/BackButton/BackButton';
import MatchFilters from '../MatchOverviewPage/Components/MatchFilters/MatchFilters';
import CollapsibleMatchSection from '../MatchOverviewPage/Components/CollapsibleMatchSection/CollapsibleMatchSection';

import './MatchesByStatusPage.scss';
import '../MatchOverviewPage/MatchOverviewPage.scss';

interface MatchesByStatusPageProps {
  status: FloorballMatchDto['status'];
  title: string;
  sectionType: 'ongoing' | 'scheduled' | 'completed' | 'cancelled';
}

const MatchesByStatusPage = ({ status, title, sectionType }: MatchesByStatusPageProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  // Data state
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFiltering, setIsFiltering] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    const fetchData = async (isInitialLoad: boolean) => {
      try {
        // Only show full loading on initial load
        if (isInitialLoad) {
          setLoading(true);
        } else {
          setIsFiltering(true);
        }
        // Load seasons
        const seasonsResp = await floorballSeasonService.getAll();
        if (seasonsResp.success && seasonsResp.data) {
          setSeasons(seasonsResp.data);
        }
        // Fetch all matches with filters (backend will handle season + search)
        const batchSize = 100;
        let page = 1;
        let allMatches: FloorballMatchDto[] = [];
        let hasNext = true;
        while (hasNext) {
          const resp = await floorballMatchService.getAll({ 
            page, 
            pageSize: batchSize,
            seasonId: selectedSeasonId || undefined,
            searchQuery: searchQuery.trim() || undefined
          });
          if (resp.success && resp.data) {
            allMatches = allMatches.concat(resp.data);
            hasNext = resp.pagination?.hasNextPage ?? false;
            page += 1;
          } else {
            hasNext = false;
          }
        }
        setMatches(allMatches);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch data');
      } finally {
        setLoading(false);
        setIsFiltering(false);
      }
    };

    // Initial load or filter change
    if (loading) {
      // Initial load - no debounce
      fetchData(true);
    } else {
      // Filter change - debounce 500ms
      const timer = setTimeout(() => {
        fetchData(false);
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [selectedSeasonId, searchQuery, loading]);

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
  };

  const handleLiveMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  // Filter by status (backend already filtered by season and search)
  const filtered = useMemo(() => {
    // Backend already filtered by season and search query
    const result = matches.filter(m => m.status === status);

    if (status === 'Scheduled') {
      result.sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime());
    } else {
      result.sort((a, b) => new Date(b.scheduledDateTime).getTime() - new Date(a.scheduledDateTime).getTime());
    }

    return result;
  }, [matches, status]);

  const totalPages = Math.ceil(filtered.length / pageSize) || 1;
  const paginated = useMemo(
    () => filtered.slice((currentPage - 1) * pageSize, currentPage * pageSize),
    [filtered, currentPage, pageSize]
  );

  const toggleCollapse = () => setCollapsed(prev => !prev);
  // navigation replaced by modal handlers

  if (loading) return <p>{t('matches.loading', 'Loading matches…')}</p>;
  if (error) return <p>{t('common.error', 'Error')}: {error}</p>;

  return (
    <div className="match-management">
      <div className="match-management__content matches-by-status-page">
        <BackButton to="/admin/floorball/matches" text={t('common.back', 'Back to Match Management')} />
        <h1>{title}</h1>
        <MatchFilters
          seasons={seasons}
          selectedSeasonId={selectedSeasonId}
          onSeasonChange={setSelectedSeasonId}
          searchQuery={searchQuery}
          onSearchChange={setSearchQuery}
        />
        
        {/* Filtering indicator */}
        {isFiltering && (
          <div style={{ 
            padding: '12px', 
            textAlign: 'center', 
            background: '#f3f4f6', 
            borderRadius: '8px',
            marginBottom: '16px',
            color: '#6b7280',
            fontSize: '0.875rem'
          }}>
            <span style={{ marginRight: '8px' }}>🔍</span>
            Searching...
          </div>
        )}
        
        <div style={{ opacity: isFiltering ? 0.6 : 1, transition: 'opacity 0.2s' }}>
        {!filtered.length ? (
          <p>{t('matches.noMatches', 'No matches found.')}</p>
        ) : (
          <>
            <CollapsibleMatchSection
              title={`${title} (${filtered.length})`}
              matches={paginated}
              isCollapsed={collapsed}
              onToggleCollapse={toggleCollapse}
              onLiveMatch={handleLiveMatch}
              onEditMatch={handleEditMatch}
              sectionType={sectionType}
            />
            <PaginationControls
              currentPage={currentPage}
              totalPages={totalPages}
              totalCount={filtered.length}
              pageSize={pageSize}
              onPageChange={page => setCurrentPage(page)}
              onPageSizeChange={size => { setPageSize(size); setCurrentPage(1); }}
            />
          </>
        )}
        </div>
      </div>
    </div>
  );
};

export default MatchesByStatusPage;