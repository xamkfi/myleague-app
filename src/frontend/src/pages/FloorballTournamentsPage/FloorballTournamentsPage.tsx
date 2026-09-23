import { useState, useEffect, useCallback, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import CatalogPage from '../../components/CatalogPage/CatalogPage';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import type { FloorballTournamentDto } from '../../types/floorball/tournamentTypes';
import { useAudience } from '../../context/AudienceContext';
import './FloorballTournamentsPage.scss';

type LifecycleStatus = 'upcoming' | 'ongoing' | 'past';

function formatDate(iso: string, locale: string): string {
  return new Date(iso).toLocaleDateString(locale, {
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

function FloorballTournamentsPage() {
  const { t, i18n } = useTranslation();
  const locale = i18n.language?.startsWith('en') ? 'en-GB' : 'fi-FI';
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

    const meta = [
      `${formatDate(tournament.startDate, locale)} – ${formatDate(tournament.endDate, locale)}`,
      tournament.venue,
      `${t('tournaments.teams', 'Joukkueet')}: ${tournament.teamCount}`,
    ].filter(Boolean).join(' · ');

    return (
      <Link
        key={tournament.id}
        to={`/tournaments/${tournament.id}?tab=summary`}
        className="tournament-card"
      >
        <div className="tournament-card__content">
          <span className={`tournament-card__badge tournament-card__badge--${lifecycle}`}>
            {lifecycleLabels[lifecycle]}
          </span>
          <h2 className="tournament-card__title">{tournament.name}</h2>
          <p className="tournament-card__meta">{meta}</p>
          {description && <p className="tournament-card__description">{description}</p>}
          <span className="tournament-card__link">{t('tournaments.open', 'Avaa turnaus')} →</span>
        </div>
      </Link>
    );
  };

  const page = (body: ReactNode) => (
    <PageTemplate title={t('nav.tournaments')} fullBleed>
      <CatalogPage
        title={t('nav.tournaments')}
        description={t('tournaments.intro', 'Selaa tulevia, käynnissä olevia ja päättyneitä turnauksia. Avaa turnaus nähdäksesi lohkot, otteluohjelman ja tilastot.')}
      >
        {body}
      </CatalogPage>
    </PageTemplate>
  );

  if (loading) {
    return page(
      <div className="tournaments-page__status">
        <LoadingSpinner variant="light" text={t('tournaments.loading', 'Ladataan turnauksia...')} />
      </div>,
    );
  }

  if (error) {
    return page(
      <div className="tournaments-page__status">
        <p>{error}</p>
        <button type="button" onClick={fetchTournaments} className="tournaments-page__retry-btn">
          {t('tournaments.retry', 'Yritä uudelleen')}
        </button>
      </div>,
    );
  }

  return page(
    tournaments.length === 0 ? (
      <div className="tournaments-page__status">
        <p>{t('tournaments.noTournaments', 'Ei turnauksia tällä hetkellä.')}</p>
      </div>
    ) : (
      <div className="tournaments-page__list">
        {tournaments.map(renderTournamentCard)}
      </div>
    ),
  );
}

export default FloorballTournamentsPage;
