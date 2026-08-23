import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  HOCKEY_POSITIONS,
  hockeyAwayTeam,
  hockeyHomeTeam,
  type HockeyMatchActivePlayerDto,
  type HockeyMatchDto,
  type HockeyTeamDto,
} from '../../types/hockey/hockeyTypes';
import '../MatchPage/components/MatchLineups.scss';

interface HockeyMatchLineupsProps {
  match: HockeyMatchDto;
  homeName: string;
  awayName: string;
  teams: HockeyTeamDto[];
  playerNames: Map<string, string>;
}

const POSITION_ORDER = [...HOCKEY_POSITIONS];

function positionRank(position: string): number {
  const index = POSITION_ORDER.indexOf(position as (typeof HOCKEY_POSITIONS)[number]);
  return index === -1 ? POSITION_ORDER.length : index;
}

function HockeyMatchLineups({
  match,
  homeName,
  awayName,
  teams,
  playerNames,
}: HockeyMatchLineupsProps) {
  const { t } = useTranslation();
  const home = hockeyHomeTeam(match);
  const away = hockeyAwayTeam(match);

  const renderSide = (
    side: ReturnType<typeof hockeyHomeTeam>,
    name: string,
  ) => {
    if (!side) {
      return null;
    }
    const career = teams.find((team) => team.id === side.teamId);
    const players = [...side.activePlayers].sort((a, b) => {
      const rank = positionRank(a.position) - positionRank(b.position);
      if (rank !== 0) {
        return rank;
      }
      return a.jerseyNumber - b.jerseyNumber;
    });

    return (
      <div key={side.id} className="lineup-team-block">
        <div className="lineup-team-title">{name}</div>
        {players.length === 0 ? (
          <div className="lineup-empty">{t('matchPage.lineups.notSet', 'Lineup not set')}</div>
        ) : (
          <div className="lineup-table-wrap">
            <div className="lineup-row lineup-row-header">
              <div className="lineup-col-number">#</div>
              <div className="lineup-col-name">{t('roster.name', 'Name')}</div>
              <div className="lineup-col-pos">{t('roster.position', 'Pos')}</div>
            </div>
            {players.map((player: HockeyMatchActivePlayerDto) => {
              const roster = career?.roster.find((row) => row.id === player.teamPlayerId);
              const captain = roster?.captainRole === 'Captain'
                ? ' (C)'
                : roster?.captainRole === 'AlternateCaptain'
                  ? ' (A)'
                  : '';
              const label = playerNames.get(player.teamPlayerId) ?? `#${player.jerseyNumber}`;
              return (
                <div key={player.id} className="lineup-row lineup-row-player">
                  <div className="lineup-col-number">{player.jerseyNumber}</div>
                  <div className="lineup-col-name">
                    {roster ? (
                      <Link to={`/hockeyplayer/${roster.playerId}`}>{label}{captain}</Link>
                    ) : (
                      <>{label}{captain}</>
                    )}
                  </div>
                  <div className="lineup-col-pos">
                    {t(`hockey.positions.${player.position}`, player.position)}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="match-lineups-container">
      <div className="match-lineups-grid">
        {renderSide(home, homeName)}
        {renderSide(away, awayName)}
      </div>
    </div>
  );
}

export default HockeyMatchLineups;
