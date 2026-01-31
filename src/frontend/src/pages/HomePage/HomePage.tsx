import HomeNewsSection from '../../components/HomeNewsSection/HomeNewsSection';
import MatchSidebar from '../../components/MatchSidebar/MatchSidebar';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './HomePage.scss';

const mockStandings = {
  rows: [
    { position: 1, team: 'JOUKKUE 1', points: 37 },
    { position: 2, team: 'JOUKKUE 2', points: 32 },
    { position: 3, team: 'JOUKKUE 3', points: 30 }
  ]
};

const mockTeamStats = [
  { teamName: 'Joukkue 1', playerName: '', value: 4 },
  { teamName: 'Joukkue 2', playerName: '', value: 3 },
  { teamName: 'Joukkue 1', playerName: '', value: 4 },
  { teamName: 'Joukkue 2', playerName: '', value: 3 },
  { teamName: 'Joukkue 1', playerName: '', value: 4 },
  { teamName: 'Joukkue 2', playerName: '', value: 3 }
];

function HomePage() {
  return (
    <PageTemplate title="Home">
      <div className="home-page">
        <div className="main-content">
          <div className="news-section-container">
            <HomeNewsSection />
          </div>
          <div className="sidebar-container">
            <MatchSidebar
              match={{
                date: '12/6/2025',
                homeTeam: { name: 'Team 1' },
                awayTeam: { name: 'Team 2' }
              }}
              standings={mockStandings}
              teamStats={mockTeamStats}
            />
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}

export default HomePage; 