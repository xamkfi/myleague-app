import { useEffect, useState, useMemo } from "react";
import { useParams, Link } from "react-router-dom";
import { FloorballPosition } from "../../types/floorball/floorballTypes";
import {
  floorballPlayerService,
  type FloorballPlayerWithMatchesDto,
  type FloorballPlayerMatchDto,
} from "../../api/floorball/floorballPlayerService";
import {
  floorballStatisticsService,
  type FloorballPlayerProfileDto,
  type FloorballPlayerSeasonStatisticsDto,
  type FloorballGoalieSeasonStatisticsDto,
} from "../../api/floorball/floorballStatistics";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import { slugify } from "../../utils/slugUtils";
import './FloorballTeamPlayerUserPage.scss';

const getPositionText = (position: FloorballPosition | string): string => {
  switch (position) {
    case FloorballPosition.Goalkeeper:
    case 'Goalkeeper':
      return 'Maalivahti';
    case FloorballPosition.Defender:
    case 'Defender':
      return 'Puolustaja';
    case FloorballPosition.Forward:
    case 'Forward':
      return 'Hyökkääjä';
    default:
      return 'Pelaaja';
  }
};

const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr);
  return date.toLocaleDateString('fi-FI', {
    weekday: 'short',
    day: 'numeric',
    month: 'numeric',
    year: 'numeric',
  });
};

const calculateAge = (birthDate: string | null): number | null => {
  if (!birthDate) return null;
  const birth = new Date(birthDate);
  const today = new Date();
  let age = today.getFullYear() - birth.getFullYear();
  const monthDiff = today.getMonth() - birth.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age--;
  }
  return age;
};

const formatBirthDate = (birthDate: string | null): string => {
  if (!birthDate) return '';
  const date = new Date(birthDate);
  return date.toLocaleDateString('fi-FI');
};

interface SeasonTotals {
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  penaltyMinutes: number;
  plusMinusRating: number;
  powerPlayGoals: number;
  powerPlayAssists: number;
  shortHandedGoals: number;
  shortHandedAssists: number;
}

const calculateSeasonTotals = (stats: FloorballPlayerSeasonStatisticsDto[]): SeasonTotals => {
  return stats.reduce(
    (total, s) => ({
      gamesPlayed: total.gamesPlayed + s.gamesPlayed,
      goals: total.goals + s.goals,
      assists: total.assists + s.assists,
      points: total.points + s.points,
      penaltyMinutes: total.penaltyMinutes + s.penaltyMinutes,
      plusMinusRating: total.plusMinusRating + s.plusMinusRating,
      powerPlayGoals: total.powerPlayGoals + s.powerPlayGoals,
      powerPlayAssists: total.powerPlayAssists + s.powerPlayAssists,
      shortHandedGoals: total.shortHandedGoals + s.shortHandedGoals,
      shortHandedAssists: total.shortHandedAssists + s.shortHandedAssists,
    }),
    {
      gamesPlayed: 0, goals: 0, assists: 0, points: 0, penaltyMinutes: 0,
      plusMinusRating: 0, powerPlayGoals: 0, powerPlayAssists: 0,
      shortHandedGoals: 0, shortHandedAssists: 0,
    }
  );
};

interface MatchTotals {
  goals: number;
  assists: number;
  penaltyMinutes: number;
}

const calculateMatchTotals = (matches: FloorballPlayerMatchDto[]): MatchTotals => {
  return matches.reduce(
    (total, m) => ({
      goals: total.goals + (m.playerStats?.goals ?? 0),
      assists: total.assists + (m.playerStats?.assists ?? 0),
      penaltyMinutes: total.penaltyMinutes + (m.playerStats?.penaltyMinutes ?? 0),
    }),
    { goals: 0, assists: 0, penaltyMinutes: 0 }
  );
};

interface GoalieTotals {
  gamesPlayed: number;
  wins: number;
  losses: number;
  ties: number;
  saves: number;
  shotsAgainst: number;
  goalsAgainst: number;
  shutouts: number;
  minutesPlayed: number;
}

