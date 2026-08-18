import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import type { FloorballTournamentDto } from '../../types/floorball/tournamentTypes';
import { useAudience } from '../../context/AudienceContext';
import './TournamentsPage.scss';

type LifecycleStatus = 'upcoming' | 'ongoing' | 'past';

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fi-FI', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

/**
 * Determine the visible lifecycle (Tulossa / Käynnissä / Päättynyt) of a tournament.
 * Considers explicit Completed status, but otherwise computes against the current time
 * relative to the tournament's [startDate, endDate] window so admins don't have to manually
 * advance state for the listing to look right.
 */
function getLifecycleStatus(tournament: FloorballTournamentDto): LifecycleStatus {
  if (tournament.tournamentStatus === 'Completed' || tournament.isCompleted) {
    return 'past';
  }
  const now = Date.now();
  const start = new Date(tournament.startDate).getTime();
  const end = new Date(tournament.endDate).getTime();
  if (now < start) return 'upcoming';
  if (now > end) return 'past';
  return 'ongoing';
}

/**
 * Strip HTML tags from contentHtml for use as a card description.
 * We render plain text here (not innerHTML) because the card preview should be a single,
 * compact paragraph regardless of what the admin entered.
 */
function htmlToPlainText(html: string | null | undefined): string {
  if (!html) return '';
  const tmp = document.createElement('div');
  tmp.innerHTML = html;
  return (tmp.textContent || tmp.innerText || '').trim();
}

function truncate(text: string, max: number): string {
  if (text.length <= max) return text;
  return text.slice(0, max).trimEnd() + '…';
}

function TournamentsPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();

  const [tournaments, setTournaments] = useState<FloorballTournamentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchTournaments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await floorballTournamentService.getAll(audience.teamCategory);
      // Show upcoming + ongoing first, then past (sorted by start date asc within each group).
      const all = response.data ?? [];
      const sorted = [...all].sort((a, b) => {
        const aLifecycle = getLifecycleStatus(a);
        const bLifecycle = getLifecycleStatus(b);
        const order: Record<LifecycleStatus, number> = { ongoing: 0, upcoming: 1, past: 2 };
        if (order[aLifecycle] !== order[bLifecycle]) {
          return order[aLifecycle] - order[bLifecycle];
        }
        return new Date(a.startDate).getTime() - new Date(b.startDate).getTime();
      });
      setTournaments(sorted);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to load tournaments';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [audience.teamCategory]);

  useEffect(() => {
    fetchTournaments();
  }, [fetchTournaments]);

  const lifecycleLabels: Record<LifecycleStatus, string> = {
    upcoming: t('tournaments.statusUpcoming', 'Tulossa'),
    ongoing: t('tournaments.statusOngoing', 'Käynnissä'),
    past: t('tournaments.statusPast', 'Päättynyt'),
  };

  const renderTournamentCard = (tournament: FloorballTournamentDto) => {
    const lifecycle = getLifecycleStatus(tournament);
    const description = truncate(htmlToPlainText(tournament.contentHtml) || t('tournaments.cardDefaultDescription', 'Selaa turnauksen lohkoja, tuloksia ja tilastoja.'), 220);

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
              {formatDate(tournament.startDate)} – {formatDate(tournament.endDate)}
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
            <span className="tournament-card__meta-value">
              {tournament.teamCount}
              <span className="tournament-card__meta-separator"> · </span>
              <span className="tournament-card__meta-label">{t('tournaments.groups', 'Lohkot')}</span>
              <span className="tournament-card__meta-value-secondary">{tournament.groups.length}</span>
            </span>
          </div>
        </div>

        {description && (
          <p className="tournament-card__description">{description}</p>
        )}

        <nav className="tournament-card__links" aria-label={tournament.name}>
          <Link to={`/tournaments/${tournament.id}?tab=summary`} className="tournament-card__link">
            {t('tournaments.tabSummary', 'Yhteenveto')}
          </Link>
          <Link to={`/tournaments/${tournament.id}?tab=groups`} className="tournament-card__link">
            {t('tournaments.tabGroups', 'Lohkot')}
          </Link>
          <Link to={`/tournaments/${tournament.id}?tab=fixtures`} className="tournament-card__link">
            {t('tournaments.tabFixtures', 'Otteluohjelma')}
          </Link>
          <Link to={`/tournaments/${tournament.id}?tab=results`} className="tournament-card__link">
            {t('tournaments.tabResults', 'Tulokset')}
          </Link>
          <Link to={`/tournaments/${tournament.id}?tab=statistics`} className="tournament-card__link">
            {t('tournaments.tabStatistics', 'Tilastot')}
          </Link>
        </nav>
      </div>
    );
  };

  if (loading) {
    return (
      <PageTemplate title={t('nav.tournaments')}>
        <div className="tournaments-page">
          <div className="tournaments-page__loading">
            <LoadingSpinner variant="light" text={t('tournaments.loading', 'Ladataan turnauksia...')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={t('nav.tournaments')}>
        <div className="tournaments-page">
          <div className="tournaments-page__error">
            <p>{error}</p>
            <button onClick={fetchTournaments} className="tournaments-page__retry-btn">
              {t('tournaments.retry', 'Yritä uudelleen')}
            </button>
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('nav.tournaments')}>
      <div className="tournaments-page">
        <div className="tournaments-page__header">
          <h1 className="tournaments-page__title">{t('nav.tournaments')}</h1>
          <p className="tournaments-page__description">
            {t('tournaments.intro', 'Selaa tulevia, käynnissä olevia ja päättyneitä turnauksia. Avaa turnaus nähdäksesi lohkot, otteluohjelman ja tilastot.')}
          </p>
        </div>

        {tournaments.length === 0 ? (
          <div className="tournaments-page__empty">
            <p>{t('tournaments.noTournaments', 'Ei turnauksia tällä hetkellä.')}</p>
          </div>
        ) : (
          <div className="tournaments-page__list">
            {tournaments.map(renderTournamentCard)}
          </div>
        )}
      </div>
    </PageTemplate>
  );
}

export default TournamentsPage;
