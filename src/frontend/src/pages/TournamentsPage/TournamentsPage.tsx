import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import type { FloorballTournamentSummaryDto } from '../../types/floorball/floorballTypes';
import './TournamentsPage.css';

function TournamentsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [tournaments, setTournaments] = useState<FloorballTournamentSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        const response = await floorballTournamentService.getAll();
        setTournaments(response.data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load tournaments');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return dateString;
    }
  };

  const getStatusLabel = (status: string) => {
    const key = status.charAt(0).toLowerCase() + status.slice(1);
    return t(`tournament.status.${key}`, status);
  };

  const isActive = (status: string) =>
    status === 'Active' || status === 'InProgress';

  if (loading) {
    return (
      <PageTemplate title={t('tournament.title', 'Tournaments')}>
        <div className="tournaments-container">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={t('tournament.title', 'Tournaments')}>
        <div className="tournaments-container">
          <p className="error-message">{error}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('tournament.title', 'Tournaments')}>
      <div className="tournaments-container">
        <p className="tournaments-intro">
          {t('tournament.publicPage.tournamentInfo', 'Browse our tournaments.')}
        </p>

        {tournaments.length === 0 ? (
          <p>{t('tournament.noTournaments', 'No tournaments found')}</p>
        ) : (
          <div className="tournaments-list">
            {tournaments.map((tournament) => (
              <div key={tournament.id} className="tournament-card">
                <div className="tournament-header">
                  <h2 className="tournament-title">{tournament.name}</h2>
                  {isActive(tournament.status) ? (
                    <span className="registration-open">{getStatusLabel(tournament.status)}</span>
                  ) : (
                    <span className="registration-closed">{getStatusLabel(tournament.status)}</span>
                  )}
                </div>

                <div className="tournament-details">
                  <p>
                    <strong>{t('tournament.fields.startDate', 'Start')}:</strong>{' '}
                    {formatDate(tournament.startDate)} &ndash; {formatDate(tournament.endDate)}
                  </p>
                  {tournament.location && (
                    <p>
                      <strong>{t('tournament.fields.location', 'Location')}:</strong>{' '}
                      {tournament.location}
                    </p>
                  )}
                  <p>
                    {tournament.groupCount} {t('tournament.fields.groups', 'groups')} &middot;{' '}
                    {tournament.teamCount} {t('tournament.fields.teams', 'teams')}
                  </p>
                </div>

                <div className="tournament-actions">
                  <button
                    className="view-details-button"
                    onClick={() => navigate(`/turnaukset/${tournament.id}`)}
                  >
                    {t('tournament.publicPage.overview', 'View Details')}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </PageTemplate>
  );
}

export default TournamentsPage;
