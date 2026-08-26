import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import HockeyMatchRow from '../../components/HockeyMatchRow/HockeyMatchRow';
import HockeyRosterSection from './HockeyRosterSection';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { clubService } from '../../api/common/clubService';
import type { HockeyMatchDto, HockeyPlayerCompetitionStatisticsDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { shouldRefreshHockeyMatches } from '../../types/hockey/hockeyTypes';
import { findTeamBySlug, slugify } from '../../utils/slugUtils';
import { loadHockeyRosterNameMaps, loadTeamNameMap } from '../../utils/hockeyLookups';
import { useAudience } from '../../context/AudienceContext';
import { useIntervalWhen } from '../../hooks/useIntervalWhen';
import '../FloorballTeamPage/FloorballTeamPage.scss';
import '../FloorballTeamPage/components/TeamNavbar.scss';
import '../../components/MatchesList/MatchesList.scss';

type HockeyTeamTab = 'roster' | 'results';

function HockeyTeamPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const navigate = useNavigate();
  const { slug } = useParams<{ slug: string }>();
  const [team, setTeam] = useState<HockeyTeamDto | null>(null);
  const [clubName, setClubName] = useState('');
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
      setTeam(selected);
      setTeamNames(await loadTeamNameMap(teams));
      setMatches(await hockeyMatchService.getByTeam(selected.id));
      const names = await loadHockeyRosterNameMaps([selected]);
      setPlayerNames(names.byPlayerId);
      const competitionIds = [...new Set(
        selected.roster
          .map((row) => row.competitionId)
          .filter((value): value is string => Boolean(value)),
      )];
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
    };
    void load().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load team'));
  }, [slug, t, audience.teamCategory]);

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
        <div className="hero-image-container">
          <div className="hero-image" />
          <div className="team-header">
            <div className="left-navigation-container">
              <div className="breadcrumb">
                {clubName && (
                  <>
                    <button
                      type="button"
                      className="club-link"
                      onClick={() => navigate(`/club/${slugify(clubName)}`)}
                    >
                      {clubName}
                    </button>
                    <span className="separator">›</span>
                  </>
                )}
                <span className="current">{team.name}</span>
              </div>
            </div>
            <div className="header-content">
              <div className="team-info">
                <h1>{team.name}</h1>
                <p>{team.homeArena}{team.shortName ? ` · ${team.shortName}` : ''}</p>
              </div>
            </div>
          </div>
        </div>
        <div className="team-navigation-tabs" role="tablist">
          <button type="button" className={`team-nav-tab ${activeTab === 'roster' ? 'active' : ''}`} onClick={() => setActiveTab('roster')}>
            {t('teamUserPage.roster', 'Roster')}
          </button>
          <button type="button" className={`team-nav-tab ${activeTab === 'results' ? 'active' : ''}`} onClick={() => setActiveTab('results')}>
            {t('teamUserPage.results', 'Results')}
          </button>
        </div>
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
    </PageTemplate>
  );
}

export default HockeyTeamPage;
