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
import Navbar from '../../../../components/Navigation/Navbar';

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
  const [error, setError] = useState<string | null>(null);
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        // Load seasons
        const seasonsResp = await floorballSeasonService.getAll();
        if (seasonsResp.success && seasonsResp.data) {
          setSeasons(seasonsResp.data);
        }
        // Batch-fetch all matches (backend limits pageSize <= 100)
        const batchSize = 100;
        let page = 1;
        let allMatches: FloorballMatchDto[] = [];
        let hasNext = true;
        while (hasNext) {
          const resp = await floorballMatchService.getAll({ page, pageSize: batchSize });
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
      }
    };
    fetchData();
  }, []);

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
  };

  const handleLiveMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  // Filter by status and optionally season
  const filtered = useMemo(() => {
    const base = selectedSeasonId
      ? matches.filter(m => m.seasonId === selectedSeasonId)
      : matches;

    const result = base.filter(m => m.status === status);

    if (status === 'Scheduled') {
      result.sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime());
    } else {
      result.sort((a, b) => new Date(b.scheduledDateTime).getTime() - new Date(a.scheduledDateTime).getTime());
    }

    return result;
  }, [matches, selectedSeasonId, status]);

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
      <Navbar />
      <div className="match-management__content matches-by-status-page">
        <BackButton to="/admin/floorball/matches" text={t('common.back', 'Back to Match Management')} />
        <h1>{title}</h1>
        <MatchFilters
          seasons={seasons}
          selectedSeasonId={selectedSeasonId}
          onSeasonChange={setSelectedSeasonId}
        />
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
  );
};

export default MatchesByStatusPage;