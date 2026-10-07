import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  FootballPosition,
  type FootballLineupPlayer,
  type FootballMatchDto,
  type FootballTeamPlayer,
} from '../../../types/football/footballTypes';
import { getPlayerPath } from '../../../utils/sportRoutes';
import { LicenceLegend, LicenceStatusDot } from '../../../components/LicenceStatus/LicenceStatus';
import './MatchLineups.scss';

type RosterLookup = Map<string, FootballTeamPlayer>;

interface ActiveRosterEntry {
  playerId: string;
  playerName: string;
  jerseyNumber?: number;
  position: FootballPosition;
  isOnField: boolean;
  isSentOff: boolean;
  licenceActive?: boolean;
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

// The roster holds base and competition rows. Prefer the match competition's row,
// because the licence is tracked per competition.
const buildRosterLookup = (roster: FootballTeamPlayer[], competitionId: string): RosterLookup => {
  const byId: RosterLookup = new Map<string, FootballTeamPlayer>();
  for (const player of roster) {
    const current: FootballTeamPlayer | undefined = byId.get(player.playerId);
    if (!current || (player.competitionId === competitionId && current.competitionId !== competitionId)) {
      byId.set(player.playerId, player);
    }
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
      licenceActive: player.isActive,
    });
  }
  return entries;
};

interface MatchLineupsProps {
  match: FootballMatchDto;
  homeRoster: FootballTeamPlayer[];
  awayRoster: FootballTeamPlayer[];
}

export default function MatchLineups({ match, homeRoster, awayRoster }: MatchLineupsProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const homeLookup: RosterLookup = useMemo(
    () => buildRosterLookup(homeRoster, match.competitionId),
    [homeRoster, match.competitionId],
  );
  const awayLookup: RosterLookup = useMemo(
    () => buildRosterLookup(awayRoster, match.competitionId),
    [awayRoster, match.competitionId],
  );

  const homeActiveRoster: ActiveRosterEntry[] = useMemo(
    () => buildActiveRoster(match.homeLineup ?? [], homeLookup),
    [match.homeLineup, homeLookup],
  );

  const awayActiveRoster: ActiveRosterEntry[] = useMemo(
    () => buildActiveRoster(match.awayLineup ?? [], awayLookup),
    [match.awayLineup, awayLookup],
  );

  const handlePlayerClick = (playerId: string): void => {
    navigate(getPlayerPath('football', playerId));
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
                      <span className="jersey-badge licence-anchor">
                        {entry.jerseyNumber}
                        {entry.licenceActive !== undefined && <LicenceStatusDot active={entry.licenceActive} />}
                      </span>
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
      {[...homeActiveRoster, ...awayActiveRoster].some((entry) => entry.licenceActive !== undefined) && (
        <LicenceLegend />
      )}
      <div className="match-lineups-grid">
        {renderTeamRoster(homeActiveRoster, match.homeTeamName ?? 'TBD')}
        {renderTeamRoster(awayActiveRoster, match.awayTeamName ?? 'TBD')}
      </div>
    </div>
  );
}
