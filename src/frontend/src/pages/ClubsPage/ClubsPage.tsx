import { useState, useEffect, useCallback, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { clubService, type Club } from '../../api/common/clubService';
import { createClubSlug } from '../../utils/slugUtils';
import './ClubsPage.scss';

const PAGE_SIZE = 12;

function ClubsPage() {
  const { t } = useTranslation();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);

  const fetchClubs = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const allClubs = await clubService.getAll();
      setClubs(allClubs);
    } catch (err) {
      console.error('Failed to fetch clubs:', err);
      setError(t('clubsPage.error'));
    } finally {
      setIsLoading(false);
    }
  }, [t]);

  useEffect(() => {
    fetchClubs();
  }, [fetchClubs]);

  const filteredClubs = useMemo(() => {
    if (searchQuery.trim() === '') return clubs;
    const query = searchQuery.toLowerCase();
    return clubs.filter(
      (club) =>
        club.name.toLowerCase().includes(query) ||
        club.city?.toLowerCase().includes(query)
    );
  }, [searchQuery, clubs]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchQuery]);

  const totalPages = Math.ceil(filteredClubs.length / PAGE_SIZE);
  const startIndex = (currentPage - 1) * PAGE_SIZE;
  const paginatedClubs = filteredClubs.slice(startIndex, startIndex + PAGE_SIZE);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <PageTemplate title={t('clubsPage.title')}>
      <div className="clubs-page">
        {/* Hero Banner */}
        <div className="clubs-page__hero">
          <div className="clubs-page__hero-overlay" />
          <div className="clubs-page__hero-content">
            <h1 className="clubs-page__title">{t('clubsPage.title')}</h1>
            <p className="clubs-page__description">{t('clubsPage.description')}</p>

            <div className="clubs-page__search">
              <div className="clubs-page__search-icon" aria-hidden="true">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="11" cy="11" r="8" />
                  <line x1="21" y1="21" x2="16.65" y2="16.65" />
                </svg>
              </div>
              <input
                type="text"
                placeholder={t('clubsPage.searchPlaceholder')}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="clubs-page__search-input"
              />
              {searchQuery && (
                <button
                  className="clubs-page__search-clear"
                  onClick={() => setSearchQuery('')}
                  aria-label="Clear search"
                >
                  &times;
                </button>
              )}
            </div>
          </div>
        </div>

        {/* Content */}
        <div className="clubs-page__content">
          {isLoading ? (
            <div className="clubs-page__loading">
              <LoadingSpinner text={t('clubsPage.loading')} />
            </div>
          ) : error ? (
            <div className="clubs-page__error">
              <p>{error}</p>
              <button onClick={fetchClubs} className="clubs-page__retry-btn">
                {t('common.retry')}
              </button>
            </div>
          ) : filteredClubs.length === 0 ? (
            <div className="clubs-page__empty">
              <p>{t('clubsPage.noClubs')}</p>
            </div>
          ) : (
            <>
              <div className="clubs-page__count">
                {t('clubsPage.totalClubs', { count: filteredClubs.length })}
              </div>

              <div className="clubs-page__grid">
                {paginatedClubs.map((club) => (
                  <Link
                    key={club.id}
                    to={`/club/${createClubSlug(club)}`}
                    className="club-card"
                  >
                    <div className="club-card__logo">
                      {club.logoUrl ? (
                        <img
                          src={club.logoUrl}
                          alt={`${club.name} logo`}
                          loading="lazy"
                          onError={(e) => {
                            (e.target as HTMLImageElement).style.display = 'none';
                            const parent = (e.target as HTMLImageElement).parentElement;
                            if (parent) {
                              const placeholder = document.createElement('span');
                              placeholder.className = 'club-card__logo-placeholder';
                              placeholder.textContent = club.name.charAt(0).toUpperCase();
                              parent.appendChild(placeholder);
                            }
                          }}
                        />
                      ) : (
                        <span className="club-card__logo-placeholder">
                          {club.name.charAt(0).toUpperCase()}
                        </span>
                      )}
                    </div>
                    <div className="club-card__content">
                      <h2 className="club-card__name">{club.name}</h2>
                      {club.city && (
                        <p className="club-card__city">{club.city}</p>
                      )}
                      <span className="club-card__link">
                        {t('clubsPage.viewClub')}
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <polyline points="9 18 15 12 9 6" />
                        </svg>
                      </span>
                    </div>
                  </Link>
                ))}
              </div>

              {totalPages > 1 && (
                <div className="clubs-page__pagination">
                  <button
                    className="pagination-btn"
                    onClick={() => handlePageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                  >
                    {t('common.previous')}
                  </button>
                  <span className="pagination-info">
                    {t('common.pageInfo', { current: currentPage, total: totalPages })}
                  </span>
                  <button
                    className="pagination-btn"
                    onClick={() => handlePageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                  >
                    {t('common.next')}
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default ClubsPage;
