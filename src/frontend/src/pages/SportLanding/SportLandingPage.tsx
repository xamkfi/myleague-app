import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import Pagination from '../../components/Pagination';
import SeasonStandingsCard, {
  type SeasonStandingsNavLink,
  type SeasonStandingsRow,
} from '../../components/SeasonStandingsCard/SeasonStandingsCard';
import SeasonInfoCards from '../../components/SeasonInfoCards/SeasonInfoCards';
import { TeamLink } from '../../components/SportLinks';
import type { SeasonContentBlockDto } from '../../types/common/seasonContent';
import { getMatchPath, type SportKind } from '../../utils/sportRoutes';
import { formatMatchDateTime } from '../../utils/helpers';
import { formatSeasonYearLabel } from '../../utils/seasonYear';
import './SportLanding.scss';

export interface SportLandingYear {
  year: string;
  hasActiveSeason: boolean;
}

export interface SportLandingSeason {
  id: string;
  name: string;
  isActive: boolean;
}

export interface SportLandingUpcomingMatch {
  id: string;
  scheduledDateTime: string;
  homeTeamId?: string | null;
  awayTeamId?: string | null;
  homeTeamName?: string | null;
  awayTeamName?: string | null;
  homeTeamLogo?: string | null;
  awayTeamLogo?: string | null;
}

export interface SportLandingSeasonData {
  season: SportLandingSeason;
  standings: SeasonStandingsRow[];
  standingsLoading: boolean;
}

export interface SportLandingLabels {
  loading: string;
  error: string;
  retry: string;
  noSeasonsForYear: string;
  seasonLabel: string;
  seasonYears: string;
  standingsTitle: string;
  teamShort: string;
  gdShort: string;
  ptsShort: string;
  noStandings: string;
  viewFullTable: string;
  upcomingMatches: string;
  tbd: string;
  archiveTitle: string;
  archiveText: string;
  backToCurrent: string;
  fixtures: string;
  results: string;
  statistics: string;
  summary: string;
}

const PAGE_SIZE = 6;
const MAX_STANDINGS_PREVIEW = 10;

