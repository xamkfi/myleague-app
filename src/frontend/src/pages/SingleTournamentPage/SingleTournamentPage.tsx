import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import type {
  FloorballTournamentDto,
  FloorballTournamentGroupStandingsDto,
} from '../../types/floorball/floorballTypes';
import './SingleTournamentPage.scss';

type Tab = 'overview' | 'schedule' | 'standings';

const SingleTournamentPage = () => {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();

  const [tournament, setTournament] = useState<FloorballTournamentDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<Tab>('overview');

  // Standings state
  const [standings, setStandings] = useState<Record<string, FloorballTournamentGroupStandingsDto>>({});
  const [loadingStandings, setLoadingStandings] = useState(false);

  useEffect(() => {
    const load = async () => {
      if (!id) return;
      try {
        setLoading(true);
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

  const loadStandings = useCallback(async () => {
    if (!id || !tournament?.groups) return;
    setLoadingStandings(true);
    const results: Record<string, FloorballTournamentGroupStandingsDto> = {};
    for (const group of tournament.groups) {
      try {
        const response = await floorballTournamentService.getGroupStandings(id, group.id);
        results[group.id] = response.data;
      } catch {
        // skip failed group
      }
    }
    setStandings(results);
    setLoadingStandings(false);
  }, [id, tournament?.groups]);

  useEffect(() => {
    if (activeTab === 'standings' && tournament?.groups && Object.keys(standings).length === 0) {
      loadStandings();
    }
  }, [activeTab, tournament, standings, loadStandings]);

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return dateString;
    }
  };

  const formatDateTime = (dateString: string) => {
    try {
      const d = new Date(dateString);
      return `${d.toLocaleDateString()} ${d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
    } catch {
      return dateString;
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('tournament.publicPage.loading', 'Loading tournament...')}>
        <div className="single-tournament"><p>{t('common.loading', 'Loading...')}</p></div>
      </PageTemplate>
    );
  }

  if (error || !tournament) {
    return (
      <PageTemplate title={t('tournament.publicPage.notFound', 'Tournament not found')}>
        <div className="single-tournament"><p>{error || t('tournament.publicPage.notFound', 'Tournament not found')}</p></div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={tournament.name}>
      <div className="single-tournament">
        <div className="single-tournament__header">
          <h1>{tournament.name}</h1>
          <div className="single-tournament__meta">
            <span>{formatDate(tournament.startDate)} &ndash; {formatDate(tournament.endDate)}</span>
            {tournament.location && <span>&middot; {tournament.location}</span>}
          </div>
        </div>

        <div className="single-tournament__tabs">
          {(['overview', 'schedule', 'standings'] as Tab[]).map((tab) => (
            <button
              key={tab}
              className={`single-tournament__tab${activeTab === tab ? ' single-tournament__tab--active' : ''}`}
              onClick={() => setActiveTab(tab)}
            >
              {t(`tournament.publicPage.${tab}`, tab.charAt(0).toUpperCase() + tab.slice(1))}
            </button>
          ))}
        </div>

        <div className="single-tournament__content">
          {/* Overview Tab */}
          {activeTab === 'overview' && (
            <div className="single-tournament__overview">
              {tournament.descriptionHtml ? (
                <div
                  className="single-tournament__description"
                  dangerouslySetInnerHTML={{ __html: tournament.descriptionHtml }}
                />
              ) : (
                <p className="single-tournament__empty">{t('tournament.publicPage.noDescription', 'No description available.')}</p>
              )}

              {tournament.groups && tournament.groups.length > 0 && (
                <div className="single-tournament__groups-overview">
                  <h3>{t('tournament.publicPage.groupsAndTeams', 'Groups & Teams')}</h3>
                  <div className="single-tournament__groups-grid">
                    {tournament.groups
                      .slice()
                      .sort((a, b) => a.sortOrder - b.sortOrder)
                      .map((group) => (
                        <div key={group.id} className="single-tournament__group-card">
                          <h4>{group.name}</h4>
                          {group.teams.length === 0 ? (
                            <p className="single-tournament__empty">-</p>
                          ) : (
                            <ul>
                              {group.teams.map((gt) => (
                                <li key={gt.teamId}>{gt.teamName}</li>
                              ))}
                            </ul>
                          )}
                        </div>
                      ))}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Schedule Tab */}
          {activeTab === 'schedule' && (
            <div className="single-tournament__schedule">
              <h3>{t('tournament.publicPage.matchSchedule', 'Match Schedule')}</h3>
              {(!tournament.matches || tournament.matches.length === 0) ? (
                <p className="single-tournament__empty">{t('tournament.publicPage.noMatches', 'No matches.')}</p>
              ) : (
                <div className="single-tournament__matches">
                  {tournament.matches
                    .slice()
                    .sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime())
                    .map((match) => (
                      <div key={match.id} className="single-tournament__match-row">
                        <span className="single-tournament__match-time">
                          {formatDateTime(match.scheduledDateTime)}
                        </span>
                        <span className="single-tournament__match-teams">
                          <span className="single-tournament__team-home">{match.homeTeamName}</span>
                          <span className="single-tournament__match-score">
                            {match.status === 'Completed' || match.status === 'InProgress'
                              ? `${match.homeScore} - ${match.awayScore}`
                              : t('tournament.publicPage.vs', 'vs')}
                          </span>
                          <span className="single-tournament__team-away">{match.awayTeamName}</span>
                        </span>
                        {match.venue && (
                          <span className="single-tournament__match-venue">{match.venue}</span>
                        )}
                      </div>
                    ))}
                </div>
              )}
            </div>
          )}

          {/* Standings Tab */}
          {activeTab === 'standings' && (
            <div className="single-tournament__standings">
              {loadingStandings ? (
                <p>{t('common.loading', 'Loading...')}</p>
              ) : tournament.groups && tournament.groups.length > 0 ? (
                tournament.groups
                  .slice()
                  .sort((a, b) => a.sortOrder - b.sortOrder)
                  .map((group) => {
                    const groupStandings = standings[group.id];
                    return (
                      <div key={group.id} className="single-tournament__standings-group">
                        <h3>{group.name}</h3>
                        {!groupStandings || groupStandings.entries.length === 0 ? (
                          <p className="single-tournament__empty">{t('tournament.publicPage.noStandings', 'No standings available.')}</p>
                        ) : (
                          <table className="single-tournament__standings-table">
                            <thead>
                              <tr>
                                <th>#</th>
                                <th>{t('tournament.fields.teamName', 'Team')}</th>
                                <th>GP</th>
                                <th>W</th>
                                <th>D</th>
                                <th>L</th>
                                <th>GF</th>
                                <th>GA</th>
                                <th>GD</th>
                                <th>PTS</th>
                              </tr>
                            </thead>
                            <tbody>
                              {groupStandings.entries.map((entry) => (
                                <tr key={entry.teamId}>
                                  <td>{entry.rank}</td>
                                  <td className="single-tournament__standings-team">{entry.teamName}</td>
                                  <td>{entry.gamesPlayed}</td>
                                  <td>{entry.wins}</td>
                                  <td>{entry.draws}</td>
                                  <td>{entry.losses}</td>
                                  <td>{entry.goalsFor}</td>
                                  <td>{entry.goalsAgainst}</td>
                                  <td>{entry.goalDifference > 0 ? `+${entry.goalDifference}` : entry.goalDifference}</td>
                                  <td className="single-tournament__standings-points">{entry.points}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        )}
                      </div>
                    );
                  })
              ) : (
                <p className="single-tournament__empty">{t('tournament.publicPage.noStandings', 'No standings available.')}</p>
              )}
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
};

export default SingleTournamentPage;