const calculateGoalieTotals = (stats: FloorballGoalieSeasonStatisticsDto[]): GoalieTotals => {
  return stats.reduce(
    (total, s) => ({
      gamesPlayed: total.gamesPlayed + s.gamesPlayed,
      wins: total.wins + s.wins,
      losses: total.losses + s.losses,
      ties: total.ties + s.ties,
      saves: total.saves + s.saves,
      shotsAgainst: total.shotsAgainst + s.shotsAgainst,
      goalsAgainst: total.goalsAgainst + s.goalsAgainst,
      shutouts: total.shutouts + s.shutouts,
      minutesPlayed: total.minutesPlayed + s.minutesPlayed,
    }),
    {
      gamesPlayed: 0, wins: 0, losses: 0, ties: 0, saves: 0,
      shotsAgainst: 0, goalsAgainst: 0, shutouts: 0, minutesPlayed: 0,
    }
  );
};

const calculateOverallSavePercentage = (totals: GoalieTotals): number => {
  return totals.shotsAgainst > 0 ? (totals.saves / totals.shotsAgainst) * 100 : 0;
};

const FloorballTeamPlayerUserPage = () => {
  const { id } = useParams<{ id: string }>();
  const [profile, setProfile] = useState<FloorballPlayerProfileDto | null>(null);
  const [matchData, setMatchData] = useState<FloorballPlayerWithMatchesDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadPlayerData = async () => {
      if (!id) return;

      try {
        setLoading(true);
        setError(null);

        const [profileResult, matchesResult] = await Promise.all([
          floorballStatisticsService.getPlayerProfile(id),
          floorballPlayerService.getPlayerMatches(id, 50),
        ]);

        setProfile(profileResult);
        setMatchData(matchesResult);
      } catch (err) {
        console.error('Error loading player data:', err);
        setError(err instanceof Error ? err.message : 'Virhe ladattaessa pelaajan tietoja');
      } finally {
        setLoading(false);
      }
    };

    loadPlayerData();
  }, [id]);

  const seasonStats = useMemo(() => profile?.seasonStatistics ?? [], [profile]);
  const goalieStats = useMemo(() => profile?.seasonStatisticsForGoalie ?? [], [profile]);
  const matches = useMemo(() => matchData?.recentMatches ?? [], [matchData]);
  const totals = useMemo(() => calculateSeasonTotals(seasonStats), [seasonStats]);
  const goalieTotals = useMemo(() => calculateGoalieTotals(goalieStats), [goalieStats]);
  const matchTotals = useMemo(() => calculateMatchTotals(matches), [matches]);

  if (loading) return <PageTemplate title="Pelaaja"><div className="player-loading">Ladataan...</div></PageTemplate>;
  if (error) return <PageTemplate title="Pelaaja"><div className="player-error">Virhe: {error}</div></PageTemplate>;
  if (!profile) return <PageTemplate title="Pelaaja"><div className="player-error">Pelaajaa ei löytynyt</div></PageTemplate>;

  const { player } = profile;
  const playerName = player.person.fullName;
  const isCreator = playerName === 'Tuomas Reijonen';
  const age = calculateAge(player.person.birthDate);
  const teamName = matchData?.teamName ?? player.team?.name ?? '';
  const position = matchData?.position ?? player.position;
  const jerseyNumber = matchData?.jerseyNumber;

  return (
    <PageTemplate title={playerName}>
      <div className="player-page">
        {/* Player Header */}
        <div className="player-container">
          <div className="player-info-layout">
            <div className="player-info-box">
              <div className={`player-avatar-large${isCreator ? ' player-avatar--creator' : ''}`}>
                {isCreator ? (
                  <img
                    className="creator-avatar-img"
                    src="https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExanpjZzBqNm1xYnp1d3Y5c2V5OWxoeTg2ZjV5dHpldHQ4anI0dDd5MSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/sPE5g5cHJ3dNm/giphy.gif"
                    alt="Creator avatar"
                  />
                ) : seasonStats[0]?.teamLogo ? (
                  <img
                    className="team-logo-img"
                    src={seasonStats[0].teamLogo}
                    alt={teamName}
                  />
                ) : null}
              </div>
              <div className="player-details">
                <div className={`player-name${isCreator ? ' player-name--creator' : ''}`}>
                  {playerName}
                  {isCreator && (
                    <span className="creator-title" title="System Creator">Macho King</span>
                  )}
                </div>
                <div className="player-details-row">
                  {teamName && <span className="player-team">{teamName}</span>}
                  <span className="player-position">{getPositionText(position)}</span>
                  {jerseyNumber != null && <span className="player-jersey">#{jerseyNumber}</span>}
                </div>
              </div>
            </div>

            <div className="player-stats-box">
              {age !== null && (
                <div className="stat-item">
                  <span className="stat-label">Ikä:</span>
                  <span className="stat-value">{age} ({formatBirthDate(player.person.birthDate)})</span>
                </div>
              )}
              <div className="stat-item">
                <span className="stat-label">Status:</span>
                <span className={`stat-value ${player.isActive ? 'active' : 'inactive'}`}>
                  {player.isActive ? 'Aktiivinen' : 'Ei aktiivinen'}
                </span>
              </div>
              <div className="stat-item">
                <span className="stat-label">Pelipaikka:</span>
                <span className="stat-value">{getPositionText(position)}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Career Summary Boxes */}
        <div className="player-container">
          <div className="career-stats-section">
            <h3>Urastatistiikka</h3>
            <div className="stats-grid">
              <div className="stats-box">
                <div className="stats-value">{totals.gamesPlayed}</div>
                <div className="stats-label">Ottelut</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.goals}</div>
                <div className="stats-label">Maalit</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.assists}</div>
                <div className="stats-label">Syötöt</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.points}</div>
                <div className="stats-label">Pisteet</div>
              </div>
              <div className="stats-box">
                <div className="stats-value">{totals.penaltyMinutes}</div>
                <div className="stats-label">Jäähymin</div>
              </div>
            </div>
          </div>
        </div>

        {/* Goalie Career Summary - shown only if player has goalie stats */}
        {goalieStats.length > 0 && (
          <div className="player-container">
            <div className="career-stats-section">
              <h3>Maalivahtitilastot (ura)</h3>
              <div className="stats-grid">
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.gamesPlayed}</div>
                  <div className="stats-label">Ottelut</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.wins}</div>
                  <div className="stats-label">Voitot</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.losses}</div>
                  <div className="stats-label">Tappiot</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{calculateOverallSavePercentage(goalieTotals).toFixed(1)}%</div>
                  <div className="stats-label">Torjunta-%</div>
                </div>
                <div className="stats-box">
                  <div className="stats-value">{goalieTotals.shutouts}</div>
                  <div className="stats-label">Nollapelit</div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Otteluhistoria (Match History) */}
        <div className="player-container">
          <div className="section-block">
            <h3>Otteluhistoria</h3>
            {matches.length > 0 ? (
              <div className="stats-table-scroll">
                <table className="stats-table">
                  <thead>
                    <tr>
                      <th className="col-date">Päivä</th>
                      <th className="col-team">Koti</th>
                      <th className="col-score">Tulos</th>
                      <th className="col-team">Vieras</th>
                      <th className="col-num" title="Maalit (Goals)">M</th>
                      <th className="col-num" title="Syötöt (Assists)">S</th>
                      <th className="col-num" title="Pisteet (Points)">P</th>
                      <th className="col-num" title="Jäähyminuutit (Penalty Minutes)">JM</th>
                    </tr>
                  </thead>
                  <tbody>
                    {matches.map((match) => (
                      <tr key={match.id}>
                        <td className="col-date">{formatDate(match.scheduledDateTime)}</td>
                        <td className="col-team">
                          <Link to={`/team/${slugify(match.homeTeamName)}`} className="team-link">{match.homeTeamName}</Link>
                        </td>
                        <td className="col-score">{match.homeScore} - {match.awayScore}</td>
                        <td className="col-team">
                          <Link to={`/team/${slugify(match.awayTeamName)}`} className="team-link">{match.awayTeamName}</Link>
                        </td>
                        <td className="col-num">{match.playerStats?.goals ?? 0}</td>
                        <td className="col-num">{match.playerStats?.assists ?? 0}</td>
                        <td className="col-num">{(match.playerStats?.goals ?? 0) + (match.playerStats?.assists ?? 0)}</td>
                        <td className="col-num">{match.playerStats?.penaltyMinutes ?? 0}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="totals-row">
                      <td colSpan={4}>Ottelut yhteensä: {matches.length}</td>
                      <td className="col-num">{matchTotals.goals}</td>
                      <td className="col-num">{matchTotals.assists}</td>
                      <td className="col-num">{matchTotals.goals + matchTotals.assists}</td>
                      <td className="col-num">{matchTotals.penaltyMinutes}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            ) : (
              <p className="no-data-message">Ei otteluhistoriaa saatavilla.</p>
            )}
          </div>
        </div>

        {/* Henkilökohtaiset tilastot (Personal Season Statistics) */}
        <div className="player-container">
          <div className="section-block">
            <h3>Henkilökohtaiset tilastot</h3>
            {seasonStats.length > 0 ? (
              <div className="stats-table-scroll">
                <table className="stats-table">
                  <thead>
                    <tr>
                      <th className="col-season">Kausi</th>
                      <th className="col-team">Joukkue</th>
                      <th className="col-num" title="Pelatut ottelut (Games Played)">O</th>
                      <th className="col-num" title="Maalit (Goals)">M</th>
                      <th className="col-num" title="Syötöt (Assists)">S</th>
                      <th className="col-num" title="Pisteet (Points)">P</th>
                      <th className="col-num" title="Jäähyminuutit (Penalty Minutes)">JM</th>
                      <th className="col-num" title="Plus/miinus-tilasto (+/-)">+/-</th>
                      <th className="col-num" title="Ylivoimamaalit (Power Play Goals)">YVM</th>
                      <th className="col-num" title="Ylivoimasyötöt (Power Play Assists)">YVS</th>
                      <th className="col-num" title="Alivoimamaalit (Shorthanded Goals)">AVM</th>
                      <th className="col-num" title="Alivoimasyötöt (Shorthanded Assists)">AVS</th>
                    </tr>
                  </thead>
                  <tbody>
                    {seasonStats.map((stat) => (
                      <tr key={stat.id}>
                        <td className="col-season">{stat.seasonName}</td>
                        <td className="col-team">
                          <div className="team-cell">
                            {stat.teamLogo && (
                              <img src={stat.teamLogo} alt={stat.teamName} className="team-logo-small" />
                            )}
                            <Link to={`/team/${slugify(stat.teamName)}`} className="team-link">{stat.teamName}</Link>
                          </div>
                        </td>
                        <td className="col-num">{stat.gamesPlayed}</td>
                        <td className="col-num">{stat.goals}</td>
                        <td className="col-num">{stat.assists}</td>
                        <td className="col-num">{stat.points}</td>
                        <td className="col-num">{stat.penaltyMinutes}</td>
                        <td className="col-num">{stat.plusMinusRating}</td>
                        <td className="col-num">{stat.powerPlayGoals}</td>
                        <td className="col-num">{stat.powerPlayAssists}</td>
                        <td className="col-num">{stat.shortHandedGoals}</td>
                        <td className="col-num">{stat.shortHandedAssists}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="totals-row">
                      <td>Ura yhteensä</td>
                      <td></td>
                      <td className="col-num">{totals.gamesPlayed}</td>
                      <td className="col-num">{totals.goals}</td>
                      <td className="col-num">{totals.assists}</td>
                      <td className="col-num">{totals.points}</td>
                      <td className="col-num">{totals.penaltyMinutes}</td>
                      <td className="col-num">{totals.plusMinusRating}</td>
                      <td className="col-num">{totals.powerPlayGoals}</td>
                      <td className="col-num">{totals.powerPlayAssists}</td>
                      <td className="col-num">{totals.shortHandedGoals}</td>
                      <td className="col-num">{totals.shortHandedAssists}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            ) : (
              <p className="no-data-message">Ei kausitilastoja saatavilla.</p>
            )}
          </div>
        </div>

        {/* Maalivahtitilastot kausittain (Goalie Season Statistics) */}
        {goalieStats.length > 0 && (
          <div className="player-container">
            <div className="section-block">
              <h3>Maalivahtitilastot kausittain</h3>
              <div className="stats-table-scroll">
                <table className="stats-table">
                  <thead>
                    <tr>
                      <th className="col-season">Kausi</th>
                      <th className="col-team">Joukkue</th>
                      <th className="col-num" title="Pelatut ottelut (Games Played)">O</th>
                      <th className="col-num" title="Voitot (Wins)">V</th>
                      <th className="col-num" title="Tappiot (Losses)">H</th>
                      <th className="col-num" title="Tasapelit (Ties)">T</th>
                      <th className="col-num" title="Torjunnat (Saves)">TO</th>
                      <th className="col-num" title="Laukauksia vastaan (Shots Against)">LA</th>
                      <th className="col-num" title="Torjuntaprosentti (Save Percentage)">TO%</th>
                      <th className="col-num" title="Päästetyt maalit (Goals Against)">PM</th>
                      <th className="col-num" title="Nollapelit (Shutouts)">NP</th>
                      <th className="col-num" title="Peliminuutit (Minutes Played)">MIN</th>
                    </tr>
                  </thead>
                  <tbody>
                    {goalieStats.map((stat) => (
                      <tr key={stat.id}>
                        <td className="col-season">{stat.seasonName}</td>
                        <td className="col-team">
                          <div className="team-cell">
                            <Link to={`/team/${slugify(stat.teamName)}`} className="team-link">{stat.teamName}</Link>
                          </div>
                        </td>
                        <td className="col-num">{stat.gamesPlayed}</td>
                        <td className="col-num">{stat.wins}</td>
                        <td className="col-num">{stat.losses}</td>
                        <td className="col-num">{stat.ties}</td>
                        <td className="col-num">{stat.saves}</td>
                        <td className="col-num">{stat.shotsAgainst}</td>
                        <td className="col-num">{stat.savePercentage.toFixed(1)}%</td>
                        <td className="col-num">{stat.goalsAgainst}</td>
                        <td className="col-num">{stat.shutouts}</td>
                        <td className="col-num">{stat.minutesPlayed}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="totals-row">
                      <td>Ura yhteensä</td>
                      <td></td>
                      <td className="col-num">{goalieTotals.gamesPlayed}</td>
                      <td className="col-num">{goalieTotals.wins}</td>
                      <td className="col-num">{goalieTotals.losses}</td>
                      <td className="col-num">{goalieTotals.ties}</td>
                      <td className="col-num">{goalieTotals.saves}</td>
                      <td className="col-num">{goalieTotals.shotsAgainst}</td>
                      <td className="col-num">{calculateOverallSavePercentage(goalieTotals).toFixed(1)}%</td>
                      <td className="col-num">{goalieTotals.goalsAgainst}</td>
                      <td className="col-num">{goalieTotals.shutouts}</td>
                      <td className="col-num">{goalieTotals.minutesPlayed}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>
        )}

        {/* Pelaajaura (Career Timeline) */}
        <div className="player-container">
          <div className="section-block">
            <h3>Pelaajaura</h3>
            {seasonStats.length > 0 ? (
              <div className="stats-table-scroll">
                <table className="stats-table career-timeline-table">
                  <thead>
                    <tr>
                      <th className="col-season">Kausi</th>
                      <th className="col-team">Joukkue</th>
                      <th className="col-position">Pelipaikka</th>
                    </tr>
                  </thead>
                  <tbody>
                    {seasonStats.map((stat) => (
                      <tr key={stat.id}>
                        <td className="col-season">
                          <Link to={`/league/${stat.seasonId}`} className="team-link">{stat.seasonName}</Link>
                        </td>
                        <td className="col-team">
                          <div className="team-cell">
                            {stat.teamLogo && (
                              <img src={stat.teamLogo} alt={stat.teamName} className="team-logo-small" />
                            )}
                            <Link to={`/team/${slugify(stat.teamName)}`} className="team-link">{stat.teamName}</Link>
                          </div>
                        </td>
                        <td className="col-position">{getPositionText(position)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="no-data-message">Ei uratietoja saatavilla.</p>
            )}
          </div>
        </div>
      </div>
    </PageTemplate>
  );
};

export default FloorballTeamPlayerUserPage;
