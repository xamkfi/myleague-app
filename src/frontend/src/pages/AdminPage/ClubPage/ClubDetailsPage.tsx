import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import { clubService, type Club } from '../../../api/common/clubService';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { footballTeamService } from '../../../api/football/footballTeamService';
import { useDivisions } from '../../../hooks/useDivisions';
import type { ClubAdminUser } from '../../../types/clubAdmin/clubAdminTypes';
import {
  ClubAdminsModal,
  getClubAdminDisplayName,
  type ClubAdminSelection,
} from './ClubAdminsPicker';
import { resolveClubAdminUserIds } from './resolveClubAdminUserIds';
import './ClubDetailsPage.scss';

type TeamSport = 'floorball' | 'football';

interface TeamCardData {
  id: string;
  sport: TeamSport;
  name: string;
  divisionId?: string | null;
  homeArena: string;
  rosterCount: number;
  logoUrl?: string;
}

interface TeamPage<T> {
  data?: T[];
  pagination?: { totalPages?: number };
}

function toAdminSelection(admin: ClubAdminUser): ClubAdminSelection {
  return {
    userId: admin.userId,
    personId: admin.personId,
    firstName: admin.firstName,
    lastName: admin.lastName,
    email: admin.email,
  };
}

function formatDmy(iso?: string | null): string {
  if (!iso) return '—';
  const dt = new Date(iso);
  if (Number.isNaN(dt.getTime())) return '—';
  const yyyy = dt.getUTCFullYear();
  if (yyyy <= 1) return '—';
  const dd = String(dt.getUTCDate()).padStart(2, '0');
  const mm = String(dt.getUTCMonth() + 1).padStart(2, '0');
  return `${dd}.${mm}.${yyyy}`;
}

async function fetchAllPages<T>(
  loadPage: (page: number, pageSize: number) => Promise<TeamPage<T>>,
  pageSize = 100,
): Promise<T[]> {
  const first = await loadPage(1, pageSize);
  const items = [...(first.data ?? [])];
  const totalPages = first.pagination?.totalPages ?? 1;

  if (totalPages > 1) {
    const rest = await Promise.all(
      Array.from({ length: totalPages - 1 }, (_, index) => loadPage(index + 2, pageSize)),
    );
    for (const page of rest) {
      items.push(...(page.data ?? []));
    }
  }

  return items;
}

