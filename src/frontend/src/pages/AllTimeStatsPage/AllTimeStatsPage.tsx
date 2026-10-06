import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import Pagination from '../../components/Pagination';
import SportIcon from '../../components/SportIcon/SportIcon';
import { useAudience } from '../../context/AudienceContext';
import { TeamCategory } from '../../types/floorball/floorballTypes';
import {
  getAllTimePlayerStatistics,
  getAllTimeTeams,
  type AllTimeCompetitionFilter,
  type AllTimePlayerStatisticsDto,
  type AllTimeSortDirection,
  type AllTimeStatSort,
  type AllTimeTeamOptionDto,
} from '../../api/allTimeStatistics';
import {
  ALL_TIME_STATS_HUB_PATH,
  getPlayerPath,
  type SportKind,
} from '../../utils/sportRoutes';
import bannerImage from '../../assets/floorball-banner.png';
import '../SportLanding/SportLanding.scss';
import './AllTimeStatsPage.scss';

const PAGE_SIZE = 25;
const SKELETON_ROWS = 8;
const SEARCH_DEBOUNCE_MS = 300;
const TEAM_PARAM = 'team';

interface AllTimeStatsPageProps {
  sport: SportKind;
}

interface SortColumn {
  sort: AllTimeStatSort;
  labelKey: string;
  titleKey: string;
}

const sharedColumns: SortColumn[] = [
  { sort: 'Games', labelKey: 'allTimeStats.gamesShort', titleKey: 'allTimeStats.games' },
  { sort: 'Goals', labelKey: 'allTimeStats.goalsShort', titleKey: 'allTimeStats.goals' },
  { sort: 'Assists', labelKey: 'allTimeStats.assistsShort', titleKey: 'allTimeStats.assists' },
  { sort: 'Points', labelKey: 'allTimeStats.pointsShort', titleKey: 'allTimeStats.points' },
];

const penaltyColumn: SortColumn = {
  sort: 'Penalties',
  labelKey: 'allTimeStats.penaltiesShort',
  titleKey: 'allTimeStats.penalties',
};

const cardColumns: SortColumn[] = [
  { sort: 'YellowCards', labelKey: 'allTimeStats.yellowShort', titleKey: 'allTimeStats.yellowCards' },
  { sort: 'RedCards', labelKey: 'allTimeStats.redShort', titleKey: 'allTimeStats.redCards' },
];

function columnsFor(sport: SportKind): SortColumn[] {
  if (sport === 'football') {
    return [...sharedColumns, ...cardColumns];
  }
  return [...sharedColumns, penaltyColumn];
}

function cellValue(player: AllTimePlayerStatisticsDto, sort: AllTimeStatSort): number {
  switch (sort) {
    case 'Games':
      return player.gamesPlayed;
    case 'Goals':
      return player.goals;
    case 'Assists':
      return player.assists;
    case 'Points':
      return player.points;
    case 'Penalties':
      return player.penaltyMinutes ?? 0;
    case 'YellowCards':
      return player.yellowCards ?? 0;
    case 'RedCards':
      return player.redCards ?? 0;
  }
}

