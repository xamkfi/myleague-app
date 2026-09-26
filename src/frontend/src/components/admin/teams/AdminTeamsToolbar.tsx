import { useTranslation } from 'react-i18next';
import TeamCategoryFilter from '../../TeamCategoryFilter/TeamCategoryFilter';
import { ALL_TEAMS_SEASON, type AdminSeasonChoice, type NamedOption } from './adminTeamListTypes';
import './AdminTeamsToolbar.scss';

interface AdminTeamsToolbarProps {
  totalCount: number;
  seasonId: string | null;
  seasons: AdminSeasonChoice[];
  divisionId: string;
  divisions: NamedOption[];
  clubId: string;
  clubs: NamedOption[];
  searchTerm: string;
  categories: string[];
  onSeasonChange: (seasonId: string) => void;
  onDivisionChange: (divisionId: string) => void;
  onClubChange: (clubId: string) => void;
  onSearchChange: (value: string) => void;
  onCategoriesChange: (categories: string[]) => void;
  onCreateTeam: () => void;
  onOpenSeason?: () => void;
}

function AdminTeamsToolbar({
  totalCount,
  seasonId,
  seasons,
  divisionId,
  divisions,
  clubId,
  clubs,
  searchTerm,
  categories,
  onSeasonChange,
  onDivisionChange,
  onClubChange,
  onSearchChange,
  onCategoriesChange,
  onCreateTeam,
  onOpenSeason,
}: AdminTeamsToolbarProps) {
  const { t } = useTranslation();
  const selectedSeason = seasons.find((season) => season.id === seasonId) ?? null;
  const seasonMode = seasonId !== null && seasonId !== ALL_TEAMS_SEASON && selectedSeason !== null;
  const countLabel = seasonMode
    ? t('admin.teams.countInSeason', { count: totalCount, season: selectedSeason.name })
    : t('admin.teams.totalCount', { count: totalCount });

  return (
    <>
      <div className="admin-teams-toolbar__header">
        <div className="admin-teams-toolbar__count">{countLabel}</div>
        <div className="admin-teams-toolbar__actions">
          {seasonMode && onOpenSeason && (
            <button type="button" className="admin-teams-toolbar__secondary" onClick={onOpenSeason}>
              {t('admin.teams.openSeason')}
            </button>
          )}
          <button type="button" className="admin-teams-toolbar__primary" onClick={onCreateTeam}>
            {t('admin.teams.createNew')}
          </button>
        </div>
      </div>

      <div className="admin-teams-toolbar__filters">
        <div className="admin-teams-toolbar__row">
          <label className="admin-teams-toolbar__field">
            <span>{t('admin.teams.season')}</span>
            <select
              value={seasonId ?? ''}
              onChange={(event) => onSeasonChange(event.target.value)}
            >
              <option value="" disabled>
                {t('admin.teams.season')}
              </option>
              {seasons.map((season) => (
                <option key={season.id} value={season.id}>
                  {season.isActive ? `${season.name} (${t('admin.teams.active')})` : season.name}
                </option>
              ))}
              <option value={ALL_TEAMS_SEASON}>{t('admin.teams.allTeams')}</option>
            </select>
          </label>

          <label className="admin-teams-toolbar__field">
            <span>{seasonMode ? t('admin.teams.division') : t('admin.teams.homeDivision')}</span>
            <select value={divisionId} onChange={(event) => onDivisionChange(event.target.value)}>
              <option value="">{t('common.all', 'All')}</option>
              {divisions.map((division) => (
                <option key={division.id} value={division.id}>
                  {division.name}
                </option>
              ))}
            </select>
          </label>

          <label className="admin-teams-toolbar__field">
            <span>{t('admin.teams.club')}</span>
            <select value={clubId} onChange={(event) => onClubChange(event.target.value)}>
              <option value="">{t('common.all', 'All')}</option>
              {clubs.map((club) => (
                <option key={club.id} value={club.id}>
                  {club.name}
                </option>
              ))}
            </select>
          </label>

          <label className="admin-teams-toolbar__field admin-teams-toolbar__field--search">
            <span>{t('admin.teams.searchPlaceholder')}</span>
            <input
              type="search"
              value={searchTerm}
              onChange={(event) => onSearchChange(event.target.value)}
              placeholder={t('admin.teams.searchPlaceholder')}
            />
          </label>
        </div>

        <TeamCategoryFilter selected={categories} onChange={onCategoriesChange} />
      </div>
    </>
  );
}

export default AdminTeamsToolbar;
