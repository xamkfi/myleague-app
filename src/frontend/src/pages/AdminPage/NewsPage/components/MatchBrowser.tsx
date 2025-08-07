import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { getMatchesService, type FloorballMatch } from '../../../../api/admin/News/GetMatchesService';
import '../styles/MatchBrowser.scss';

interface MatchBrowserProps {
  onInsertMatches: (matches: FloorballMatch[]) => void;
}

type MatchCategory = 'all' | 'scheduled' | 'results' | 'cancelled';

export default function MatchBrowser({ onInsertMatches }: MatchBrowserProps) {
  const { t } = useTranslation();
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
        return t('admin.news.matches.upcoming_matches', 'Upcoming matches');
      case 'results':
        return t('admin.news.matches.results', 'Results');
      case 'cancelled':
        return t('admin.news.matches.cancelled', 'Cancelled');
      default:
        return t('admin.news.matches.all_matches', 'All matches');
    }
  };

  return (
    <>
      <button 
        className="match-browser__trigger-btn"
        onClick={() => setShowBrowser(true)}
      >
        {t('admin.news.matches.add_matches_selected')}
      </button>

      {showBrowser && (
        <div className="match-browser-modal">
          <div className="match-browser-modal__backdrop" onClick={() => setShowBrowser(false)} />
          <div className="match-browser-modal__content">
            <div className="match-browser-modal__header">
              <h2>{t('admin.news.matches.add_matches', 'ADD MATCHES')}</h2>
              <button 
                className="match-browser-modal__close-btn"
                onClick={() => setShowBrowser(false)}
              >
                ×
              </button>
            </div>

            <div className="match-browser-modal__body">
              {/* Error message */}
              {error && (
                <div className="match-browser__error">
                  <p>{t('admin.news.matches.error', 'Error')}: {error}</p>
                  <button onClick={fetchMatches}>{t('admin.news.matches.try_again', 'Try again')}</button>
                </div>
              )}

              {/* Category filter buttons */}
              <div className="match-browser__categories">
                <button 
                  className={getCategoryButtonClass('all')}
                  onClick={() => setSelectedCategory('all')}
                >
                  {t('admin.news.matches.all_matches', 'All')}
                </button>
                <button 
                  className={getCategoryButtonClass('scheduled')}
                  onClick={() => setSelectedCategory('scheduled')}
                >
                  {t('admin.news.matches.upcoming_matches', 'Upcoming')}
                </button>
                <button 
                  className={getCategoryButtonClass('results')}
                  onClick={() => setSelectedCategory('results')}
                >
                  {t('admin.news.matches.results', 'Results')}
                </button>
                <button 
                  className={getCategoryButtonClass('cancelled')}
                  onClick={() => setSelectedCategory('cancelled')}
                >
                  {t('admin.news.matches.cancelled', 'Cancelled')}
                </button>
              </div>

              {/* Search bar */}
              <div className="match-browser__search">
                <div className="match-browser__search-icon">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <circle cx="11" cy="11" r="8"/>
                    <path d="m21 21-4.35-4.35"/>
                  </svg>
                </div>
                <input
                  type="text"
                  placeholder={t('admin.news.matches.search_matches', 'Search matches...')}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="match-browser__search-input"
                />
              </div>

              {/* Matches list */}
              {!loading && !error && (
                <div className="match-browser__list-section">
                  <h3 className="match-browser__list-title">{getCategoryTitle()}</h3>
                  <div className="match-browser__list">
                    {filteredMatches.length === 0 ? (
                      <div className="match-browser__empty">
                        <p>{t('admin.news.matches.no_matches_available', 'No matches available')}</p>
                      </div>
                    ) : (
                      filteredMatches.map(match => (
                        <div key={match.id} className="match-browser__item">
                          <div className="match-browser__item-content">
                            <div className="match-browser__item-date">
                              {new Date(match.scheduledDateTime).toLocaleDateString('en-GB')} {new Date(match.scheduledDateTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })}
                            </div>
                            <div className="match-browser__item-sport">
                              football
                            </div>
                            <div className="match-browser__item-teams">
                              {match.homeTeamName} {match.homeScore} - {match.awayScore} {match.awayTeamName}
                            </div>
                          </div>
                          <input
                            type="checkbox"
                            checked={selectedMatches.some(m => m.id === match.id)}
                            onChange={() => handleMatchSelect(match)}
                            className="match-browser__item-checkbox"
                          />
                        </div>
                      ))
                    )}
                  </div>
                </div>
              )}

              {/* Loading state */}
              {loading && (
                <div className="match-browser__loading">
                  <p>{t('admin.news.matches.loading_matches', 'Loading matches...')}</p>
                </div>
              )}

              {/* Selected matches chips */}
              {selectedMatches.length > 0 && (
                <div className="match-browser__selected">
                  <div className="match-browser__selected-chips">
                    {selectedMatches.map(match => (
                      <span key={match.id} className="match-browser__selected-chip">
                        {match.homeTeamName} vs {match.awayTeamName}
                        <button 
                          onClick={() => handleMatchSelect(match)}
                          className="match-browser__selected-chip-remove"
                        >
                          ×
                        </button>
                      </span>
                    ))}
                  </div>
                  <button 
                    onClick={clearSelection}
                    className="match-browser__clear-all"
                  >
                    {t('admin.news.matches.clear_all', 'Clear all')}
                  </button>
                </div>
              )}

              {/* Action buttons */}
              <div className="match-browser__actions">
                <button 
                  onClick={() => setShowBrowser(false)}
                  className="match-browser__cancel-btn"
                >
                  {t('admin.news.matches.cancel', 'Cancel')}
                </button>
                <button 
                  onClick={insertSelectedMatches}
                  disabled={selectedMatches.length === 0}
                  className="match-browser__add-btn"
                >
                  {t('admin.news.matches.add_selected', 'Add selected')}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
} 