export default function AllTimeStatsPage({ sport }: AllTimeStatsPageProps) {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [searchParams, setSearchParams] = useSearchParams();
  const teamId = searchParams.get(TEAM_PARAM) ?? '';
  const [teams, setTeams] = useState<AllTimeTeamOptionDto[]>([]);
  const [searchInput, setSearchInput] = useState<string>('');
  const [search, setSearch] = useState<string>('');
  const [teamCategory, setTeamCategory] = useState<TeamCategory>(audience.teamCategory);
  const [competitionType, setCompetitionType] = useState<AllTimeCompetitionFilter>('Season');
  const [sort, setSort] = useState<AllTimeStatSort>('Points');
  const [direction, setDirection] = useState<AllTimeSortDirection>('Desc');
  const [page, setPage] = useState<number>(1);
  const [players, setPlayers] = useState<AllTimePlayerStatisticsDto[]>([]);
  const [totalPages, setTotalPages] = useState<number>(0);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState<number>(0);

  useEffect(() => {
    setTeamCategory(audience.teamCategory);
    setPage(1);
  }, [audience.teamCategory]);

  useEffect(() => {
    const next = searchInput.trim();
    if (next === search) {
      return undefined;
    }
    const timer = window.setTimeout(() => {
      setSearch(next);
      setPage(1);
    }, SEARCH_DEBOUNCE_MS);
    return () => window.clearTimeout(timer);
  }, [searchInput, search]);

  useEffect(() => {
    let cancelled = false;
    getAllTimeTeams(sport, teamCategory, competitionType)
      .then((options) => {
        if (!cancelled) {
          setTeams(options);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setTeams([]);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [sport, teamCategory, competitionType]);

  const changeTeam = (nextTeamId: string) => {
    setPage(1);
    setSearchParams(
      (current) => {
        const next = new URLSearchParams(current);
        if (nextTeamId) {
          next.set(TEAM_PARAM, nextTeamId);
        } else {
          next.delete(TEAM_PARAM);
        }
        return next;
      },
      { replace: true },
    );
  };

  useEffect(() => {
    let cancelled = false;
    setIsLoading(true);
    setError(null);

    getAllTimePlayerStatistics(sport, {
      page,
      pageSize: PAGE_SIZE,
      teamCategory,
      competitionType,
      sort,
      direction,
      search: search || undefined,
      teamId: teamId || undefined,
    })
      .then((response) => {
        if (cancelled) {
          return;
        }
        setPlayers(response.data ?? []);
        setTotalPages(response.pagination?.totalPages ?? 0);
        setTotalCount(response.pagination?.totalCount ?? 0);
      })
      .catch((loadError: unknown) => {
        if (cancelled) {
          return;
        }
        const message = loadError instanceof Error ? loadError.message : t('allTimeStats.error');
        setError(message);
        setPlayers([]);
      })
      .finally(() => {
        if (!cancelled) {
          setIsLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [sport, page, teamCategory, competitionType, sort, direction, search, teamId, reloadToken, t]);

  const columns = columnsFor(sport);
  const title = t('allTimeStats.title');
  const sportName = t(`allTimeStats.sports.${sport}`);
  const activeColumn = columns.find((column) => column.sort === sort);
  const selectedTeam = teams.find((team) => team.teamId === teamId);

  const changeSort = (nextSort: AllTimeStatSort) => {
    setPage(1);
    if (nextSort === sort) {
      setDirection((current) => (current === 'Desc' ? 'Asc' : 'Desc'));
      return;
    }
    setSort(nextSort);
    setDirection('Desc');
  };

  const renderTableBody = () => {
    if (isLoading) {
      return (
        <div className="all-time-stats__skeleton" aria-busy="true" aria-label={t('allTimeStats.loading')}>
          {Array.from({ length: SKELETON_ROWS }, (_, index) => (
            <div key={index} className="all-time-stats__skeleton-row" />
          ))}
        </div>
      );
    }

    if (error) {
      return (
        <div className="all-time-stats__state">
          <p>{error}</p>
          <button type="button" className="fb-retry-btn" onClick={() => setReloadToken((value) => value + 1)}>
            {t('allTimeStats.retry')}
          </button>
        </div>
      );
    }

    if (players.length === 0) {
      return (
        <p className="all-time-stats__state">
          {search ? t('allTimeStats.noSearchResults', { search }) : t('allTimeStats.empty')}
        </p>
      );
    }

    return (
      <div className="all-time-stats__table-wrap">
        <table className="all-time-stats__table">
          <thead>
            <tr>
              <th className="all-time-stats__rank" scope="col">#</th>
              <th className="all-time-stats__player" scope="col">{t('allTimeStats.player')}</th>
              <th className="all-time-stats__team" scope="col">{t('allTimeStats.team')}</th>
              {columns.map((column) => {
                const active = sort === column.sort;
                const columnTitle = t(column.titleKey);
                return (
                  <th
                    key={column.sort}
                    scope="col"
                    className={`all-time-stats__num${active ? ' is-active' : ''}`}
                    aria-sort={active ? (direction === 'Desc' ? 'descending' : 'ascending') : 'none'}
                  >
                    <button
                      type="button"
                      title={columnTitle}
                      aria-label={t('allTimeStats.sortBy', { column: columnTitle })}
                      onClick={() => changeSort(column.sort)}
                    >
                      {t(column.labelKey)}
                      <span className="all-time-stats__sort-icon" aria-hidden="true">
                        {active ? (direction === 'Desc' ? '▼' : '▲') : '↕'}
                      </span>
                    </button>
                  </th>
                );
              })}
            </tr>
          </thead>
          <tbody>
            {players.map((player) => {
              const rank = player.rank;
              const podium = rank <= 3 && direction === 'Desc' ? ` all-time-stats__rank-badge--${rank}` : '';
              return (
                <tr key={player.playerId}>
                  <td className="all-time-stats__rank">
                    <span className={`all-time-stats__rank-badge${podium}`}>{rank}</span>
                  </td>
                  <td className="all-time-stats__player">
                    <Link to={getPlayerPath(sport, player.playerId)}>{player.playerName}</Link>
                    <span className="all-time-stats__player-team">{player.teamName}</span>
                  </td>
                  <td className="all-time-stats__team">{player.teamName}</td>
                  {columns.map((column) => (
                    <td
                      key={column.sort}
                      className={`all-time-stats__num${sort === column.sort ? ' is-active' : ''}`}
                    >
                      {cellValue(player, column.sort)}
                    </td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    );
  };

  return (
    <PageTemplate title={title} fullBleed>
      <div className="sport-landing all-time-stats">
        <section className="fb-banner all-time-stats__banner">
          <img className="fb-banner__image" src={bannerImage} alt="" aria-hidden="true" />
          <div className="fb-banner__content">
            <nav className="all-time-stats__breadcrumb" aria-label={t('allTimeStats.breadcrumb')}>
              <Link to={ALL_TIME_STATS_HUB_PATH}>{title}</Link>
              <span aria-hidden="true">/</span>
              <span aria-current="page">{sportName}</span>
            </nav>
            <div className="fb-banner__title-row">
              <SportIcon sport={sport} size="lg" inverted decorative />
              <h1 className="fb-banner__title">{title}</h1>
            </div>
            <p className="all-time-stats__lead">{t('allTimeStats.lead')}</p>
            <div className="fb-banner__nav">
              <label className="fb-banner__select-wrap">
                <span className="fb-banner__select-label">{t('audience.filterLabel')}</span>
                <select
                  className="fb-banner__select"
                  value={teamCategory}
                  onChange={(event) => {
                    setTeamCategory(event.target.value as TeamCategory);
                    setPage(1);
                  }}
                >
                  <option value={TeamCategory.Adult}>{t('audience.adult')}</option>
                  <option value={TeamCategory.Youth}>{t('audience.youth')}</option>
                  <option value={TeamCategory.Women}>{t('audience.women')}</option>
                </select>
                <span className="fb-banner__select-chevron" aria-hidden="true" />
              </label>
              <label className="fb-banner__select-wrap">
                <span className="fb-banner__select-label">{t('allTimeStats.competitionType')}</span>
                <select
                  className="fb-banner__select"
                  value={competitionType}
                  onChange={(event) => {
                    setCompetitionType(event.target.value as AllTimeCompetitionFilter);
                    setPage(1);
                  }}
                >
                  <option value="Season">{t('allTimeStats.seasons')}</option>
                  <option value="Tournament">{t('allTimeStats.tournaments')}</option>
                  <option value="All">{t('allTimeStats.allCompetitions')}</option>
                </select>
                <span className="fb-banner__select-chevron" aria-hidden="true" />
              </label>
            </div>
          </div>
        </section>

        <div className="fb-content">
          <div className="fb-container">
            <section className="all-time-stats__card">
              <header className="all-time-stats__card-header">
                <h2 className="all-time-stats__card-title">
                  {selectedTeam ? selectedTeam.teamName : activeColumn ? t(activeColumn.titleKey) : title}
                </h2>
                {!isLoading && !error && totalCount > 0 && activeColumn && (
                  <span className="all-time-stats__summary">
                    {t('allTimeStats.summary', { count: totalCount, column: t(activeColumn.titleKey) })}
                  </span>
                )}
              </header>

              <div className="all-time-stats__toolbar">
                <label className="all-time-stats__search">
                  <svg
                    className="all-time-stats__search-icon"
                    viewBox="0 0 24 24"
                    width="18"
                    height="18"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    aria-hidden="true"
                  >
                    <circle cx="11" cy="11" r="7" />
                    <path d="m20 20-3.5-3.5" />
                  </svg>
                  <input
                    type="search"
                    value={searchInput}
                    placeholder={t('allTimeStats.searchPlaceholder')}
                    aria-label={t('allTimeStats.searchLabel')}
                    maxLength={100}
                    onChange={(event) => setSearchInput(event.target.value)}
                  />
                  {searchInput && (
                    <button
                      type="button"
                      className="all-time-stats__search-clear"
                      aria-label={t('allTimeStats.clearSearch')}
                      onClick={() => setSearchInput('')}
                    >
                      ×
                    </button>
                  )}
                </label>
                <label className="all-time-stats__team-filter">
                  <select
                    value={teamId}
                    aria-label={t('allTimeStats.team')}
                    onChange={(event) => changeTeam(event.target.value)}
                  >
                    <option value="">{t('allTimeStats.allTeams')}</option>
                    {teamId && !selectedTeam && <option value={teamId}>{t('allTimeStats.team')}</option>}
                    {teams.map((team) => (
                      <option key={team.teamId} value={team.teamId}>
                        {team.teamName}
                      </option>
                    ))}
                  </select>
                  <span className="all-time-stats__team-chevron" aria-hidden="true" />
                </label>
              </div>
              <p className="all-time-stats__hint">
                {selectedTeam ? t('allTimeStats.teamHint') : t('allTimeStats.searchHint')}
              </p>

              {renderTableBody()}

              {totalPages > 1 && (
                <div className="all-time-stats__pagination">
                  <Pagination
                    currentPage={page}
                    totalPages={totalPages}
                    totalCount={totalCount}
                    pageSize={PAGE_SIZE}
                    onPageChange={setPage}
                    onPageSizeChange={() => undefined}
                    showPageSizeSelector={false}
                  />
                </div>
              )}
            </section>
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}
