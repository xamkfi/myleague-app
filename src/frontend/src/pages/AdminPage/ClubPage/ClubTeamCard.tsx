import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  getTeamEditPath,
  getTeamRosterPath,
  loadClubTeamCompetitions,
  loadCompetitionRosterCount,
  type ClubTeamCompetition,
  type TeamSport,
} from './clubTeamCompetitions';

export interface ClubTeamCardData {
  id: string;
  sport: TeamSport;
  name: string;
  divisionId?: string | null;
  homeArena: string;
  rosterCount: number;
  logoUrl?: string;
}

interface ClubTeamCardProps {
  team: ClubTeamCardData;
  divisionName: string;
  isExpanded: boolean;
  onToggle: () => void;
}

function sportLabelKey(sport: TeamSport): string {
  if (sport === 'football') {
    return 'clubAdmin.sportFootball';
  }
  if (sport === 'hockey') {
    return 'clubAdmin.sportHockey';
  }
  return 'clubAdmin.sportFloorball';
}

function ClubTeamCard({ team, divisionName, isExpanded, onToggle }: ClubTeamCardProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [competitions, setCompetitions] = useState<ClubTeamCompetition[]>([]);
  const [playerCounts, setPlayerCounts] = useState<Record<string, number>>({});
  const [loading, setLoading] = useState(true);
  const [hasLoaded, setHasLoaded] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isExpanded) {
      return;
    }

    let cancelled = false;

    const load = async () => {
      setLoading(true);
      setError(null);

      try {
        const rows = await loadClubTeamCompetitions(team.sport, team.id);
        if (cancelled) {
          return;
        }

        setCompetitions(rows);

        const counts = await Promise.all(
          rows.map(async (row) => {
            try {
              const count = await loadCompetitionRosterCount(team.sport, team.id, row.id);
              return [row.id, count] as const;
            } catch {
              return [row.id, 0] as const;
            }
          }),
        );

        if (!cancelled) {
          setPlayerCounts(Object.fromEntries(counts));
          setHasLoaded(true);
        }
      } catch (err: unknown) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : String(err));
          setHasLoaded(true);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void load();

    return () => {
      cancelled = true;
    };
  }, [isExpanded, team.id, team.sport]);

  return (
    <article className={`club-team-card ${isExpanded ? 'club-team-card--expanded' : ''}`}>
      <button
        type="button"
        className="club-team-card__header"
        onClick={onToggle}
        aria-expanded={isExpanded}
      >
        <div className="club-team-card__top">
          <div className="club-team-card__logo">
            {team.logoUrl ? (
              <img src={team.logoUrl} alt="" />
            ) : (
              <span aria-hidden="true">{team.name.charAt(0)}</span>
            )}
          </div>
          <div className="club-team-card__title">
            <div className="team-name">{team.name}</div>
            {team.homeArena && <div className="team-sub">{team.homeArena}</div>}
          </div>
          <span className="club-team-card__chevron" aria-hidden="true">
            {isExpanded ? '▲' : '▼'}
          </span>
        </div>
        <div className="team-meta">
          <span className={`chip chip--${team.sport}`}>
            {t(sportLabelKey(team.sport))}
          </span>
          {divisionName && <span className="chip">{divisionName}</span>}
          <span className="chip">
            {t('clubs.details.members')}: {team.rosterCount}
          </span>
        </div>
      </button>

      {isExpanded && (
        <div className="club-team-card__body">
          <div className="club-team-card__actions">
            <button
              type="button"
              className="club-team-card__edit-link"
              onClick={() => navigate(getTeamEditPath(team.sport, team.id))}
            >
              {t('clubs.details.editTeam')}
            </button>
          </div>

          <h4 className="club-team-card__competitions-title">
            {t('clubs.details.competitions')}
          </h4>

          {(loading || !hasLoaded) && (
            <p className="club-team-card__status">{t('common.loading')}</p>
          )}

          {error && <p className="club-team-card__status">{error}</p>}

          {hasLoaded && !loading && !error && competitions.length === 0 && (
            <p className="empty">{t('clubs.details.noCompetitions')}</p>
          )}

          {!loading && competitions.length > 0 && (
            <ul className="club-team-card__competitions">
              {competitions.map((row) => (
                <li key={row.id}>
                  <button
                    type="button"
                    className="club-team-card__competition"
                    onClick={() => navigate(getTeamRosterPath(team.sport, team.id, row.id))}
                  >
                    <span className="club-team-card__competition-name">{row.name}</span>
                    <span className="club-team-card__competition-meta">
                      <span className="chip">
                        {row.kind === 'tournament'
                          ? t('clubs.details.tournament')
                          : t('clubs.details.season')}
                      </span>
                      <span className="chip">
                        {t('clubs.details.members')}: {playerCounts[row.id] ?? '…'}
                      </span>
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </article>
  );
}

export default ClubTeamCard;
