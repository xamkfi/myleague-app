import { useState, useEffect, useMemo, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballMatchEventService } from '../../../../api/floorball/floorballMatchEventService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import PaginationControls from '../FloorballPlayersPage/components/PaginationControls';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import MatchFilters from '../MatchOverviewPage/Components/MatchFilters/MatchFilters';
import CollapsibleMatchSection from '../MatchOverviewPage/Components/CollapsibleMatchSection/CollapsibleMatchSection';
import ConfirmationDialog from '../ManageMatchPage/components/ConfirmationDialog';
import LoadingSpinner from '../../../../components/LoadingSpinner/LoadingSpinner';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';

import './MatchesByStatusPage.scss';

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

  const fetchData = useCallback(async (isInitialLoad: boolean) => {
    try {
      if (isInitialLoad) {
        setLoading(true);
      } else {
        setIsFiltering(true);
      }
      setError(null);

      const seasonsResp = await floorballSeasonService.getAll();
      if (seasonsResp.success && seasonsResp.data) {
        setSeasons(seasonsResp.data);
      }

      const batchSize = 100;
      let page = 1;
      let allMatches: FloorballMatchDto[] = [];
      let hasNext = true;
      while (hasNext) {
        const resp = await floorballMatchService.getAll({
          page,
          pageSize: batchSize,
          seasonId: selectedSeasonId || undefined,
          searchQuery: searchQuery.trim() || undefined,
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
  }, [selectedSeasonId, searchQuery]);

  // Initial load
  useEffect(() => {
    fetchData(true);
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Debounced filter changes
  useEffect(() => {
    if (loading) return;
    const timer = setTimeout(() => {
      fetchData(false);
    }, 500);
    return () => clearTimeout(timer);
  }, [selectedSeasonId, searchQuery]); // eslint-disable-line react-hooks/exhaustive-deps

  const [confirmDialog, setConfirmDialog] = useState<{
    type: 'cancel' | 'reactivate';
    match: FloorballMatchDto;
  } | null>(null);
  const [dialogLoading, setDialogLoading] = useState(false);

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
  };

  const handleLiveMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  const handleCancelMatch = (match: FloorballMatchDto) => {
    setConfirmDialog({ type: 'cancel', match });
  };

  const handleReactivateMatch = (match: FloorballMatchDto) => {
    setConfirmDialog({ type: 'reactivate', match });
  };

  const handleConfirmAction = async () => {
    if (!confirmDialog) return;
    try {
      setDialogLoading(true);
      if (confirmDialog.type === 'cancel') {
        await floorballMatchEventService.cancelMatch(confirmDialog.match.id);
      } else {
        await floorballMatchEventService.reactivateMatch(confirmDialog.match.id);
      }
      setConfirmDialog(null);
      fetchData(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setDialogLoading(false);
    }
  };

  const filtered = useMemo(() => {
    const result = matches.filter((m) => m.status === status);
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

  if (loading) {
    return (
      <div className="mbs-page">
        <LoadingSpinner text={t('floorball.matches.loading', 'Loading matches...')} />
      </div>
    );
  }

  return (
    <div className="mbs-page">
      <div className="mbs-page__header">
        <h1 className="mbs-page__title">{title}</h1>
        <span className="mbs-page__count">
          {filtered.length} {t('floorball.matches.matchCount', 'match(es)')}
        </span>
      </div>

      <ErrorPopup message={error} />

      <MatchFilters
        seasons={seasons}
        selectedSeasonId={selectedSeasonId}
        onSeasonChange={setSelectedSeasonId}
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
      />

      {isFiltering && (
        <div className="mbs-page__filtering">
          <i className="fas fa-spinner fa-spin"></i>
          <span>{t('common.searching', 'Searching...')}</span>
        </div>
      )}

      <div className={`mbs-page__content ${isFiltering ? 'mbs-page__content--dimmed' : ''}`}>
        {filtered.length === 0 ? (
          <div className="mbs-page__empty">
            <i className="fas fa-calendar-times"></i>
            <p>{t('floorball.matches.noMatches', 'No matches found.')}</p>
          </div>
        ) : (
          <>
            <CollapsibleMatchSection
              title={`${title} (${filtered.length})`}
              matches={paginated}
              isCollapsed={collapsed}
              onToggleCollapse={() => setCollapsed((prev) => !prev)}
              onLiveMatch={handleLiveMatch}
              onEditMatch={handleEditMatch}
              onCancelMatch={handleCancelMatch}
              onReactivateMatch={handleReactivateMatch}
              sectionType={sectionType}
            />
            <PaginationControls
              currentPage={currentPage}
              totalPages={totalPages}
              totalCount={filtered.length}
              pageSize={pageSize}
              onPageChange={(page) => setCurrentPage(page)}
              onPageSizeChange={(size) => { setPageSize(size); setCurrentPage(1); }}
            />
          </>
        )}
      </div>

      <ConfirmationDialog
        isOpen={confirmDialog !== null}
        icon={confirmDialog?.type === 'cancel' ? '⚠️' : '✅'}
        title={
          confirmDialog?.type === 'cancel'
            ? t('floorball.matches.confirmCancel.title', 'Cancel Match')
            : t('floorball.matches.confirmReactivate.title', 'Reactivate Match')
        }
        message={
          confirmDialog?.type === 'cancel'
            ? t('floorball.matches.confirmCancel.message', 'Are you sure you want to cancel this match? This will mark the match as cancelled.')
            : t('floorball.matches.confirmReactivate.message', 'Are you sure you want to reactivate this match? This will set the match back to Scheduled status.')
        }
        confirmText={
          confirmDialog?.type === 'cancel'
            ? t('floorball.matches.confirmCancel.confirm', 'Yes, Cancel Match')
            : t('floorball.matches.confirmReactivate.confirm', 'Yes, Reactivate Match')
        }
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={dialogLoading}
        onConfirm={handleConfirmAction}
        onCancel={() => setConfirmDialog(null)}
      />
    </div>
  );
};

export default MatchesByStatusPage;
