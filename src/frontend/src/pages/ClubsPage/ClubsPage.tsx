import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { clubService, type Club } from '../../api/common/clubService';
import { createClubSlug } from '../../utils/slugUtils';
import './ClubsPage.scss';

const PAGE_SIZE = 12;

function ClubsPage() {
  const { t } = useTranslation();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [filteredClubs, setFilteredClubs] = useState<Club[]>([]);
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
      setFilteredClubs(allClubs);
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

  // Filter clubs based on search query
  useEffect(() => {
    if (searchQuery.trim() === '') {
      setFilteredClubs(clubs);
    } else {
      const query = searchQuery.toLowerCase();
      const filtered = clubs.filter(
        club =>
          club.name.toLowerCase().includes(query) ||
          club.city?.toLowerCase().includes(query)
      );
      setFilteredClubs(filtered);
    }
    setCurrentPage(1);
  }, [searchQuery, clubs]);

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(e.target.value);
  };

  // Pagination
  const totalPages = Math.ceil(filteredClubs.length / PAGE_SIZE);
  const startIndex = (currentPage - 1) * PAGE_SIZE;
  const paginatedClubs = filteredClubs.slice(startIndex, startIndex + PAGE_SIZE);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  if (isLoading) {
    return (
      <PageTemplate title={t('clubsPage.title')}>
        <div className="clubs-page">
          <div className="clubs-page__loading">
            <div className="loading-spinner"></div>
            <span>{t('clubsPage.loading')}</span>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={t('clubsPage.title')}>
        <div className="clubs-page">
          <div className="clubs-page__error">
            <p>{error}</p>
            <button onClick={fetchClubs} className="clubs-page__retry-btn">
              {t('common.retry')}
            </button>
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('clubsPage.title')}>
      <div className="clubs-page">
        <div className="clubs-page__header">
          <h1 className="clubs-page__title">{t('clubsPage.title')}</h1>
          <p className="clubs-page__description">
            {t('clubsPage.description')}
          </p>
        </div>

        <div className="clubs-page__search">
          <input
            type="text"
            placeholder={t('clubsPage.searchPlaceholder')}
            value={searchQuery}
            onChange={handleSearchChange}
            className="clubs-page__search-input"
          />
          {searchQuery && (
            <button
              className="clubs-page__search-clear"
              onClick={() => setSearchQuery('')}
              aria-label="Clear search"
            >
              ×
            </button>
          )}
        </div>

        {filteredClubs.length === 0 ? (
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
                        onError={(e) => {
                          const target = e.target as HTMLImageElement;
                          target.style.display = 'none';
                          target.parentElement!.innerHTML = '<span class="club-card__logo-placeholder">🏠</span>';
                        }}
                      />
                    ) : (
                      <span className="club-card__logo-placeholder">🏠</span>
                    )}
                  </div>
                  <div className="club-card__content">
                    <h2 className="club-card__name">{club.name}</h2>
                    {club.city && (
                      <p className="club-card__city">{club.city}</p>
                    )}
                    <span className="club-card__link">
                      {t('clubsPage.viewClub')} →
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
    </PageTemplate>
  );
}

export default ClubsPage;
