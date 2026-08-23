import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  HOCKEY_POSITIONS,
  type HockeyPlayerCompetitionStatisticsDto,
  type HockeyTeamDto,
  type HockeyTeamPlayerDto,
} from '../../types/hockey/hockeyTypes';
import '../FloorballTeamPage/components/RosterSection.scss';

interface HockeyRosterSectionProps {
  team: HockeyTeamDto;
  playerNames: Map<string, string>;
  playerStats: HockeyPlayerCompetitionStatisticsDto[];
}

function HockeyRosterSection({ team, playerNames, playerStats }: HockeyRosterSectionProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const statsLookup = new Map(playerStats.map((row) => [row.playerId, row]));
  const positions = HOCKEY_POSITIONS.filter((position) =>
    team.roster.some((row) => row.position === position),
  );

  const renderPlayer = (row: HockeyTeamPlayerDto) => {
    const stats = statsLookup.get(row.playerId);
    const captain = row.captainRole === 'Captain'
      ? ' (C)'
      : row.captainRole === 'AlternateCaptain'
        ? ' (A)'
        : '';
    return (
      <div
        className="table roster-player"
        onClick={() => navigate(`/hockeyplayer/${row.playerId}`)}
        key={row.id}
      >
        <div className="roster-jersey row">{row.jerseyNumber ?? '—'}</div>
        <div className="roster-player-name">
          {playerNames.get(row.playerId) ?? row.playerId.slice(0, 8)}{captain}
        </div>
        <div className="roster-age">—</div>
        <div className="roster-games-played">{stats?.gamesPlayed ?? '—'}</div>
        <div className="roster-goals">{stats?.goals ?? '—'}</div>
        <div className="roster-assists">{stats?.assists ?? '—'}</div>
      </div>
    );
  };

  return (
    <div className="roster-section">
      {positions.map((position) => (
        <div className="roster-container" key={position}>
          <div className="roster-position-header">
            {t(`hockey.positions.${position}`, position)}
          </div>
          <div className="roster-position-container">
            <div className="table-wrapper">
              <div className="table stats-header">
                <div className="roster-jersey" title={t('roster.tooltips.jerseyNumber')}>{t('roster.jerseyNumber')}</div>
                <div className="roster-player-name">{t('roster.name')}</div>
                <div className="roster-age" title={t('roster.tooltips.age')}>{t('roster.age')}</div>
                <div className="roster-games-played" title={t('roster.tooltips.matchesPlayed')}>{t('roster.matchesPlayed')}</div>
                <div className="roster-goals" title={t('roster.tooltips.goals')}>{t('roster.goals')}</div>
                <div className="roster-assists" title={t('roster.tooltips.assists')}>{t('roster.assists')}</div>
              </div>
              {team.roster
                .filter((row) => row.position === position)
                .sort((a, b) => (a.jerseyNumber ?? 99) - (b.jerseyNumber ?? 99))
                .map(renderPlayer)}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

export default HockeyRosterSection;
