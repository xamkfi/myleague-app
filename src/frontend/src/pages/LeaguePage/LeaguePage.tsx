import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import StandingsSection from './components/StandingsSection';
import ResultsSection from './components/ResultsSection';
import FixturesSection from './components/FixturesSection';
import SummarySection from './components/SummarySection';
import NewsSection from './components/NewsSection';
import { floorballStatisticsService, type FloorballSeasonStatisticsSummaryDto } from '../../api/floorball/floorballStatistics';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import './LeaguePage.scss';

type TabType = 'summary' | 'news' | 'results' | 'fixtures' | 'standings';

export default function LeaguePage() {
  const { id } = useParams<{ id: string }>();
  const [activeTab, setActiveTab] = useState<TabType>('summary');
  
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
        setError(err instanceof Error ? err.message : 'Failed to load league data');
      } finally {
        setLoading(false);
      }
    };

    fetchSeasonData();
  }, [id]);

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
        setMatchesError(err instanceof Error ? err.message : 'Failed to load matches');
      } finally {
        setMatchesLoading(false);
      }
    };

    fetchMatchesData();
  }, [id, currentPage]);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const tabs: { key: TabType; label: string }[] = [
    { key: 'standings', label: 'Standings' },
    { key: 'summary', label: 'Summary' },
    { key: 'news', label: 'News' },
    { key: 'results', label: 'Results' },
    { key: 'fixtures', label: 'Fixtures' }
  ];

  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return <SummarySection />;
      case 'news':
        return <NewsSection />;
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
          <StandingsSection 
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
    <PageTemplate title={`League ${id}`}>
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
                <h1 className="league-title">LEAGUE</h1>
                <div className="league-details">
                  <div className="detail-item">
                    <span className="detail-icon">🕐</span>
                    <span>2025/2026</span>
                  </div>
                  <div className="detail-item">
                    <span className="detail-icon">📍</span>
                    <span>Location</span>
                  </div>
                </div>
                <div className="league-tabs">
                  {tabs.map((tab) => (
                    <button
                      key={tab.key}
                      className={`tab-button ${activeTab === tab.key ? 'active' : ''}`}
                      onClick={() => setActiveTab(tab.key)}
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
