import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import type { FloorballTournamentDto } from '../../types/floorball/tournamentTypes';
import './TournamentsPage.css';

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fi-FI', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

function getStatusLabel(status: string): string {
  const map: Record<string, string> = {
    Draft: 'Upcoming',
    Registration: 'Registration Open',
    GroupStage: 'Group Stage',
    PlayoffStage: 'Playoff Stage',
    Completed: 'Completed',
  };
  return map[status] ?? status;
}

function TournamentsPage() {
  const { t } = useTranslation();

  const [tournaments, setTournaments] = useState<FloorballTournamentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await floorballTournamentService.getActive();
        setTournaments(response.data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load tournaments');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  return (
    <PageTemplate title={t('nav.tournaments')}>
      <div className="tournaments-container">
        <p className="tournaments-intro">
          {t('tournaments.intro', 'Browse our upcoming and active tournaments.')}
        </p>

        {loading && (
          <div style={{ textAlign: 'center', padding: '3rem' }}>
            <p style={{ color: '#6b7280' }}>{t('common.loading', 'Loading...')}</p>
          </div>
        )}

        {error && (
          <div style={{ textAlign: 'center', padding: '2rem', color: '#ef4444' }}>
            <p>{error}</p>
          </div>
        )}

        {!loading && !error && tournaments.length === 0 && (
          <div style={{ textAlign: 'center', padding: '3rem', color: '#6b7280' }}>
            <p>{t('tournaments.noTournaments', 'No active tournaments at this time.')}</p>
          </div>
        )}

        <div className="tournaments-list">
          {tournaments.map((tournament) => (
            <div key={tournament.id} className="tournament-card">
              <div className="tournament-header">
                <h2 className="tournament-title">{tournament.name}</h2>
                <span
                  className={
                    tournament.tournamentStatus === 'Registration'
                      ? 'registration-open'
                      : tournament.tournamentStatus === 'Completed'
                        ? 'registration-closed'
                        : 'registration-open'
                  }
                >
                  {getStatusLabel(tournament.tournamentStatus)}
                </span>
              </div>

              <div className="tournament-details">
                <p>
                  <strong>{t('tournaments.dates', 'Dates')}:</strong>{' '}
                  {formatDate(tournament.startDate)} – {formatDate(tournament.endDate)}
                </p>
                {tournament.venue && (
                  <p>
                    <strong>{t('tournaments.venue', 'Venue')}:</strong> {tournament.venue}
                  </p>
                )}
                <p>
                  <strong>{t('tournaments.teams', 'Teams')}:</strong> {tournament.teamCount} |{' '}
                  <strong>{t('tournaments.groups', 'Groups')}:</strong> {tournament.groups.length}
                </p>
              </div>

              <div className="tournament-actions">
                <Link to={`/tournaments/${tournament.id}`} className="view-details-button">
                  {t('tournaments.viewDetails', 'View Details')}
                </Link>
              </div>
            </div>
          ))}
        </div>
      </div>
    </PageTemplate>
  );
}

export default TournamentsPage;
