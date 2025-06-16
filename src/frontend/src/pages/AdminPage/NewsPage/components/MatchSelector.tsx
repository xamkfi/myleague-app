import { useState } from 'react';
import { mockMatches, type MatchData } from './matchData';
import type { MatchResultValue } from './MatchResultBlot';
import '../styles/MatchSelector.scss';

interface MatchSelectorProps {
  onMatchSelect: (matchData: MatchResultValue) => void;
}

export default function MatchSelector({ onMatchSelect }: MatchSelectorProps) {
  const [activeTab, setActiveTab] = useState<'finished' | 'upcoming'>('finished');
  const [searchTerm, setSearchTerm] = useState('');

  const finishedMatches = mockMatches.filter(match => match.status === 'finished');
  const upcomingMatches = mockMatches.filter(match => match.status === 'upcoming');

  const currentMatches = activeTab === 'finished' ? finishedMatches : upcomingMatches;
  
  const filteredMatches = currentMatches.filter(match => 
    match.homeTeam.toLowerCase().includes(searchTerm.toLowerCase()) ||
    match.awayTeam.toLowerCase().includes(searchTerm.toLowerCase()) ||
    match.competition.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const convertToBlotValue = (match: MatchData): MatchResultValue => {
    if (match.status === 'upcoming') {
      return {
        homeTeam: match.homeTeam,
        awayTeam: match.awayTeam,
        homeScore: match.kickoffTime || 'Tulossa',
        awayScore: '',
        date: match.date,
        link: match.link
      };
    } else {
      return {
        homeTeam: match.homeTeam,
        awayTeam: match.awayTeam,
        homeScore: match.homeScore || '0',
        awayScore: match.awayScore || '0',
        date: match.date,
        link: match.link
      };
    }
  };

  const handleMatchClick = (match: MatchData) => {
    const blotValue = convertToBlotValue(match);
    onMatchSelect(blotValue);
  };

  return (
    <div className="match-selector match-selector--modal">
      <div className="match-selector__header">
        {/* Search */}
        <div className="match-selector__search">
          <input
            type="text"
            placeholder="Search teams or competition..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="match-selector__search-input"
          />
        </div>

        {/* Tabs */}
        <div className="match-selector__tabs">
          <button
            className={`match-selector__tab ${activeTab === 'finished' ? 'match-selector__tab--active' : ''}`}
            onClick={() => setActiveTab('finished')}
          >
            Results ({finishedMatches.length})
          </button>
          <button
            className={`match-selector__tab ${activeTab === 'upcoming' ? 'match-selector__tab--active' : ''}`}
            onClick={() => setActiveTab('upcoming')}
          >
            Upcoming ({upcomingMatches.length})
          </button>
        </div>
      </div>

      <div className="match-selector__content">
        {filteredMatches.length === 0 ? (
          <div className="match-selector__empty">
            <p>No matches found</p>
          </div>
        ) : (
          <div className="match-selector__list">
            {filteredMatches.map((match) => (
              <div
                key={match.id}
                className="match-selector__item"
                onClick={() => handleMatchClick(match)}
              >
                <div className="match-selector__match-info">
                  <div className="match-selector__teams">
                    <span className="match-selector__team">{match.homeTeam}</span>
                    {match.status === 'finished' ? (
                      <span className="match-selector__score">
                        {match.homeScore} - {match.awayScore}
                      </span>
                    ) : (
                      <span className="match-selector__vs">vs</span>
                    )}
                    <span className="match-selector__team">{match.awayTeam}</span>
                  </div>
                  
                  <div className="match-selector__meta">
                    <span className="match-selector__date">
                      {match.date}
                      {match.kickoffTime && ` ${match.kickoffTime}`}
                    </span>
                    <span className="match-selector__competition">{match.competition}</span>
                  </div>
                  
                  <div className="match-selector__venue">{match.venue}</div>
                </div>

                <div className="match-selector__action">
                  <svg className="match-selector__add-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
} 