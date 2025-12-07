import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import BackButton from '../../../components/BackButton/BackButton';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import { clubService, type Club } from '../../../api/common/clubService';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { useDivisions } from '../../../hooks/useDivisions';
import './ClubDetailsPage.scss';

interface TeamCardData {
  id: string;
  name: string;
  divisionId: string;
  homeArena: string;
  rosterCount: number;
}

function ClubDetailsPage() {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisions } = useDivisions();
  const [club, setClub] = useState<Club | null>(null);
  const [teams, setTeams] = useState<TeamCardData[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      if (!id) return;
      setError(null);
      setLoading(true);
      try {
        const clubData = await clubService.getById(id);
        setClub(clubData);

        // Fetch all teams for the club with paginated requests (backend pageSize cap ~100)
        const pageSize = 100;
        const aggregated: TeamCardData[] = [];

        // First page to obtain totalPages
        const firstResp = await floorballTeamService.getAll({ clubId: id, page: 1, pageSize });
        const firstList = Array.isArray(firstResp?.data) ? firstResp.data : [];
        aggregated.push(
          ...firstList.map((t) => ({
            id: t.id,
            name: t.name,
            divisionId: t.divisionId,
            homeArena: t.homeArena,
            rosterCount: Array.isArray(t.roster) ? t.roster.length : 0
          }))
        );

        const totalPages = firstResp.pagination?.totalPages ?? 1;
        for (let pg = 2; pg <= totalPages; pg++) {
          const resp = await floorballTeamService.getAll({ clubId: id, page: pg, pageSize });
          const list = Array.isArray(resp?.data) ? resp.data : [];
          aggregated.push(
            ...list.map((t) => ({
              id: t.id,
              name: t.name,
              divisionId: t.divisionId,
              homeArena: t.homeArena,
              rosterCount: Array.isArray(t.roster) ? t.roster.length : 0
            }))
          );
        }
        setTeams(aggregated);
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id]);

  const getDivisionName = (divisionId: string) =>
    divisions.find((d) => d.id === divisionId)?.name || '';

  const formatDmy = (iso?: string) => {
    if (!iso) return '-';
    const dt = new Date(iso);
    if (Number.isNaN(dt.getTime())) return '-';
    const dd = String(dt.getUTCDate()).padStart(2, '0');
    const mm = String(dt.getUTCMonth() + 1).padStart(2, '0');
    const yyyy = dt.getUTCFullYear();
    return `${dd}-${mm}-${yyyy}`;
  };

  return (
    <AdminPageTemplate title={t('clubs.details.title', 'Club Details')}>
      <div className="club-details-page">
        <BackButton to="/admin/clubs" text={t('common.back', 'Back')} />
        <ErrorPopup message={error} />
        {loading && <p>{t('common.loading', 'Loading...')}</p>}
        {!loading && club && (
          <>
            {/* Club Card */}
            <div className="club-card">
              <div className="club-card-header">
                <div className="club-logo">
                  {club.logoUrl ? (
                    <img src={club.logoUrl} alt={`${club.name} logo`} />
                  ) : (
                    <div className="logo-placeholder">{club.name.charAt(0)}</div>
                  )}
                </div>
                <div className="club-identity">
                  <h2 className="club-name">{club.name}</h2>
                  <p className="club-location">{club.city}, {club.country}</p>
                </div>
                <div className="club-actions">
                  <button className="btn btn-secondary" onClick={() => navigate(`/admin/clubs/${club.id}/edit`)}>
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
                    {club.websiteUrl ? <a href={club.websiteUrl} target="_blank" rel="noreferrer">{club.websiteUrl}</a> : '-'}
                  </span>
                </div>
                <div className="info-row">
                  <span className="label">{t('clubs.form.contactEmail', 'Contact Email')}</span>
                  <span className="value">{club.contactEmail || '-'}</span>
                </div>
              </div>
            </div>

            <div className="teams-section">
              <h3>{t('clubs.details.teams', 'Teams')}</h3>
              {teams.length === 0 ? (
                <p className="empty">{t('clubs.details.noTeams', 'No teams yet')}</p>
              ) : (
                <div className="club-teams-grid">
                  {teams.map((team) => (
                    <button
                      key={team.id}
                      className="team-card"
                      onClick={() => navigate(`/admin/floorball/teams/${team.id}/edit`)}
                    >
                      <div className="team-name">{team.name}</div>
                      <div className="team-meta">
                        <span className="chip sport">Floorball</span>
                        <span className="chip division">{getDivisionName(team.divisionId)}</span>
                        <span className="chip members">{t('clubs.details.members', 'Members')}: {team.rosterCount}</span>
                      </div>
                      <div className="team-sub">{team.homeArena}</div>
                    </button>
                  ))}
                </div>
              )}
            </div>
          </>
        )}
      </div>
    </AdminPageTemplate>
  );
}

export default ClubDetailsPage;


