import { useState, useEffect } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LeagueStanding from '../../components/LeagueStanding/LeagueStanding';
import ResultsSection from './components/ResultsSection';
import FixturesSection from './components/FixturesSection';
import SummarySection from './components/SummarySection';
import { floorballStatisticsService, type FloorballSeasonStatisticsSummaryDto } from '../../api/floorball/floorballStatistics';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import './LeaguePage.scss';

type TabType = 'summary' | 'news' | 'results' | 'fixtures' | 'standings';

export default function LeaguePage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  
  // Get initial tab from URL params or default to 'summary'
  const getInitialTab = (): TabType => {
    const tabParam = searchParams.get('tab');
    const validTabs: TabType[] = ['summary', 'news', 'results', 'fixtures', 'standings'];
    if (tabParam && validTabs.includes(tabParam as TabType)) {
      return tabParam as TabType;
    }
    return 'summary';
  };
  
  const [activeTab, setActiveTab] = useState<TabType>(getInitialTab);
  
  // Update tab when URL params change
  useEffect(() => {
    const tabParam = searchParams.get('tab');
    const validTabs: TabType[] = ['summary', 'news', 'results', 'fixtures', 'standings'];
    if (tabParam && validTabs.includes(tabParam as TabType)) {
      setActiveTab(tabParam as TabType);
    }
  }, [searchParams]);
  
  // Update URL when tab changes
  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    setSearchParams({ tab });
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
        
        // Use the league ID from the URL as the season ID
        // The league ID (e.g., 63ed4ed5-1c56-47ce-8047-1e8f595575dc) will be used to fetch season data
        const seasonId = id;
        
        const data = await floorballStatisticsService.getSeasonStatistics(seasonId);
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

  // Fetch matches data
  useEffect(() => {
    const fetchMatchesData = async () => {
      if (!id) return;
      
      try {
        setMatchesLoading(true);
        setMatchesError(null);
        
        // Use the same API call pattern as FloorballTeamPage
        // For league page, we'll fetch all matches for the season/league
        const response = await floorballMatchService.getAll({
          seasonId: id, // Use league ID as season ID
          page: currentPage,
          pageSize: 10,
          sortOrder: 'asc'
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
  }, [id, currentPage, t]);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const tabs: { key: TabType; label: string }[] = [
    { key: 'summary', label: t('leaguePage.tabs.summary') },
    { key: 'standings', label: t('leaguePage.tabs.standings') },
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
      case 'standings':
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
