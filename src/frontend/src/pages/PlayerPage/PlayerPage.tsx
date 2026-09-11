import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import SportIcon from '../../components/SportIcon/SportIcon';
import { FloorballPlayerProfile } from '../FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage';
import { FootballPlayerProfile } from '../FootballPlayerPage/FootballPlayerPage';
import { HockeyPlayerProfile } from '../HockeyPlayerPage/HockeyPlayerPage';
import {
  personPlayerSportsService,
  type PersonPlayerSports,
  type PersonSportKind,
} from '../../api/common/personPlayerSportsService';
import { unwrapApiErrorMessage } from '../../api/utils/ParseErrorResponse';
import './PlayerPage.scss';
import '../FloorballTeamPlayerUserPage/FloorballTeamPlayerUserPage.scss';

const VALID_SPORTS: PersonSportKind[] = ['floorball', 'football', 'hockey'];

function isPersonSportKind(value: string | null): value is PersonSportKind {
  return value === 'floorball' || value === 'football' || value === 'hockey';
}

function PlayerPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const [data, setData] = useState<PersonPlayerSports | null>(null);
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
        const result = await personPlayerSportsService.getById(id);
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

  return (
    <PageTemplate title={title}>
      <div className="player-page player-page--shell">
        {availableSports.length > 1 && activeSport && (
          <div className="player-sport-tabs" role="tablist" aria-label={t('playerPage.sportsLabel')}>
            <div className="player-sport-tabs__group">
              {availableSports.map((sport) => (
                <button
                  key={sport}
                  type="button"
                  role="tab"
                  aria-selected={sport === activeSport}
                  className={`tab-button${sport === activeSport ? ' active' : ''}`}
                  onClick={() => handleSportChange(sport)}
                >
                  <SportIcon
                    sport={sport}
                    size="sm"
                    inverted={sport === activeSport}
                    decorative
                  />
                  <span>{t(`playerPage.sports.${sport}`)}</span>
                </button>
              ))}
            </div>
          </div>
        )}

        {availableSports.length === 0 && (
          <div className="player-container">
            <p className="no-data-message">{t('playerPage.noSports')}</p>
          </div>
        )}

        {activeSport === 'floorball' && activePlayerId && (
          <FloorballPlayerProfile playerId={activePlayerId} embedded />
        )}
        {activeSport === 'football' && activePlayerId && (
          <FootballPlayerProfile playerId={activePlayerId} embedded />
        )}
        {activeSport === 'hockey' && activePlayerId && (
          <HockeyPlayerProfile playerId={activePlayerId} embedded />
        )}
      </div>
    </PageTemplate>
  );
}

export default PlayerPage;