function ClubDetailsPage() {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisions } = useDivisions();

  const [club, setClub] = useState<Club | null>(null);
  const [teams, setTeams] = useState<TeamCardData[]>([]);
  const [admins, setAdmins] = useState<ClubAdminSelection[]>([]);
  const [loading, setLoading] = useState(true);
  const [savingAdmins, setSavingAdmins] = useState(false);
  const [isAdminsModalOpen, setIsAdminsModalOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const divisionNameById = useMemo(() => {
    const map = new Map<string, string>();
    for (const division of divisions) {
      map.set(division.id, division.name);
    }
    return map;
  }, [divisions]);

  const loadPage = useCallback(async (clubId: string) => {
    setError(null);
    setLoading(true);

    try {
      const [clubData, adminUsers, floorballTeams, footballTeams] = await Promise.all([
        clubService.getById(clubId),
        clubService.getAdmins(clubId),
        fetchAllPages((page, pageSize) =>
          floorballTeamService.getAll({ clubId, page, pageSize })),
        fetchAllPages((page, pageSize) =>
          footballTeamService.getAll({ clubId, page, pageSize })),
      ]);

      setClub(clubData);
      setAdmins(adminUsers.map(toAdminSelection));
      setTeams([
        ...floorballTeams.map((team) => ({
          id: team.id,
          sport: 'floorball' as const,
          name: team.name,
          divisionId: team.divisionId ?? null,
          homeArena: team.homeArena,
          rosterCount: Array.isArray(team.roster) ? team.roster.length : 0,
          logoUrl: team.logoUrl,
        })),
        ...footballTeams.map((team) => ({
          id: team.id,
          sport: 'football' as const,
          name: team.name,
          divisionId: team.divisionId ?? null,
          homeArena: team.homeArena,
          rosterCount: Array.isArray(team.roster) ? team.roster.length : 0,
          logoUrl: team.logoUrl,
        })),
      ]);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!id) return;
    void loadPage(id);
  }, [id, loadPage]);

  const persistAdmins = async (nextAdmins: ClubAdminSelection[]) => {
    if (!id) return;

    const previous = admins;
    setAdmins(nextAdmins);
    setSavingAdmins(true);
    setError(null);

    try {
      const userIds = await resolveClubAdminUserIds(nextAdmins, id);
      await clubService.setAdmins(id, userIds);
      const refreshed = await clubService.getAdmins(id);
      setAdmins(refreshed.map(toAdminSelection));
    } catch (err: unknown) {
      setAdmins(previous);
      setError(
        err instanceof Error
          ? err.message
          : t('clubs.admins.saveError', 'Failed to update club admins'),
      );
    } finally {
      setSavingAdmins(false);
    }
  };

  const getDivisionName = (divisionId?: string | null) =>
    divisionId ? divisionNameById.get(divisionId) ?? '' : '';

  const getTeamEditPath = (team: TeamCardData) =>
    team.sport === 'football'
      ? `/admin/football/teams/${team.id}/edit`
      : `/admin/floorball/teams/${team.id}/edit`;

  return (
    <AdminPageTemplate title={club?.name ?? t('clubs.details.title', 'Club Details')}>
      <div className="club-details-page">
        <Link to="/admin/clubs" className="club-details-back">
          ← {t('clubs.details.backToClubs', 'Back to clubs')}
        </Link>

        <ErrorPopup message={error} />

        {loading && <p className="club-details-loading">{t('common.loading', 'Loading...')}</p>}

        {!loading && club && (
          <>
            <div className="club-details-top">
              <article className="club-card">
                <div className="club-card-header">
                  <div className="club-logo">
                    {club.logoUrl ? (
                      <img src={club.logoUrl} alt="" />
                    ) : (
                      <div className="logo-placeholder" aria-hidden="true">
                        {club.name.charAt(0)}
                      </div>
                    )}
                  </div>
                  <div className="club-identity">
                    <h2 className="club-name">{club.name}</h2>
                    <p className="club-location">
                      {[club.city, club.country].filter(Boolean).join(', ') || '—'}
                    </p>
                  </div>
                  <div className="club-actions">
                    <button
                      type="button"
                      className="btn btn-secondary"
                      onClick={() => navigate(`/admin/clubs/${club.id}/edit`)}
                    >
                      {t('common.edit', 'Edit')}
                    </button>
                  </div>
                </div>
                <div className="club-card-body">
                  <div className="info-row">
                    <span className="label">{t('clubs.details.foundingDate', 'Founding Date')}</span>
                    <span className="value">{formatDmy(club.foundingDate)}</span>
                  </div>
                  <div className="info-row">
                    <span className="label">{t('clubs.form.websiteUrl', 'Website URL')}</span>
                    <span className="value">
                      {club.websiteUrl ? (
                        <a href={club.websiteUrl} target="_blank" rel="noopener noreferrer">
                          {club.websiteUrl}
                        </a>
                      ) : (
                        '—'
                      )}
                    </span>
                  </div>
                  <div className="info-row">
                    <span className="label">{t('clubs.form.contactEmail', 'Contact Email')}</span>
                    <span className="value">
                      {club.contactEmail ? (
                        <a href={`mailto:${club.contactEmail}`}>{club.contactEmail}</a>
                      ) : (
                        '—'
                      )}
                    </span>
                  </div>
                </div>
              </article>

              <article className="club-admins-card">
                <div className="club-admins-card__header">
                  <div>
                    <h3>{t('clubs.admins.title', 'Club admins')}</h3>
                    <p>
                      {t(
                        'clubs.admins.cardHint',
                        'These users can edit club information and manage team rosters.',
                      )}
                    </p>
                  </div>
                  <button
                    type="button"
                    className="btn btn-primary"
                    onClick={() => setIsAdminsModalOpen(true)}
                    disabled={savingAdmins}
                  >
                    {t('clubs.admins.addButton', 'Add club admin')}
                  </button>
                </div>

                {admins.length === 0 ? (
                  <p className="empty">{t('clubs.admins.noneSelected', 'No club admins assigned yet.')}</p>
                ) : (
                  <ul className="club-admins-card__list">
                    {admins.map((admin) => (
                      <li key={admin.personId} className="club-admins-card__item">
                        <div className="club-admins-card__identity">
                          <span className="club-admins-card__name">{getClubAdminDisplayName(admin)}</span>
                          {admin.email && (
                            <a className="club-admins-card__email" href={`mailto:${admin.email}`}>
                              {admin.email}
                            </a>
                          )}
                        </div>
                        <button
                          type="button"
                          className="club-admins-card__remove"
                          onClick={() => void persistAdmins(
                            admins.filter((item) => item.personId !== admin.personId),
                          )}
                          disabled={savingAdmins}
                        >
                          {t('clubs.admins.remove', 'Remove')}
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
              </article>
            </div>

            <section className="teams-section">
              <div className="teams-section__header">
                <h3>{t('clubs.details.teams', 'Teams')}</h3>
                <span className="teams-section__count">{teams.length}</span>
              </div>
              {teams.length === 0 ? (
                <p className="empty">{t('clubs.details.noTeams', 'No teams yet')}</p>
              ) : (
                <div className="club-teams-grid">
                  {teams.map((team) => {
                    const divisionName = getDivisionName(team.divisionId);
                    return (
                      <button
                        key={`${team.sport}-${team.id}`}
                        type="button"
                        className="team-card"
                        onClick={() => navigate(getTeamEditPath(team))}
                      >
                        <div className="team-card__top">
                          <div className="team-card__logo">
                            {team.logoUrl ? (
                              <img src={team.logoUrl} alt="" />
                            ) : (
                              <span aria-hidden="true">{team.name.charAt(0)}</span>
                            )}
                          </div>
                          <div className="team-name">{team.name}</div>
                        </div>
                        <div className="team-meta">
                          <span className={`chip chip--${team.sport}`}>
                            {team.sport === 'football'
                              ? t('clubAdmin.sportFootball', 'Football')
                              : t('clubAdmin.sportFloorball', 'Floorball')}
                          </span>
                          {divisionName && <span className="chip">{divisionName}</span>}
                          <span className="chip">
                            {t('clubs.details.members', 'Members')}: {team.rosterCount}
                          </span>
                        </div>
                        {team.homeArena && <div className="team-sub">{team.homeArena}</div>}
                      </button>
                    );
                  })}
                </div>
              )}
            </section>
          </>
        )}

        {isAdminsModalOpen && (
          <ClubAdminsModal
            selectedAdmins={admins}
            onChange={(nextAdmins) => void persistAdmins(nextAdmins)}
            onClose={() => setIsAdminsModalOpen(false)}
          />
        )}
      </div>
    </AdminPageTemplate>
  );
}

export default ClubDetailsPage;
