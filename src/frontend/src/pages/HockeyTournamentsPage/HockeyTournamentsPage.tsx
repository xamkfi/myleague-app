import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { hockeyTournamentService } from '../../api/hockey/hockeyTournamentService';
import type { HockeyTournamentDto } from '../../types/hockey/hockeyTypes';
import { formatHockeyDate } from '../../utils/hockeyLookups';
import { useAudience } from '../../context/AudienceContext';
import '../TournamentsPage/TournamentsPage.scss';

type LifecycleStatus = 'upcoming' | 'ongoing' | 'past';

function getLifecycleStatus(tournament: HockeyTournamentDto): LifecycleStatus {
  if (tournament.status === 'Completed' || tournament.isCompleted) {
    return 'past';
  }
  const now = Date.now();
  const start = new Date(tournament.startDate).getTime();
  const end = new Date(tournament.endDate).getTime();
  if (now < start) {
    return 'upcoming';
  }
  if (now > end) {
    return 'past';
  }
  return 'ongoing';
}

function HockeyTournamentsPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [tournaments, setTournaments] = useState<HockeyTournamentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchTournaments = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      setError(null);
      const all = await hockeyTournamentService.getAll(audience.teamCategory);
      const sorted = [...all].sort((a, b) => {
        const order: Record<LifecycleStatus, number> = { ongoing: 0, upcoming: 1, past: 2 };
        const aLifecycle = getLifecycleStatus(a);
        const bLifecycle = getLifecycleStatus(b);
        if (order[aLifecycle] !== order[bLifecycle]) {
          return order[aLifecycle] - order[bLifecycle];
        }
        return new Date(a.startDate).getTime() - new Date(b.startDate).getTime();
      });
      setTournaments(sorted);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tournaments');
    } finally {
      setLoading(false);
    }
  }, [audience.teamCategory]);

  useEffect(() => {
    void fetchTournaments();
  }, [fetchTournaments]);

  const lifecycleLabels: Record<LifecycleStatus, string> = {
    upcoming: t('tournaments.statusUpcoming', 'Tulossa'),
    ongoing: t('tournaments.statusOngoing', 'Käynnissä'),
    past: t('tournaments.statusPast', 'Päättynyt'),
  };

  if (loading) {
    return (
      <PageTemplate title={t('hockey.tournaments.title', 'Hockey tournaments')}>
        <div className="tournaments-page">
          <div className="tournaments-page__loading">
            <LoadingSpinner variant="light" text={t('tournaments.loading', 'Ladataan turnauksia...')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('hockey.tournaments.title', 'Hockey tournaments')}>
      <div className="tournaments-page">
        <div className="tournaments-page__header">
          <h1 className="tournaments-page__title">{t('hockey.tournaments.title', 'Hockey tournaments')}</h1>
          <p className="tournaments-page__description">
            {t('hockeyPage.tournamentsIntro', 'Browse hockey tournaments, groups, and results.')}
          </p>
        </div>
        {error && (
          <div className="tournaments-page__error">
            <p>{error}</p>
            <button type="button" className="tournaments-page__retry-btn" onClick={() => void fetchTournaments()}>
              {t('hockeyPage.retry', 'Try again')}
            </button>
          </div>
        )}
        {tournaments.length === 0 ? (
          <div className="tournaments-page__empty">
            <p>{t('tournaments.noTournaments', 'Ei turnauksia tällä hetkellä.')}</p>
          </div>
        ) : (
          <div className="tournaments-page__list">
            {tournaments.map((tournament) => {
              const lifecycle = getLifecycleStatus(tournament);
              return (
                <div key={tournament.id} className="tournament-card">
                  <div className="tournament-card__header">
                    <h2 className="tournament-card__title">{tournament.name}</h2>
                    <span className={`tournament-card__badge tournament-card__badge--${lifecycle}`}>
                      {lifecycleLabels[lifecycle]}
                    </span>
                  </div>
                  <div className="tournament-card__meta">
                    <div className="tournament-card__meta-row">
                      <span className="tournament-card__meta-label">{t('tournaments.dates', 'Päivämäärät')}</span>
                      <span className="tournament-card__meta-value">
                        {formatHockeyDate(tournament.startDate)} – {formatHockeyDate(tournament.endDate)}
                      </span>
                    </div>
                    {tournament.venue && (
                      <div className="tournament-card__meta-row">
                        <span className="tournament-card__meta-label">{t('tournaments.venue', 'Paikka')}</span>
                        <span className="tournament-card__meta-value">{tournament.venue}</span>
                      </div>
                    )}
                    <div className="tournament-card__meta-row">
                      <span className="tournament-card__meta-label">{t('tournaments.teams', 'Joukkueet')}</span>
                      <span className="tournament-card__meta-value">{tournament.teams.length}</span>
                    </div>
                  </div>
                  <nav className="tournament-card__links" aria-label={tournament.name}>
                    <Link to={`/hockey/tournaments/${tournament.id}`} className="tournament-card__link">
                      {t('tournaments.tabSummary', 'Yhteenveto')}
                    </Link>
                    <Link to={`/hockey/tournaments/${tournament.id}`} className="tournament-card__link">
                      {t('tournaments.tabGroups', 'Lohkot')}
                    </Link>
                  </nav>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </PageTemplate>
  );
}

export default HockeyTournamentsPage;
