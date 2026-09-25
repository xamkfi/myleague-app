import { useEffect, useState, useMemo, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import CatalogPage from '../../components/CatalogPage/CatalogPage';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { TeamLink, type NamedTeam } from '../../components/SportLinks';
import SportIcon from '../../components/SportIcon/SportIcon';
import TeamLogoMark from '../../components/TeamLogoMark/TeamLogoMark';
import type { Club } from '../../api/common/clubService';
import { getClubs } from '../../api/common/clubService';
import { findClubBySlug } from '../../utils/slugUtils';
import { clubEmail, clubFoundingYear, clubPublicUrl, clubText } from '../../utils/clubDisplay';
import { resolveLogoUrl } from '../../utils/resolveLogoUrl';
import type { SportKind } from '../../utils/sportRoutes';
import { useFloorballTeamsData, useFootballTeamsData } from '../../hooks/useTeamsData';
import { floorballSeasonService, type FloorballSeasonDto } from '../../api/floorball/floorballSeasonService';
import { footballSeasonService, type FootballSeasonDto } from '../../api/football/footballSeasonService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import type { HockeySeasonDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { ClubTeamSeasonList, type ClubTeamSeason } from './components/ClubTeamSeasonList';
import { useAudience } from '../../context/AudienceContext';
import './ClubPage.scss';

interface ClubListedTeam {
  id: string;
  name: string;
  logoUrl?: string | null;
}

interface ClubSportBlockProps {
  sport: SportKind;
  title: string;
  teams: ClubListedTeam[];
  loading: boolean;
  seasonsFor: (teamId: string) => ClubTeamSeason[];
}

function ClubSportBlock({ sport, title, teams, loading, seasonsFor }: ClubSportBlockProps) {
  const { t } = useTranslation();
  if (!loading && teams.length === 0) {
    return null;
  }

  const namedTeams: NamedTeam[] = teams.map((team) => ({ id: team.id, name: team.name }));

  return (
    <section className="club-page__sport">
      <h2 className="club-page__sport-title">
        <SportIcon sport={sport} size="sm" decorative />
        <span>{title}</span>
      </h2>
      {loading ? (
        <div className="club-page__teams-loading">
          <LoadingSpinner size="sm" text={t('clubPage.teamsLoading')} />
        </div>
      ) : (
        <div className="club-page__teams-grid">
          {teams.map((team) => {
            const teamSeasons = seasonsFor(team.id);
            return (
              <article key={team.id} className="club-page-team">
                <div className="club-page-team__mark">
                  <TeamLogoMark
                    logo={resolveLogoUrl(team.logoUrl)}
                    name={team.name}
                    imageClassName="club-page-team__logo"
                    fallbackClassName="club-page-team__logo club-page-team__logo--empty"
                  />
                </div>
                <div className="club-page-team__content">
                  <h3 className="club-page-team__name">
                    <TeamLink
                      sport={sport}
                      teamId={team.id}
                      teamName={team.name}
                      teams={namedTeams}
                      seasonId={teamSeasons[0]?.id}
                    >
                      {team.name}
                    </TeamLink>
                  </h3>
                  <ClubTeamSeasonList
                    sport={sport}
                    teamId={team.id}
                    teamName={team.name}
                    teams={namedTeams}
                    seasons={teamSeasons}
                  />
                  <TeamLink
                    sport={sport}
                    teamId={team.id}
                    teamName={team.name}
                    teams={namedTeams}
                    seasonId={teamSeasons[0]?.id}
                    className="club-page-team__link"
                  >
                    {t('clubPage.viewTeam')} →
                  </TeamLink>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}

function compareSeasonsByLatestEnd(
  a: { endDate: string; startDate: string },
  b: { endDate: string; startDate: string },
): number {
  const endDifference = new Date(b.endDate).getTime() - new Date(a.endDate).getTime();
  if (endDifference !== 0) {
    return endDifference;
  }
  return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
}

function ClubPage() {
  const { slug } = useParams<{ slug: string }>();
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [footballSeasons, setFootballSeasons] = useState<FootballSeasonDto[]>([]);
  const [hockeyTeams, setHockeyTeams] = useState<HockeyTeamDto[]>([]);
  const [hockeySeasons, setHockeySeasons] = useState<HockeySeasonDto[]>([]);
  const {
    teams,
    setParams: setTeamParams,
    isLoading: teamsLoading,
  } = useFloorballTeamsData();
  const {
    teams: footballTeams,
    setParams: setFootballTeamParams,
    isLoading: footballTeamsLoading,
  } = useFootballTeamsData();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [clubsData, seasonsResponse, footballSeasonsResponse, hockeySeasonsResponse] = await Promise.all([
          getClubs(),
          floorballSeasonService.getAll(),
          footballSeasonService.getAll().catch(() => ({ data: [] as FootballSeasonDto[] })),
          hockeySeasonService.getAll(audience.teamCategory).catch(() => [] as HockeySeasonDto[]),
        ]);
        setClubs(clubsData);
        setSeasons(seasonsResponse.data || []);
        setFootballSeasons(footballSeasonsResponse.data || []);
        setHockeySeasons(hockeySeasonsResponse ?? []);

        if (slug) {
          let foundClub = findClubBySlug(clubsData, slug);
          if (!foundClub) {
            foundClub = clubsData.find((club) => club.id === slug);
          }
          if (foundClub) {
            const teamFilter = { clubId: foundClub.id, teamCategories: [audience.teamCategory] };
            setTeamParams(teamFilter);
            setFootballTeamParams(teamFilter);
            const hockey = await hockeyTeamService
              .getByClubId(foundClub.id, audience.teamCategory)
              .catch(() => []);
            setHockeyTeams(hockey);
          } else {
            setHockeyTeams([]);
          }
        }

        setLoading(false);
      } catch {
        setError(t('clubPage.errorMessage'));
        setLoading(false);
      }
    };

    fetchData();
  }, [slug, setTeamParams, setFootballTeamParams, t, audience.teamCategory]);

  const club = useMemo(
    () => (!loading && slug ? findClubBySlug(clubs, slug) : undefined),
    [loading, slug, clubs]
  );

  const getTeamSeasons = useCallback(
    (teamId: string): FloorballSeasonDto[] => {
      return seasons
        .filter((season) =>
          season.seasonDivisions.some((sd) => sd.teamIds.includes(teamId))
        )
        .sort(compareSeasonsByLatestEnd);
    },
    [seasons]
  );

  const getFootballTeamSeasons = useCallback(
    (teamId: string): FootballSeasonDto[] => {
      return footballSeasons
        .filter((season) =>
          season.seasonDivisions.some((sd) => sd.teamIds.includes(teamId))
        )
        .sort(compareSeasonsByLatestEnd);
    },
    [footballSeasons]
  );

  const getHockeyTeamSeasons = useCallback(
    (teamId: string): HockeySeasonDto[] => {
      return hockeySeasons
        .filter((season) => (season.teams ?? []).some((team) => team.teamId === teamId))
        .sort(compareSeasonsByLatestEnd);
    },
    [hockeySeasons]
  );

  if (loading) {
    return (
      <PageTemplate title={t('clubPage.loading')} fullBleed>
        <CatalogPage title={t('clubPage.loading')}>
          <div className="club-page__state">
            <LoadingSpinner text={t('clubPage.loading')} />
          </div>
        </CatalogPage>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={t('clubPage.errorTitle')} fullBleed>
        <CatalogPage title={t('clubPage.errorTitle')}>
          <div className="club-page__state">
            <p>{error}</p>
          </div>
        </CatalogPage>
      </PageTemplate>
    );
  }

  if (!club) {
    return (
      <PageTemplate title={t('clubPage.notFoundTitle')} fullBleed>
        <CatalogPage title={t('clubPage.notFoundTitle')} description={t('clubPage.notFoundMessage')}>
          <div className="club-page__state">
            <Link to="/clubs" className="club-page__back-link">
              {t('clubPage.backToClubs')}
            </Link>
          </div>
        </CatalogPage>
      </PageTemplate>
    );
  }

  const foundingYear = clubFoundingYear(club.foundingDate);
  const city = clubText(club.city);
  const country = clubText(club.country);
  const location = [city, country].filter(Boolean).join(', ');
  const websiteUrl = clubPublicUrl(club.websiteUrl);
  const contactEmail = clubEmail(club.contactEmail);
  const logoUrl = clubPublicUrl(club.logoUrl);

  const description = [
    location,
    foundingYear ? `${t('clubPage.founded')} ${foundingYear}` : null,
  ].filter((part): part is string => Boolean(part)).join(' · ');

  const bannerLeading = (
    <div className="club-page__lead">
      <Link to="/clubs" className="club-page__back">
        {t('clubPage.backToClubs')}
      </Link>
      <TeamLogoMark
        logo={logoUrl}
        name={club.name}
        imageClassName="club-page__mark"
        fallbackClassName="club-page__mark club-page__mark--empty"
      />
    </div>
  );

  const bannerExtra = (websiteUrl || contactEmail) ? (
    <div className="club-page__links">
      {websiteUrl && (
        <a href={websiteUrl} target="_blank" rel="noopener noreferrer" className="club-page__link">
          {t('clubPage.website')}
        </a>
      )}
      {contactEmail && (
        <a href={`mailto:${contactEmail}`} className="club-page__link">
          {t('clubPage.contact')}
        </a>
      )}
    </div>
  ) : null;

  const noTeams = !teamsLoading && !footballTeamsLoading && teams.length === 0 && footballTeams.length === 0 && hockeyTeams.length === 0;

  return (
    <PageTemplate title={club.name} fullBleed>
      <CatalogPage
        title={club.name}
        description={description || undefined}
        bannerLeading={bannerLeading}
        bannerExtra={bannerExtra}
      >
        <div className="club-page">
          <h2 className="club-page__section-title">{t('clubPage.teams')}</h2>

          <ClubSportBlock
            sport="floorball"
            title={t('sports.floorball')}
            teams={teams}
            loading={teamsLoading}
            seasonsFor={getTeamSeasons}
          />
          <ClubSportBlock
            sport="football"
            title={t('sports.football')}
            teams={footballTeams}
            loading={footballTeamsLoading}
            seasonsFor={getFootballTeamSeasons}
          />
          <ClubSportBlock
            sport="hockey"
            title={t('sports.iceHockey')}
            teams={hockeyTeams}
            loading={false}
            seasonsFor={getHockeyTeamSeasons}
          />

          {noTeams && (
            <div className="club-page__no-teams">
              <p>{t('clubPage.noTeams')}</p>
            </div>
          )}
        </div>
      </CatalogPage>
    </PageTemplate>
  );
}

export default ClubPage;
