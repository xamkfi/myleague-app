import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { footballTeamService } from '../../../api/football/footballTeamService';
import {
  FootballPosition,
  type FootballLineupPlayer,
  type FootballMatchDto,
  type FootballTeamPlayer,
} from '../../../types/football/footballTypes';
import './MatchLineups.scss';

type RosterLookup = Map<string, FootballTeamPlayer>;

interface ActiveRosterEntry {
  playerId: string;
  playerName: string;
  jerseyNumber?: number;
  position: FootballPosition;
  isOnField: boolean;
  isSentOff: boolean;
}

const POSITION_DISPLAY_ORDER: FootballPosition[] = [
  FootballPosition.Goalkeeper,
  FootballPosition.Defender,
  FootballPosition.Midfielder,
  FootballPosition.Forward,
];

const sortByJerseyThenName = (a: ActiveRosterEntry, b: ActiveRosterEntry): number => {
  const numA: number = a.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  const numB: number = b.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  if (numA !== numB) return numA - numB;
  return a.playerName.localeCompare(b.playerName, undefined, { sensitivity: 'base' });
};

const buildRosterLookup = (roster: FootballTeamPlayer[]): RosterLookup => {
  const byId: RosterLookup = new Map<string, FootballTeamPlayer>();
  for (const player of roster) {
    byId.set(player.playerId, player);
  }
  return byId;
};

const buildActiveRoster = (
  lineup: readonly FootballLineupPlayer[],
  lookup: RosterLookup,
): ActiveRosterEntry[] => {
  const entries: ActiveRosterEntry[] = [];
  for (const entry of lineup) {
    const player: FootballTeamPlayer | undefined = lookup.get(entry.playerId);
    if (!player) {
      entries.push({
        playerId: entry.playerId,
        playerName: entry.playerId,
        position: entry.position,
        isOnField: entry.isOnField,
        isSentOff: entry.isSentOff,
      });
      continue;
    }
    entries.push({
      playerId: player.playerId,
      playerName: player.playerName,
      jerseyNumber: player.jerseyNumber,
      position: entry.position,
      isOnField: entry.isOnField,
      isSentOff: entry.isSentOff,
    });
  }
  return entries;
};

export default function MatchLineups({ match }: { match: FootballMatchDto }) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [homeLookup, setHomeLookup] = useState<RosterLookup>(new Map());
  const [awayLookup, setAwayLookup] = useState<RosterLookup>(new Map());

  useEffect(() => {
    let cancelled = false;
    async function fetchRosters(): Promise<void> {
      try {
        if (!match.homeTeamId || !match.awayTeamId) {
          setHomeLookup(new Map());
          setAwayLookup(new Map());
          return;
        }
        const [homeResponse, awayResponse] = await Promise.all([
          footballTeamService.getById(match.homeTeamId),
          footballTeamService.getById(match.awayTeamId),
        ]);
        if (cancelled) return;
        setHomeLookup(buildRosterLookup(homeResponse.roster));
        setAwayLookup(buildRosterLookup(awayResponse.roster));
      } catch (error) {
        console.error('Failed to load team rosters for match lineup', error);
      }
    }
    void fetchRosters();
    return () => {
      cancelled = true;
    };
  }, [match.homeTeamId, match.awayTeamId]);

  const homeActiveRoster: ActiveRosterEntry[] = useMemo(
    () => buildActiveRoster(match.homeLineup ?? [], homeLookup),
    [match.homeLineup, homeLookup],
  );

  const awayActiveRoster: ActiveRosterEntry[] = useMemo(
    () => buildActiveRoster(match.awayLineup ?? [], awayLookup),
    [match.awayLineup, awayLookup],
  );

  const handlePlayerClick = (playerId: string): void => {
    navigate(`/football/player/${playerId}`);
  };

  const renderTeamRoster = (roster: ActiveRosterEntry[], teamName: string) => {
    if (roster.length === 0) {
      return (
        <div className="lineup-team-block">
          <div className="lineup-team-title">{teamName}</div>
          <div className="lineup-empty">
            {t('matchPage.lineups.notSet', 'The match lineup has not been set yet.')}
          </div>
        </div>
      );
    }

    const presentPositions: FootballPosition[] = POSITION_DISPLAY_ORDER.filter((pos) =>
      roster.some((entry) => entry.position === pos),
    );

    return (
      <div className="lineup-team-block">
        <div className="lineup-team-title">{teamName}</div>
        {presentPositions.map((pos) => {
          const entries: ActiveRosterEntry[] = roster
            .filter((entry) => entry.position === pos)
            .sort(sortByJerseyThenName);
          return (
            <div key={pos} className="lineup-position-group">
              <div className="lineup-position-label">
                {t(`football.positions.${pos}`, pos)}
              </div>
              <div className="lineup-table-wrap">
                <div className="lineup-row lineup-row-header">
                  <div className="lineup-col-number">#</div>
                  <div className="lineup-col-name">{t('roster.name')}</div>
                  <div className="lineup-col-pos">{t('roster.position')}</div>
                </div>
                {entries.map((entry) => (
                  <div
                    key={entry.playerId}
                    className="lineup-row lineup-row-player"
                    onClick={() => handlePlayerClick(entry.playerId)}
                  >
                    <div className="lineup-col-number">
                      <span className="jersey-badge">{entry.jerseyNumber}</span>
                    </div>
                    <div className="lineup-col-name player-link">
                      {entry.playerName}
                      {entry.isSentOff ? ` (${t('football.match.sentOff', 'sent off')})` : ''}
                    </div>
                    <div className="lineup-col-pos">
                      {t(`football.positions.${entry.position}`, entry.position)}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    );
  };

  return (
    <div className="match-lineups-container">
      <div className="match-lineups-grid">
        {renderTeamRoster(homeActiveRoster, match.homeTeamName ?? 'TBD')}
        {renderTeamRoster(awayActiveRoster, match.awayTeamName ?? 'TBD')}
      </div>
    </div>
  );
}
