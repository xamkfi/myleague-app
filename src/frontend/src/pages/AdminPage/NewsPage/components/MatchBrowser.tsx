import { useState } from 'react';
import mockMatches from './mockData.json';
import '../styles/MatchBrowser.scss';

interface MatchData {
  id: string;
  homeTeam: string;
  awayTeam: string;
  homeScore: string;
  awayScore: string;
  date: string;
  link: string;
}

interface MatchBrowserProps {
  onInsertMatches: (matches: MatchData[]) => void;
}

export default function MatchBrowser({ onInsertMatches }: MatchBrowserProps) {
  const [showBrowser, setShowBrowser] = useState(false);
  const [selectedMatches, setSelectedMatches] = useState<MatchData[]>([]);
  const [searchTerm, setSearchTerm] = useState('');

  const filteredMatches = mockMatches.matches.filter(match => 
    match.homeTeam.toLowerCase().includes(searchTerm.toLowerCase()) ||
    match.awayTeam.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleMatchSelect = (match: MatchData) => {
    setSelectedMatches(prev => {
      const exists = prev.find(m => m.id === match.id);
      if (exists) {
        return prev.filter(m => m.id !== match.id);
      }
      return [...prev, match];
    });
  };

  const insertSelectedMatches = () => {
    if (selectedMatches.length > 0) {
      onInsertMatches(selectedMatches);
      setSelectedMatches([]);
      setShowBrowser(false);
    }
  };

  const clearSelection = () => {
    setSelectedMatches([]);
  };

  return (
    <>
      <button onClick={() => setShowBrowser(true)}>
        Hae otteluita ({selectedMatches.length} valittu)
      </button>

      {showBrowser && (
        <div className="match-browser-modal">
          <div className="match-browser-modal__backdrop" onClick={() => setShowBrowser(false)} />
          <div className="match-browser-modal__content">
            <div className="match-browser-modal__header">
              <h3>Ottelujen selaus</h3>
              <button onClick={() => setShowBrowser(false)}>×</button>
            </div>

            <div className="match-browser-modal__body">
              {/* Search */}
              <div className="match-browser__search">
                <input
                  type="text"
                  placeholder="Etsi joukkueita..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>

              {/* Selected matches */}
              {selectedMatches.length > 0 && (
                <div className="match-browser__selected">
                  <h4>Valitut ottelut ({selectedMatches.length})</h4>
                  <div className="match-browser__selected-list">
                    {selectedMatches.map(match => (
                      <span key={match.id} className="match-browser__selected-item">
                        {match.homeTeam} vs {match.awayTeam}
                        <button onClick={() => handleMatchSelect(match)}>×</button>
                      </span>
                    ))}
                  </div>
                  <div className="match-browser__selected-actions">
                    <button onClick={insertSelectedMatches}>Lisää valitut</button>
                    <button onClick={clearSelection}>Tyhjennä</button>
                  </div>
                </div>
              )}

              {/* Matches list */}
              <div className="match-browser__list">
                {filteredMatches.map(match => (
                  <div key={match.id} className="match-browser__item">
                    <label>
                      <input
                        type="checkbox"
                        checked={selectedMatches.some(m => m.id === match.id)}
                        onChange={() => handleMatchSelect(match)}
                      />
                      <span className="match-browser__date">{match.date}</span>
                      <span className="match-browser__teams">
                        {match.homeTeam} {match.homeScore} - {match.awayScore} {match.awayTeam}
                      </span>
                    </label>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
} 