import React from 'react';
import Navbar from '../../components/Navigation/Navbar';
import HeroSection from '../../components/HeroSection/HeroSection';
import MatchSidebar from '../../components/MatchSidebar/MatchSidebar';
import './HomePage.css';

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

const HomePage: React.FC = () => {
  const handleLogin = () => {
    console.log('Login button clicked');
  };

  const handleExploreEvents = () => {
    console.log('Explore events button clicked');
  };

  return (
    <div className="home-page">
      <Navbar onLogin={handleLogin} />
      <div className="main-content">
        <div className="hero-container">
          <HeroSection 
            onButtonClick={handleExploreEvents}
          />
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
  );
};

export default HomePage; 