import { useState, useEffect } from 'react';
import { getMatchesService, type FloorballMatch } from '../../../../api/admin/News/GetMatchesService';
import '../styles/MatchBrowser.scss';

interface MatchBrowserProps {
  onInsertMatches: (matches: FloorballMatch[]) => void;
}

type MatchCategory = 'all' | 'scheduled' | 'results' | 'cancelled';

export default function MatchBrowser({ onInsertMatches }: MatchBrowserProps) {
  const [showBrowser, setShowBrowser] = useState(false);
  const [selectedMatches, setSelectedMatches] = useState<FloorballMatch[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [matches, setMatches] = useState<FloorballMatch[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<MatchCategory>('all');

  // Fetch matches when browser is opened
  useEffect(() => {
    if (showBrowser && matches.length === 0) {
      fetchMatches();
    }
  }, [showBrowser, matches.length]);

  const fetchMatches = async () => {
    setLoading(true);
    setError(null);
    try {
      const fetchedMatches = await getMatchesService.getAll();
      setMatches(fetchedMatches);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch matches');
      console.error('Error fetching matches:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleMatchSelect = (match: FloorballMatch) => {
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

  const handleRefresh = () => {
    fetchMatches();
  };

  // Filter matches by search term and category
  const getFilteredMatches = () => {
    const filtered = matches.filter(match => 
      match.homeTeamName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      match.awayTeamName.toLowerCase().includes(searchTerm.toLowerCase())
    );

    switch (selectedCategory) {
      case 'scheduled':
        return filtered.filter(match => match.status.toLowerCase() === 'scheduled');
      case 'results':
        return filtered.filter(match => match.status.toLowerCase() === 'completed');
      case 'cancelled':
        return filtered.filter(match => match.status.toLowerCase() === 'cancelled');
      default:
        return filtered;
    }
  };

  const filteredMatches = getFilteredMatches();

  const getCategoryButtonClass = (category: MatchCategory) => {
    return `match-browser__category-btn ${selectedCategory === category ? 'match-browser__category-btn--active' : ''}`;
  };

  const getCategoryTitle = () => {
    switch (selectedCategory) {
      case 'scheduled':
        return 'Aikataulutetut ottelut';
      case 'results':
        return 'Päättyneet ottelut';
      case 'cancelled':
        return 'Perutut ottelut';
      default:
        return 'Kaikki ottelut';
    }
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
              <div className="match-browser-modal__header-actions">
                <button onClick={handleRefresh} disabled={loading}>
                  {loading ? 'Ladataan...' : 'Päivitä'}
                </button>
                <button onClick={() => setShowBrowser(false)}>×</button>
              </div>
            </div>

            <div className="match-browser-modal__body">
              {/* Error message */}
              {error && (
                <div className="match-browser__error">
                  <p>Virhe: {error}</p>
                  <button onClick={fetchMatches}>Yritä uudelleen</button>
                </div>
              )}

              {/* Search */}
              <div className="match-browser__search">
                <input
                  type="text"
                  placeholder="Etsi joukkueita..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>

              {/* Category selection */}
              <div className="match-browser__categories">
                <button 
                  className={getCategoryButtonClass('all')}
                  onClick={() => setSelectedCategory('all')}
                >
                  All
                </button>
                <button 
                  className={getCategoryButtonClass('scheduled')}
                  onClick={() => setSelectedCategory('scheduled')}
                >
                  Upcoming
                </button>
                <button 
                  className={getCategoryButtonClass('results')}
                  onClick={() => setSelectedCategory('results')}
                >
                  Results
                </button>
                <button 
                  className={getCategoryButtonClass('cancelled')}
                  onClick={() => setSelectedCategory('cancelled')}
                >
                  Cancelled
                </button>
              </div>

              {/* Selected matches */}
              {selectedMatches.length > 0 && (
                <div className="match-browser__selected">
                  <h4>Valitut ottelut ({selectedMatches.length})</h4>
                  <div className="match-browser__selected-list">
                    {selectedMatches.map(match => (
                      <span key={match.id} className="match-browser__selected-item">
                        {match.homeTeamName} vs {match.awayTeamName}
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

              {/* Loading state */}
              {loading && (
                <div className="match-browser__loading">
                  <p>Ladataan otteluita...</p>
                </div>
              )}

              {/* Matches list */}
              {!loading && !error && (
                <div className="match-browser__list">
                  <div className="match-browser__list-header">
                    <h4>{getCategoryTitle()} ({filteredMatches.length})</h4>
                  </div>
                  {filteredMatches.length === 0 ? (
                    <div className="match-browser__empty">
                      <p>Ei otteluita saatavilla</p>
                    </div>
                  ) : (
                    filteredMatches.map(match => (
                      <div key={match.id} className="match-browser__item">
                        <label>
                          <input
                            type="checkbox"
                            checked={selectedMatches.some(m => m.id === match.id)}
                            onChange={() => handleMatchSelect(match)}
                          />
                          <span className="match-browser__date">{match.scheduledDateTime}</span>
                          <span className="match-browser__teams">
                            {match.homeTeamName} {match.homeScore} - {match.awayScore} {match.awayTeamName}
                          </span>
                        </label>
                      </div>
                    ))
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
} 