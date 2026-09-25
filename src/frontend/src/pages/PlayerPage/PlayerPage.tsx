import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import SportIcon from '../../components/SportIcon/SportIcon';
import UnderlineTabs from '../../components/UnderlineTabs/UnderlineTabs';
import { FloorballPlayerProfile } from '../FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage';
import { FootballPlayerProfile } from '../FootballPlayerPage/FootballPlayerPage';
import { HockeyPlayerProfile } from '../HockeyPlayerPage/HockeyPlayerPage';
import { PlayerLicenceSummary } from './PlayerLicenceSummary';
import {
  personPlayerSportsService,
  type PersonPlayerLicence,
  type PersonPlayerSports,
  type PersonSportKind,
} from '../../api/common/personPlayerSportsService';
import { floorballSeasonService } from '../../api/floorball/floorballSeasonService';
import { floorballTournamentService } from '../../api/floorball/floorballTournamentService';
import { footballSeasonService } from '../../api/football/footballSeasonService';
import { footballTournamentService } from '../../api/football/footballTournamentService';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../api/hockey/hockeyTournamentService';
import { unwrapApiErrorMessage } from '../../api/utils/ParseErrorResponse';
import './PlayerPage.scss';
import '../FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage.scss';

const VALID_SPORTS: PersonSportKind[] = ['floorball', 'football', 'hockey'];

function isPersonSportKind(value: string | null): value is PersonSportKind {
  return value === 'floorball' || value === 'football' || value === 'hockey';
}

type DatedCompetition = { id: string; endDate?: string | null };

function addCompetitionsStillInForce(
  items: DatedCompetition[] | null | undefined,
  ids: Set<string>,
): void {
  const now = Date.now();
  for (const item of items ?? []) {
    if (!item.id) {
      continue;
    }
    if (item.endDate) {
      const end = new Date(item.endDate).getTime();
      if (!Number.isNaN(end) && end < now) {
        continue;
      }
    }
    ids.add(item.id);
  }
}

async function loadActiveCompetitionIds(sports: PersonSportKind[]): Promise<Set<string>> {
  const ids = new Set<string>();
  const tasks: Promise<void>[] = [];

  if (sports.includes('floorball')) {
    tasks.push(
      floorballSeasonService
        .getActive()
        .then((response) => addCompetitionsStillInForce(response.data, ids))
        .catch(() => undefined),
    );
    tasks.push(
      floorballTournamentService
        .getActive()
        .then((response) => addCompetitionsStillInForce(response.data, ids))
        .catch(() => undefined),
    );
  }

  if (sports.includes('football')) {
    tasks.push(
      footballSeasonService
        .getActive()
        .then((response) => addCompetitionsStillInForce(response.data, ids))
        .catch(() => undefined),
    );
    tasks.push(
      footballTournamentService
        .getActive()
        .then((response) => addCompetitionsStillInForce(response.data, ids))
        .catch(() => undefined),
    );
  }

  if (sports.includes('hockey')) {
    tasks.push(
      hockeySeasonService
        .getActive()
        .then((seasons) => addCompetitionsStillInForce(seasons, ids))
        .catch(() => undefined),
    );
    tasks.push(
      hockeyTournamentService
        .getActive()
        .then((tournaments) => addCompetitionsStillInForce(tournaments, ids))
        .catch(() => undefined),
    );
  }

  await Promise.all(tasks);
  return ids;
}

function PlayerPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const [data, setData] = useState<PersonPlayerSports | null>(null);
  const [activeCompetitionIds, setActiveCompetitionIds] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }

    const load = async (): Promise<void> => {
      try {
        setLoading(true);
        setError(null);
        setData(null);
        setActiveCompetitionIds(new Set());
        const result = await personPlayerSportsService.getById(id);
        const sports = [...new Set(result.licences.map((licence) => licence.sport))];
        const inForceIds = await loadActiveCompetitionIds(sports);
        setActiveCompetitionIds(inForceIds);
        setData(result);

        if (id.toLowerCase() !== result.personId.toLowerCase()) {
          const sport = searchParams.get('sport');
          navigate(
            `/player/${result.personId}${sport ? `?sport=${encodeURIComponent(sport)}` : ''}`,
            { replace: true },
          );
        }
      } catch (err) {
        setData(null);
        setError(unwrapApiErrorMessage(err, t('playerPage.loadError')));
      } finally {
        setLoading(false);
      }
    };

    void load();
    // searchParams is read only when canonicalizing a sport-player id on first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps -- tab changes must not refetch
  }, [id, t, navigate]);

  const availableSports = useMemo(
    () => VALID_SPORTS.filter((sport) => data?.sports.some((item) => item.sport === sport) ?? false),
    [data],
  );

  const activeSport: PersonSportKind | null = useMemo(() => {
    if (availableSports.length === 0) {
      return null;
    }
    const requested = searchParams.get('sport');
    if (isPersonSportKind(requested) && availableSports.includes(requested)) {
      return requested;
    }
    return availableSports[0];
  }, [availableSports, searchParams]);

  const activePlayerId = useMemo(() => {
    if (!data || !activeSport) {
      return null;
    }
    return data.sports.find((item) => item.sport === activeSport)?.playerId ?? null;
  }, [data, activeSport]);

  const handleSportChange = (sport: PersonSportKind): void => {
    setSearchParams({ sport });
  };

  const title = data?.fullName || t('playerPage.title');

  if (loading) {
    return (
      <PageTemplate title={t('playerPage.title')}>
        <div className="player-loading">{t('common.loading')}</div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={t('playerPage.title')}>
        <div className="player-error">{error}</div>
      </PageTemplate>
    );
  }

  if (!data) {
    return (
      <PageTemplate title={t('playerPage.title')}>
        <div className="player-error">{t('playerPage.notFound')}</div>
      </PageTemplate>
    );
  }

  const licences: PersonPlayerLicence[] = (data.licences ?? []).filter(
    (licence) =>
      licence.isActive &&
      licence.competitionId !== null &&
      activeCompetitionIds.has(licence.competitionId),
  );

  return (
    <PageTemplate title={title}>
      <div className="player-page player-page--shell">
        {availableSports.length > 1 && activeSport && (
          <UnderlineTabs
            tabs={availableSports.map((sport) => ({
              id: sport,
              label: t(`playerPage.sports.${sport}`),
              icon: <SportIcon sport={sport} size="sm" decorative />,
            }))}
            activeId={activeSport}
            onChange={(id) => {
              if (isPersonSportKind(id)) {
                handleSportChange(id);
              }
            }}
            ariaLabel={t('playerPage.sportsLabel')}
          />
        )}

        {availableSports.length === 0 && (
          <div className="player-container">
            <p className="no-data-message">{t('playerPage.noSports')}</p>
          </div>
        )}

        <div
          role={availableSports.length > 1 ? 'tabpanel' : undefined}
          id={availableSports.length > 1 && activeSport ? `tabpanel-${activeSport}` : undefined}
          aria-labelledby={availableSports.length > 1 && activeSport ? `tab-${activeSport}` : undefined}
        >
          {activeSport === 'floorball' && activePlayerId && (
            <FloorballPlayerProfile
              playerId={activePlayerId}
              embedded
              licenceSummary={<PlayerLicenceSummary licences={licences.filter((licence) => licence.sport === 'floorball')} />}
            />
          )}
          {activeSport === 'football' && activePlayerId && (
            <FootballPlayerProfile
              playerId={activePlayerId}
              embedded
              licenceSummary={<PlayerLicenceSummary licences={licences.filter((licence) => licence.sport === 'football')} />}
            />
          )}
          {activeSport === 'hockey' && activePlayerId && (
            <HockeyPlayerProfile
              playerId={activePlayerId}
              embedded
              licenceSummary={<PlayerLicenceSummary licences={licences.filter((licence) => licence.sport === 'hockey')} />}
            />
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default PlayerPage;
