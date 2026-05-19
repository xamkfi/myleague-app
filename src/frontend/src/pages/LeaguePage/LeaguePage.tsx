import { useState, useEffect, useCallback } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LeagueStanding from '../../components/LeagueStanding/LeagueStanding';
import ResultsSection from './components/ResultsSection';
import FixturesSection from './components/FixturesSection';
import SummarySection from './components/SummarySection';
import { floorballStatisticsService, type FloorballSeasonStatisticsSummaryDto } from '../../api/floorball/floorballStatistics';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { type FloorballMatchDto, FloorballMatchStatus } from '../../types/floorball/floorballTypes';
import './LeaguePage.scss';

type TabType = 'summary' | 'news' | 'results' | 'fixtures' | 'statistics';

const VALID_TABS: TabType[] = ['summary', 'news', 'results', 'fixtures', 'statistics'];

function getStatusForTab(tab: TabType): FloorballMatchStatus | undefined {
  if (tab === 'results') return FloorballMatchStatus.Completed;
  return undefined;
}

function getSortOrderForTab(tab: TabType): string {
  return tab === 'results' ? 'desc' : 'asc';
}

export default function LeaguePage() {
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
  const [seasonSummary, setSeasonSummary] = useState<FloorballSeasonStatisticsSummaryDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // State for matches data
  const [matches, setMatches] = useState<FloorballMatchDto[] | null>(null);
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
        const data = await floorballStatisticsService.getSeasonStatistics(id);
        setSeasonSummary(data);
      } catch (err) {
        console.error('Failed to fetch season statistics:', err);
        setError(err instanceof Error ? err.message : t('leaguePage.errors.loadLeagueData'));
      } finally {
        setLoading(false);
      }
    };

    fetchSeasonData();
  }, [id, t]);

  // Fetch matches data - filtered by tab status
  useEffect(() => {
    const fetchMatchesData = async () => {
      if (!id) return;
      if (activeTab !== 'fixtures' && activeTab !== 'results') return;
      
      try {
        setMatchesLoading(true);
        setMatchesError(null);
        
        const pageSize = activeTab === 'fixtures' ? 20 : 10;
        
        const response = await floorballMatchService.getAll({
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
          <SummarySection 
            seasonSummary={seasonSummary}
            loading={loading}
            error={error}
          />
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
          <LeagueStanding 
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
    <PageTemplate title={id ? `${t('leaguePage.title')} ${id}` : t('leaguePage.defaultTitle')}>
      <div className="league-page">
        {/* Hero Image Background */}
        <div className="hero-image-container">
          <div className="hero-image"></div>
          
          {/* League Header */}
          <div className="league-header">
            <div className="header-content">
              <div className="league-branding">
                <div className="league-icon">
                  <div className="trophy-icon">🏆</div>
                </div>
              </div>

              <div className="league-info">
                <h1 className="league-title">{seasonSummary?.seasonName || t('leaguePage.defaultTitle')}</h1>
                <div className="league-tabs">
                  {tabs.map((tab) => (
                    <button
                      key={tab.key}
                      className={`tab-button ${activeTab === tab.key ? 'active' : ''}`}
                      onClick={() => handleTabChange(tab.key)}
                    >
                      {tab.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div className="league-content">
          {renderTabContent()}
        </div>
      </div>
    </PageTemplate>
  );
}
