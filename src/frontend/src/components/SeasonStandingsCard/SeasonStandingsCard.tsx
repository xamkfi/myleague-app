import { Link } from 'react-router-dom';
import { TeamLink } from '../SportLinks';
import { getLeaguePath, type SportKind } from '../../utils/sportRoutes';
import './SeasonStandingsCard.scss';

export interface SeasonStandingsRow {
  teamId: string;
  teamName: string;
  teamLogo?: string | null;
  goalDifference: number;
  points: number;
}

export interface SeasonStandingsCardLabels {
  standingsTitle: string;
  teamShort: string;
  gdShort: string;
  ptsShort: string;
  noStandings: string;
  viewFullTable: string;
}

export interface SeasonStandingsNavLink {
  tab: string;
  label: string;
}

interface SeasonStandingsCardProps {
  sport: SportKind;
  seasonId: string;
  seasonName: string;
  standings: SeasonStandingsRow[];
  standingsLoading: boolean;
  isDark?: boolean;
  maxRows?: number;
  labels: SeasonStandingsCardLabels;
  navLinks?: SeasonStandingsNavLink[];
}

function TeamLogo({ logo }: { logo?: string | null }) {
  return logo && logo.trim() !== '' ? (
    <img
      className="fb-team-logo"
      src={logo}
      alt=""
      onError={(e) => {
        (e.target as HTMLImageElement).style.visibility = 'hidden';
      }}
    />
  ) : (
    <span className="fb-team-logo fb-team-logo--empty" aria-hidden="true" />
  );
}

export default function SeasonStandingsCard({
  sport,
  seasonId,
  seasonName,
  standings,
  standingsLoading,
  isDark = false,
  maxRows = 10,
  labels,
  navLinks,
}: SeasonStandingsCardProps) {
  const displayStandings = standings.slice(0, maxRows);
  const namedTeams = standings.map((row) => ({ id: row.teamId, name: row.teamName }));
  const defaultNav: SeasonStandingsNavLink[] = [
    { tab: 'fixtures', label: labels.viewFullTable },
  ];
  const links = navLinks ?? defaultNav;

  return (
    <section className={`fb-standings-card${isDark ? ' fb-standings-card--dark' : ''}`}>
      <h2 className="fb-standings-card__title">{seasonName}</h2>

      {isDark && (
        <nav className="fb-standings-card__links" aria-label={seasonName}>
          {links.map((link) => (
            <Link key={link.tab} to={getLeaguePath(sport, seasonId, link.tab)}>
              {link.label}
            </Link>
          ))}
        </nav>
      )}

      <span className="fb-standings-card__label">{labels.standingsTitle}</span>

      {standingsLoading ? (
        <div className="fb-standings-card__skeleton" aria-hidden="true">
          {Array.from({ length: 5 }).map((_, index) => (
            <div key={index} className="fb-standings-card__skeleton-row" />
          ))}
        </div>
      ) : displayStandings.length === 0 ? (
        <p className="fb-standings-card__empty">{labels.noStandings}</p>
      ) : (
        <div className="fb-standings-table">
          <div className="fb-standings-table__head">
            <span className="fb-standings-table__rank">#</span>
            <span className="fb-standings-table__team">{labels.teamShort}</span>
            <span className="fb-standings-table__num">{labels.gdShort}</span>
            <span className="fb-standings-table__num fb-standings-table__num--pts">
              {labels.ptsShort}
            </span>
          </div>
          {displayStandings.map((team, index) => (
            <div key={team.teamId} className="fb-standings-table__row">
              <span className="fb-standings-table__rank">{index + 1}.</span>
              <span className="fb-standings-table__team">
                <TeamLogo logo={team.teamLogo} />
                <TeamLink
                  sport={sport}
                  teamId={team.teamId}
                  teamName={team.teamName}
                  teams={namedTeams}
                  className="fb-standings-table__team-name"
                />
              </span>
              <span className="fb-standings-table__num">{team.goalDifference}</span>
              <span className="fb-standings-table__num fb-standings-table__num--pts">
                {team.points}
              </span>
            </div>
          ))}
        </div>
      )}

      <Link to={getLeaguePath(sport, seasonId, 'statistics')} className="fb-standings-card__full-link">
        {labels.viewFullTable}
      </Link>
    </section>
  );
}
