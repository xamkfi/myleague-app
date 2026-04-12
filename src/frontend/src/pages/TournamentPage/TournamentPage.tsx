import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams, Link } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import type { FloorballTournamentDto } from '../../types/floorball/tournamentTypes';

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

const STATUS_COLORS: Record<string, string> = {
  Draft: '#6b7280',
  Registration: '#3b82f6',
  GroupStage: '#f59e0b',
  PlayoffStage: '#8b5cf6',
  Completed: '#10b981',
};

function TournamentPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();

  const [tournament, setTournament] = useState<FloorballTournamentDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    const load = async () => {
      try {
        const response = await floorballTournamentService.getById(id);
        setTournament(response.data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load tournament');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id]);

  if (loading) {
    return (
      <PageTemplate title={t('tournaments.loading', 'Loading...')}>
        <div style={{ textAlign: 'center', padding: '3rem', color: '#6b7280' }}>
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (error || !tournament) {
    return (
      <PageTemplate title={t('tournaments.error', 'Error')}>
        <div style={{ textAlign: 'center', padding: '3rem' }}>
          <p style={{ color: '#ef4444', marginBottom: '1rem' }}>
            {error ?? t('tournaments.notFound', 'Tournament not found')}
          </p>
          <Link to="/tournaments" style={{ color: '#3b82f6' }}>
            {t('tournaments.backToList', 'Back to tournaments')}
          </Link>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={tournament.name}>
      <div style={{ maxWidth: '960px', margin: '0 auto', padding: '1.5rem' }}>
        {/* Header Card */}
        <div
          style={{
            background: '#fff',
            borderRadius: '0.75rem',
            boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
            padding: '2rem',
            marginBottom: '1.5rem',
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '1rem' }}>
            <div>
              <h1 style={{ margin: '0 0 0.5rem', fontSize: '1.75rem' }}>{tournament.name}</h1>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1.5rem', color: '#6b7280', fontSize: '0.9375rem' }}>
                <span>
                  <i className="fas fa-calendar-alt" style={{ marginRight: '0.4rem' }}></i>
                  {formatDate(tournament.startDate)} – {formatDate(tournament.endDate)}
                </span>
                {tournament.venue && (
                  <span>
                    <i className="fas fa-map-marker-alt" style={{ marginRight: '0.4rem' }}></i>
                    {tournament.venue}
                  </span>
                )}
                <span>
                  <i className="fas fa-users" style={{ marginRight: '0.4rem' }}></i>
                  {tournament.teamCount} {t('tournaments.teams', 'teams')}
                </span>
              </div>
            </div>
            <span
              style={{
                display: 'inline-block',
                padding: '0.375rem 1rem',
                borderRadius: '9999px',
                fontSize: '0.8125rem',
                fontWeight: 600,
                color: '#fff',
                backgroundColor: STATUS_COLORS[tournament.tournamentStatus] ?? '#6b7280',
              }}
            >
              {getStatusLabel(tournament.tournamentStatus)}
            </span>
          </div>
        </div>

        {/* Description */}
        {tournament.contentHtml && (
          <div
            style={{
              background: '#fff',
              borderRadius: '0.75rem',
              boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
              padding: '1.5rem',
              marginBottom: '1.5rem',
            }}
          >
            <h2 style={{ fontSize: '1.25rem', marginBottom: '1rem' }}>
              {t('tournaments.about', 'About')}
            </h2>
            <div dangerouslySetInnerHTML={{ __html: tournament.contentHtml }} />
          </div>
        )}

        {/* Groups */}
        {tournament.groups.length > 0 && (
          <div>
            <h2 style={{ fontSize: '1.25rem', marginBottom: '1rem' }}>
              {t('tournaments.groups', 'Groups')}
            </h2>
            <div
              style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
                gap: '1rem',
              }}
            >
              {tournament.groups
                .slice()
                .sort((a, b) => a.order - b.order)
                .map((group) => (
                  <div
                    key={group.id}
                    style={{
                      background: '#fff',
                      borderRadius: '0.75rem',
                      boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
                      padding: '1.25rem',
                    }}
                  >
                    <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem', color: '#374151' }}>
                      {group.name}
                    </h3>
                    {group.teams.length === 0 ? (
                      <p style={{ color: '#9ca3af', fontSize: '0.875rem', margin: 0 }}>
                        {t('tournaments.noTeamsInGroup', 'No teams in this group yet.')}
                      </p>
                    ) : (
                      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
                        {group.teams.map((gt, index) => (
                          <li
                            key={gt.id}
                            style={{
                              padding: '0.5rem 0',
                              borderBottom: index < group.teams.length - 1 ? '1px solid #f3f4f6' : 'none',
                              display: 'flex',
                              alignItems: 'center',
                              gap: '0.5rem',
                              fontSize: '0.9375rem',
                            }}
                          >
                            <span
                              style={{
                                width: '1.5rem',
                                height: '1.5rem',
                                borderRadius: '50%',
                                background: '#e5e7eb',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                fontSize: '0.75rem',
                                fontWeight: 600,
                                color: '#6b7280',
                                flexShrink: 0,
                              }}
                            >
                              {index + 1}
                            </span>
                            {gt.teamName}
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                ))}
            </div>
          </div>
        )}

        {/* Back link */}
        <div style={{ marginTop: '2rem' }}>
          <Link to="/tournaments" style={{ color: '#3b82f6', textDecoration: 'none', fontSize: '0.9375rem' }}>
            &larr; {t('tournaments.backToList', 'Back to tournaments')}
          </Link>
        </div>
      </div>
    </PageTemplate>
  );
}

export default TournamentPage;
