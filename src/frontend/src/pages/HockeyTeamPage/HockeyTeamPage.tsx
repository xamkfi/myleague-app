import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import CompetitionHero from '../../components/CompetitionHero/CompetitionHero';
import UnderlineTabs from '../../components/UnderlineTabs/UnderlineTabs';
import HockeyMatchRow from '../../components/HockeyMatchRow/HockeyMatchRow';
import HockeyRosterSection from './HockeyRosterSection';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../api/hockey/hockeyTournamentService';
import { clubService } from '../../api/common/clubService';
import type { HockeyMatchDto, HockeyPlayerCompetitionStatisticsDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { shouldRefreshHockeyMatches } from '../../types/hockey/hockeyTypes';
import { findTeamBySlug, slugify } from '../../utils/slugUtils';
import { teamMarkLabel } from '../../utils/teamMarkLabel';
import { getLeaguePath, getTournamentPath, isGuid } from '../../utils/sportRoutes';
import { loadHockeyRosterNameMaps, loadTeamNameMap } from '../../utils/hockeyLookups';
import { useAudience } from '../../context/AudienceContext';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import '../FloorballTeamPage/FloorballTeamPage.scss';
import '../../components/MatchesList/MatchesList.scss';

type HockeyTeamTab = 'roster' | 'results';

function HockeyTeamPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const navigate = useNavigate();
  const { slug } = useParams<{ slug: string }>();
  const [searchParams] = useSearchParams();
  const requestedSeasonId = searchParams.get('season');
  const [team, setTeam] = useState<HockeyTeamDto | null>(null);
  const [clubName, setClubName] = useState('');
  const [clubLogo, setClubLogo] = useState<string | null>(null);
  const [competitionLink, setCompetitionLink] = useState<{ name: string; path: string } | null>(null);
  const [matches, setMatches] = useState<HockeyMatchDto[]>([]);
  const [teamNames, setTeamNames] = useState<Map<string, string>>(new Map());
  const [playerNames, setPlayerNames] = useState<Map<string, string>>(new Map());
  const [playerStats, setPlayerStats] = useState<HockeyPlayerCompetitionStatisticsDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<HockeyTeamTab>('roster');

  useEffect(() => {
    const load = async (): Promise<void> => {
      const teams = await hockeyTeamService.getAll(audience.teamCategory);
      const named = teams.map((item) => ({ id: item.id, name: item.name }));
      const found = slug ? findTeamBySlug(named, slug) : undefined;
      const selected = found ? teams.find((item) => item.id === found.id) : undefined;
      if (!selected) {
        setError(t('teamUserPage.notFound', 'Team not found'));
        return;
      }
      const matchList = await hockeyMatchService.getByTeam(selected.id);
      setMatches(matchList);
      const latestCompetitionId = matchList
        .filter((match) => match.competitionId)
        .slice()
        .sort((left, right) =>
          new Date(right.scheduledStartTime).getTime() - new Date(left.scheduledStartTime).getTime(),
        )[0]?.competitionId ?? null;
      const seasonId = isGuid(requestedSeasonId) ? requestedSeasonId : latestCompetitionId;
      let scoped = selected;
      if (seasonId) {
        try {
          scoped = await hockeyTeamService.getById(selected.id, seasonId);
        } catch {
          scoped = selected;
        }
      }
      setTeam(scoped);
      setTeamNames(await loadTeamNameMap(teams));
      const names = await loadHockeyRosterNameMaps([scoped]);
      setPlayerNames(names.byPlayerId);
      const competitionIds = seasonId ? [seasonId] : [];
      const statsLists = await Promise.all(
        competitionIds.map((competitionId) =>
          hockeyStatisticsService.getPlayers(competitionId).catch(() => []),
        ),
      );
      const byPlayer = new Map<string, HockeyPlayerCompetitionStatisticsDto>();
      for (const list of statsLists) {
        for (const row of list) {
          if (row.teamId === selected.id) {
            byPlayer.set(row.playerId, row);
          }
        }
      }
      setPlayerStats([...byPlayer.values()]);
      const clubs = await clubService.getAll().catch(() => []);
      const club = clubs.find((item) => item.id === selected.clubId);
      setClubName(club?.name ?? '');
      setClubLogo(club?.logoUrl ?? null);
      if (seasonId) {
        const season = await hockeySeasonService.getById(seasonId).catch(() => null);
        if (season) {
          setCompetitionLink({ name: season.name, path: getLeaguePath('hockey', season.id) });
        } else {
          const tournament = await hockeyTournamentService.getById(seasonId).catch(() => null);
          setCompetitionLink(
            tournament
              ? { name: tournament.name, path: getTournamentPath('hockey', tournament.id) }
              : null,
          );
        }
      } else {
        setCompetitionLink(null);
      }
    };
    void load().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load team'));
  }, [slug, t, audience.teamCategory, requestedSeasonId]);

  const refreshLiveMatches = useCallback(async (): Promise<void> => {
    if (!team) {
      return;
    }
    try {
      setMatches(await hockeyMatchService.getByTeam(team.id));
    } catch {
      /* keep last known scores */
    }
  }, [team]);

  const hasLiveMatches = shouldRefreshHockeyMatches(matches);
  useIntervalWhen(hasLiveMatches, () => {
    void refreshLiveMatches();
  }, 4000);

  if (error && !team) {
    return (
      <PageTemplate title={t('teamUserPage.notFoundTitle', 'Team')}>
        <div className="floorball-team-page">
          <div className="not-found-state">
            <h2>{t('teamUserPage.notFound')}</h2>
            <p>{error}</p>
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (!team) {
    return (
      <PageTemplate title={t('common.loading', 'Loading...')}>
        <div className="floorball-team-page">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={team.name}>
      <div className="floorball-team-page">
        <nav className="floorball-team-page__crumb" aria-label={clubName || team.name}>
          {clubName && (
            <>
              <button
                type="button"
                className="floorball-team-page__crumb-link"
                onClick={() => navigate(`/club/${slugify(clubName)}`)}
              >
                {clubName}
              </button>
              <span aria-hidden="true">›</span>
            </>
          )}
          <span className="floorball-team-page__crumb-current">{team.name}</span>
        </nav>

        <CompetitionHero
          title={team.name}
          logoUrl={team.logoUrl || clubLogo}
          markLabel={teamMarkLabel(team.name)}
          meta={competitionLink ? (
            <button
              type="button"
              className="floorball-team-page__season"
              onClick={() => navigate(competitionLink.path)}
            >
              {competitionLink.name}
            </button>
          ) : null}
        />

        <UnderlineTabs
          tabs={[
            { id: 'roster', label: t('teamUserPage.roster') },
            { id: 'results', label: t('teamUserPage.results') },
          ]}
          activeId={activeTab}
          onChange={(id) => setActiveTab(id as HockeyTeamTab)}
          ariaLabel={t('teamUserPage.summary')}
        />

        <div
          className="tab-content-container"
          role="tabpanel"
          id={`tabpanel-${activeTab}`}
          aria-labelledby={`tab-${activeTab}`}
        >
          {activeTab === 'roster' && (
            <HockeyRosterSection team={team} playerNames={playerNames} playerStats={playerStats} />
          )}
          {activeTab === 'results' && (
            <div className="results-section">
              <div className="matches-grid">
                {matches.map((match) => (
                  <HockeyMatchRow key={match.id} match={match} teamNames={teamNames} />
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default HockeyTeamPage;
