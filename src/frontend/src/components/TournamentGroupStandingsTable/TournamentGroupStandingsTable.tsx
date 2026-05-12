import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { floorballStatisticsService } from '../../api/floorball/floorballStatistics';
import type { FloorballTournamentGroupStandingDto } from '../../types/floorball/tournamentTypes';
import { useFloorballTeamsData } from '../../hooks/useTeamsData';
import { createTeamSlug } from '../../utils/slugUtils';
import '../LeagueStanding/LeagueStanding.scss';

interface TournamentGroupStandingsTableProps {
  groupId: string;
  groupName: string;
}

export default function TournamentGroupStandingsTable({ groupId, groupName }: TournamentGroupStandingsTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { teams, refetch } = useFloorballTeamsData();
  const [rows, setRows] = useState<FloorballTournamentGroupStandingDto[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    refetch();
  }, [refetch]);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await floorballStatisticsService.getTournamentGroupStandings(groupId);
        if (!cancelled) {
          setRows(data);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Failed to load group standings');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };
    load();
    return () => {
      cancelled = true;
    };
  }, [groupId]);

  const navigateToTeam = (teamId: string): void => {
    const team = teams?.find((x) => x.id === teamId);
    if (!team) return;
    const slug = createTeamSlug(team, teams);
    navigate(`/team/${slug}`);
  };

  return (
    <div className="standing-container">
      <div className="standing-header">
        <div className="header-top-row">
          <div className="league-selector">
            <span className="league-title">{groupName}</span>
          </div>
        </div>
      </div>

      <div className="table-wrapper">
        {loading ? (
          <div style={{ padding: '1.5rem', textAlign: 'center', color: '#6b7280' }}>
            {t('leaguePage.summary.loading', 'Loading...')}
          </div>
        ) : error ? (
          <div style={{ padding: '1.5rem', textAlign: 'center', color: '#ef4444' }}>{error}</div>
        ) : !rows || rows.length === 0 ? (
          <div style={{ padding: '1.5rem', textAlign: 'center', color: '#6b7280' }}>
            {t('tournaments.standings.empty', 'No matches played yet in this group.')}
          </div>
        ) : (
          <table className="standing-table">
            <colgroup>
              <col className="rank-col" />
              <col className="team-col" />
              <col className="spacer-col" />
              <col className="stats-col" />
              <col className="stats-col" />
              <col className="stats-col" />
              <col className="stats-col" />
              <col className="goals-col" />
              <col className="stats-col" />
              <col className="points-col" />
            </colgroup>
            <thead>
              <tr className="header-row">
                <th className="rank-col">#</th>
                <th className="team-col">TEAM</th>
                <th className="spacer-col"></th>
                <th className="stats-col" title="Pelatut ottelut (Matches Played)">MP</th>
                <th className="stats-col" title="Voitot (Wins)">W</th>
                <th className="stats-col" title="Tasapelit (Draws)">D</th>
                <th className="stats-col" title="Tappiot (Losses)">L</th>
                <th className="goals-col" title="Tehdyt : Päästetyt maalit (Goals)">G</th>
                <th className="stats-col" title="Maaliero (Goal Difference)">GD</th>
                <th className="points-col" title="Pisteet (Points)">PTS</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row, index) => (
                <tr
                  key={row.teamId}
                  className="clickable-row"
                  onClick={() => navigateToTeam(row.teamId)}
                >
                  <td className="rank-col">{index + 1}</td>
                  <td className="team-col">
                    <div className="team-info">
                      {row.teamLogo && row.teamLogo.trim() !== '' ? (
                        <img
                          className="logo-image"
                          src={row.teamLogo}
                          alt={row.teamName}
                          onError={(e) => {
                            const target = e.target as HTMLImageElement;
                            target.style.display = 'none';
                          }}
                        />
                      ) : (
                        <div className="logo-empty"></div>
                      )}
                      <span className="team-name">{row.teamName}</span>
                    </div>
                  </td>
                  <td className="spacer-col"></td>
                  <td className="stats-col">{row.gamesPlayed}</td>
                  <td className="stats-col">{row.wins}</td>
                  <td className="stats-col">{row.draws}</td>
                  <td className="stats-col">{row.losses}</td>
                  <td className="goals-col">
                    {row.goalsFor}:{row.goalsAgainst}
                  </td>
                  <td className="stats-col">{row.goalDifference}</td>
                  <td className="points-col">{row.points}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
