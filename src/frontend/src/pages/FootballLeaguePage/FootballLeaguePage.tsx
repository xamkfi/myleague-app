import { useState, useEffect, useCallback } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import FootballLeagueStanding from './components/FootballLeagueStanding';
import ResultsSection from './components/ResultsSection';
import FixturesSection from './components/FixturesSection';
import SummarySection from './components/SummarySection';
import SeasonInfoCards from '../../components/SeasonInfoCards/SeasonInfoCards';
import CompetitionHero from '../../components/CompetitionHero/CompetitionHero';
import UnderlineTabs from '../../components/UnderlineTabs/UnderlineTabs';
import { isNotFoundError } from '../../api/utils/isNotFoundError';
import { unwrapApiErrorMessage } from '../../api/utils/ParseErrorResponse';
import { footballSeasonService, type FootballSeasonDto } from '../../api/football/footballSeasonService';
import type { SeasonContentBlockDto } from '../../types/common/seasonContent';
import { footballStatisticsService, type FootballSeasonStatisticsSummaryDto } from '../../api/football/footballStatistics';
import { footballMatchService } from '../../api/football/footballMatchService';
import { type FootballMatchDto, FootballMatchStatus } from '../../types/football/footballTypes';
import './FootballLeaguePage.scss';

type TabType = 'summary' | 'news' | 'results' | 'fixtures' | 'statistics';

const VALID_TABS: TabType[] = ['summary', 'news', 'results', 'fixtures', 'statistics'];

function getStatusForTab(tab: TabType): FootballMatchStatus | undefined {
  if (tab === 'results') return FootballMatchStatus.Completed;
  return undefined;
}

function getSortOrderForTab(tab: TabType): string {
  return tab === 'results' ? 'desc' : 'asc';
}

export default function FootballLeaguePage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  
  const getInitialTab = (): TabType => {
    const tabParam = searchParams.get('tab');
    if (tabParam && VALID_TABS.includes(tabParam as TabType)) {
      return tabParam as TabType;
    }
    if (tabParam === 'standings') {
      return 'statistics';
    }
    return 'summary';
  };
  
  const [activeTab, setActiveTab] = useState<TabType>(getInitialTab);
  
  useEffect(() => {
    const tabParam = searchParams.get('tab');
    if (tabParam && VALID_TABS.includes(tabParam as TabType)) {
      setActiveTab(tabParam as TabType);
    }
  }, [searchParams]);
  
  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    setSearchParams({ tab });
    setCurrentPage(1);
  };
  
  // State for season statistics data
  const [seasonSummary, setSeasonSummary] = useState<FootballSeasonStatisticsSummaryDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [contentBlocks, setContentBlocks] = useState<SeasonContentBlockDto[]>([]);
  const [season, setSeason] = useState<FootballSeasonDto | null>(null);

  // State for matches data
  const [matches, setMatches] = useState<FootballMatchDto[] | null>(null);
  const [matchesLoading, setMatchesLoading] = useState(false);
  const [matchesError, setMatchesError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // Fetch season statistics data
  useEffect(() => {
    const fetchSeasonData = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        setError(null);
        const data = await footballStatisticsService.getSeasonStatistics(id);
        setSeasonSummary(data);
      } catch (err) {
        console.error('Failed to fetch season statistics:', err);
        if (isNotFoundError(err)) {
          setSeasonSummary(null);
          setError(null);
        } else {
          setError(unwrapApiErrorMessage(err, t('leaguePage.errors.loadLeagueData')));
        }
      } finally {
        setLoading(false);
      }
    };

    fetchSeasonData();
  }, [id, t]);

  useEffect(() => {
    if (!id) {
      setSeason(null);
      return;
    }

    let cancelled = false;
    footballSeasonService
      .getById(id)
      .then((result) => {
        if (!cancelled) {
          setSeason(result.data ?? null);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setSeason(null);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  useEffect(() => {
    if (!id) {
      setContentBlocks([]);
      return;
    }

    let cancelled = false;
    footballSeasonService
      .getContentBlocks(id)
      .then((result) => {
        if (!cancelled) {
          setContentBlocks(result.blocks);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setContentBlocks([]);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  // Fetch matches data - filtered by tab status
  useEffect(() => {
    const fetchMatchesData = async () => {
      if (!id) return;
      if (activeTab !== 'fixtures' && activeTab !== 'results') return;
      
      try {
        setMatchesLoading(true);
        setMatchesError(null);
        
        const pageSize = activeTab === 'fixtures' ? 20 : 10;
        
        const response = await footballMatchService.getAll({
          competitionId: id,
          page: currentPage,
          pageSize,
          sortOrder: getSortOrderForTab(activeTab),
          status: getStatusForTab(activeTab),
        });

        setMatches(response.data || []);
        setTotalPages(response.pagination.totalPages || 1);
      } catch (err) {
        console.error('Failed to fetch matches:', err);
        setMatchesError(err instanceof Error ? err.message : t('leaguePage.errors.loadMatches'));
      } finally {
        setMatchesLoading(false);
      }
    };

    fetchMatchesData();
  }, [id, currentPage, activeTab, t]);

  const handlePageChange = useCallback((page: number) => {
    setCurrentPage(page);
  }, []);

  const tabs: { key: TabType; label: string }[] = [
    { key: 'summary', label: t('leaguePage.tabs.summary') },
    { key: 'statistics', label: t('leaguePage.tabs.statistics') },
    { key: 'results', label: t('leaguePage.tabs.results') },
    { key: 'fixtures', label: t('leaguePage.tabs.fixtures') }
  ];

  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <>
            <SeasonInfoCards blocks={contentBlocks} className="season-info-cards" />
            <SummarySection 
              seasonSummary={seasonSummary}
              loading={loading}
              error={error}
            />
          </>
        );
      case 'results':
        return (
          <ResultsSection 
            matches={matches}
            matchesLoading={matchesLoading}
            matchesError={matchesError}
            currentPage={currentPage}
            totalPages={totalPages}
            handlePageChange={handlePageChange}
          />
        );
      case 'fixtures':
        return (
          <FixturesSection 
            matches={matches}
            matchesLoading={matchesLoading}
            matchesError={matchesError}
            currentPage={currentPage}
            totalPages={totalPages}
            handlePageChange={handlePageChange}
          />
        );
      case 'statistics':
        return (
          <FootballLeagueStanding 
            seasonSummary={seasonSummary}
            loading={loading}
            error={error}
          />
        );
      default:
        return null;
    }
  };

  return (
    <PageTemplate title={season?.name || seasonSummary?.seasonName || t('leaguePage.defaultTitle')}>
      <div className="league-page">
        <CompetitionHero
          title={season?.name || seasonSummary?.seasonName || t('leaguePage.defaultTitle')}
          logoUrl={season?.logoUrl}
        />
        <UnderlineTabs
          tabs={tabs.map((tab) => ({ id: tab.key, label: tab.label }))}
          activeId={activeTab}
          onChange={(id) => handleTabChange(id as TabType)}
          ariaLabel={t('leaguePage.defaultTitle')}
        />
        
        <div className="league-content">
          {renderTabContent()}
        </div>
      </div>
    </PageTemplate>
  );
}