interface SportLandingPageProps {
  sport: SportKind;
  title: string;
  bannerImage: string;
  labels: SportLandingLabels;
  extraNavLinks?: SeasonStandingsNavLink[];
  years: SportLandingYear[];
  selectedYear: string;
  currentYear: string;
  isCurrentSeasonView: boolean;
  onYearSelect: (year: string) => void;
  seasonsData: SportLandingSeasonData[];
  upcomingMatches: SportLandingUpcomingMatch[];
  contentBlocks: SeasonContentBlockDto[];
  fallbackInfo?: ReactNode;
  isLoadingYears: boolean;
  isLoadingSeasons: boolean;
  error: string | null;
  onRetry: () => void;
  currentPage: number;
  totalPages: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

function TeamLogo({ logo }: { logo?: string | null }) {
  return logo && logo.trim() !== '' ? (
    <img
      className="fb-team-logo"
      src={logo}
      alt=""
      onError={(event) => {
        (event.target as HTMLImageElement).style.visibility = 'hidden';
      }}
    />
  ) : (
    <span className="fb-team-logo fb-team-logo--empty" aria-hidden="true" />
  );
}

export default function SportLandingPage({
  sport,
  title,
  bannerImage,
  labels,
  extraNavLinks,
  years,
  selectedYear,
  currentYear,
  isCurrentSeasonView,
  onYearSelect,
  seasonsData,
  upcomingMatches,
  contentBlocks,
  fallbackInfo,
  isLoadingYears,
  isLoadingSeasons,
  error,
  onRetry,
  currentPage,
  totalPages,
  totalCount,
  onPageChange,
}: SportLandingPageProps) {
  const defaultNav: SeasonStandingsNavLink[] = [
    { tab: 'fixtures', label: labels.fixtures },
    { tab: 'results', label: labels.results },
    { tab: 'statistics', label: labels.statistics },
    { tab: 'summary', label: labels.summary },
  ];
  const navLinks = extraNavLinks ? [...defaultNav, ...extraNavLinks] : defaultNav;

  const renderStandingsCard = (data: SportLandingSeasonData, isDark: boolean) => (
    <SeasonStandingsCard
      key={data.season.id}
      sport={sport}
      seasonId={data.season.id}
      seasonName={data.season.name}
      standings={data.standings}
      standingsLoading={data.standingsLoading}
      isDark={isDark}
      maxRows={MAX_STANDINGS_PREVIEW}
      labels={{
        standingsTitle: labels.standingsTitle,
        teamShort: labels.teamShort,
        gdShort: labels.gdShort,
        ptsShort: labels.ptsShort,
        noStandings: labels.noStandings,
        viewFullTable: labels.viewFullTable,
      }}
      navLinks={navLinks}
    />
  );

  const renderUpcomingMatchesCard = () => {
    if (upcomingMatches.length === 0) {
      return null;
    }

    return (
      <section className="fb-upcoming-card">
        <h2 className="fb-upcoming-card__title">{labels.upcomingMatches}</h2>
        <div className="fb-upcoming-card__list">
          {upcomingMatches.map((match) => {
            const [date, time] = formatMatchDateTime(match.scheduledDateTime);
            return (
              <Link
                key={match.id}
                to={getMatchPath(sport, match.id)}
                className="fb-upcoming-card__row"
              >
                <span className="fb-upcoming-card__datetime">
                  <span>{date}</span>
                  <span>{time}</span>
                </span>
                <span className="fb-upcoming-card__teams">
                  <span className="fb-upcoming-card__team">
                    <TeamLogo logo={match.homeTeamLogo} />
                    {match.homeTeamId && match.homeTeamName ? (
                      <TeamLink
                        sport={sport}
                        teamId={match.homeTeamId}
                        teamName={match.homeTeamName}
                      />
                    ) : (
                      <span>{match.homeTeamName ?? labels.tbd}</span>
                    )}
                  </span>
                  <span className="fb-upcoming-card__team">
                    <TeamLogo logo={match.awayTeamLogo} />
                    {match.awayTeamId && match.awayTeamName ? (
                      <TeamLink
                        sport={sport}
                        teamId={match.awayTeamId}
                        teamName={match.awayTeamName}
                      />
                    ) : (
                      <span>{match.awayTeamName ?? labels.tbd}</span>
                    )}
                  </span>
                </span>
              </Link>
            );
          })}
        </div>
      </section>
    );
  };

  const renderInfoCards = () => {
    if (contentBlocks.length > 0) {
      return <SeasonInfoCards blocks={contentBlocks} className="season-info-cards" />;
    }

    if (!isCurrentSeasonView) {
      return (
        <article className="fb-info-card">
          <h2 className="fb-info-card__title">{labels.archiveTitle}</h2>
          <p>{labels.archiveText}</p>
          {currentYear && (
            <p>
              <button
                type="button"
                className="fb-info-card__link-button"
                onClick={() => onYearSelect(currentYear)}
              >
                {labels.backToCurrent}
              </button>
            </p>
          )}
        </article>
      );
    }

    return fallbackInfo ?? null;
  };

  if (isLoadingYears) {
    return (
      <PageTemplate title={title} fullBleed>
        <div className="sport-landing">
          <div className="sport-landing__state">
            <LoadingSpinner variant="light" text={labels.loading} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={title} fullBleed>
      <div className="sport-landing">
        <header className="fb-banner">
          <img className="fb-banner__image" src={bannerImage} alt="" aria-hidden="true" />
          <div className="fb-banner__content">
            <h1 className="fb-banner__title">{title}</h1>
            {years.length > 0 && (
              <div className="fb-banner__nav">
                <label className="fb-banner__select-wrap">
                  <span className="fb-banner__select-label">{labels.seasonLabel}</span>
                  <select
                    className="fb-banner__select"
                    value={selectedYear}
                    onChange={(event) => onYearSelect(event.target.value)}
                    aria-label={labels.seasonYears}
                  >
                    {years.map((year) => (
                      <option key={year.year} value={year.year}>
                        {formatSeasonYearLabel(year.year)}
                      </option>
                    ))}
                  </select>
                  <span className="fb-banner__select-chevron" aria-hidden="true" />
                </label>
              </div>
            )}
          </div>
        </header>

        <div className="fb-content">
          <div className="fb-container">
            {error ? (
              <div className="sport-landing__state">
                <p>{error}</p>
                <button type="button" className="fb-retry-btn" onClick={onRetry}>
                  {labels.retry}
                </button>
              </div>
            ) : (
              <div className="fb-columns">
                <div className="fb-columns__main">{renderInfoCards()}</div>
                <aside className="fb-columns__side">
                  {isLoadingSeasons && seasonsData.length === 0 ? (
                    <div className="sport-landing__state sport-landing__state--inline">
                      <LoadingSpinner variant="dark" text={labels.loading} />
                    </div>
                  ) : seasonsData.length === 0 ? (
                    <div className="fb-info-card">
                      <p>{labels.noSeasonsForYear}</p>
                    </div>
                  ) : (
                    (() => {
                      const ordered = [
                        ...seasonsData.filter((item) => item.season.isActive),
                        ...seasonsData.filter((item) => !item.season.isActive),
                      ];
                      const darkCount =
                        ordered.filter((item) => item.season.isActive).length
                        || (isCurrentSeasonView ? 1 : 0);
                      return (
                        <>
                          {ordered.slice(0, darkCount).map((item) => renderStandingsCard(item, true))}
                          {renderUpcomingMatchesCard()}
                          {ordered.slice(darkCount).map((item) => renderStandingsCard(item, false))}
                        </>
                      );
                    })()
                  )}

                  {totalPages > 1 && (
                    <div className="fb-pagination">
                      <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        totalCount={totalCount}
                        pageSize={PAGE_SIZE}
                        onPageChange={onPageChange}
                        onPageSizeChange={() => undefined}
                        showPageSizeSelector={false}
                        showSummary={false}
                      />
                    </div>
                  )}
                </aside>
              </div>
            )}
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}

export { PAGE_SIZE };
