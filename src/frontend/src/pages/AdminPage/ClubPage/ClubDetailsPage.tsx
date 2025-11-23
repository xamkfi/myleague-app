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
        const [clubData, teamsResp] = await Promise.all([
          clubService.getById(id),
          floorballTeamService.getAll({ clubId: id, pageSize: 1000 })
        ]);
        setClub(clubData);
        const list = Array.isArray(teamsResp?.data) ? teamsResp.data : [];
        setTeams(list.map((t) => ({
          id: t.id,
          name: t.name,
          divisionId: t.divisionId,
          homeArena: t.homeArena
        })));
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

  return (
    <AdminPageTemplate title={t('clubs.details.title', 'Club Details')}>
      <div className="club-details-page">
        <BackButton to="/admin/clubs" text={t('common.back', 'Back')} />
        <ErrorPopup message={error} />
        {loading && <p>{t('common.loading', 'Loading...')}</p>}
        {!loading && club && (
          <>
            <div className="club-header">
              <div>
                <h2 className="club-name">{club.name}</h2>
                <p className="club-location">
                  {club.city}, {club.country}
                </p>
              </div>
              <div className="header-actions">
                <button
                  className="btn btn-secondary"
                  onClick={() => navigate(`/admin/clubs/${club.id}/edit`)}
                >
                  {t('common.edit', 'Edit')}
                </button>
              </div>
            </div>

            <div className="club-info">
              <div className="info-item">
                <span className="label">{t('clubs.details.foundingDate', 'Founding Date')}</span>
                <span className="value">
                  {new Date(club.foundingDate).toLocaleDateString()}
                </span>
              </div>
              <div className="info-item">
                <span className="label">{t('clubs.form.websiteUrl', 'Website URL')}</span>
                <span className="value">
                  {club.websiteUrl ? (
                    <a href={club.websiteUrl} target="_blank" rel="noreferrer">
                      {club.websiteUrl}
                    </a>
                  ) : (
                    '-'
                  )}
                </span>
              </div>
              <div className="info-item">
                <span className="label">{t('clubs.form.contactEmail', 'Contact Email')}</span>
                <span className="value">{club.contactEmail || '-'}</span>
              </div>
            </div>

            <div className="teams-section">
              <h3>{t('clubs.details.teams', 'Teams')}</h3>
              {teams.length === 0 ? (
                <p className="empty">{t('clubs.details.noTeams', 'No teams yet')}</p>
              ) : (
                <div className="team-cards">
                  {teams.map((team) => (
                    <button
                      key={team.id}
                      className="team-card"
                      onClick={() => navigate(`/admin/floorball/teams/${team.id}/edit`)}
                    >
                      <div className="team-name">{team.name}</div>
                      <div className="team-meta">
                        <span className="division">{getDivisionName(team.divisionId)}</span>
                        <span className="arena">{team.homeArena}</span>
                      </div>
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


