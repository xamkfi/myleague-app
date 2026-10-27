import { useTranslation } from 'react-i18next';
import StatRow from '../../floorball/FloorballMatchPage/components/StatRow';
import { getTeamInitials } from '../../floorball/FloorballMatchPage/components/matchUtils';
import type { HockeyMatchStatisticsDto } from '../../../types/hockey/hockeyTypes';
import '../../floorball/FloorballMatchPage/components/MatchStats.scss';

interface HockeyMatchTeamStatsProps {
  stats: HockeyMatchStatisticsDto;
  homeName: string;
  awayName: string;
  homeTeamId: string | null;
  awayTeamId: string | null;
}

/** Side-by-side team totals (goals, shots, face-offs, PIM) for one hockey match. */
function HockeyMatchTeamStats({ stats, homeName, awayName, homeTeamId, awayTeamId }: HockeyMatchTeamStatsProps) {
  const { t } = useTranslation();
  const home = stats.teams.find((row) => row.teamId === homeTeamId);
  const away = stats.teams.find((row) => row.teamId === awayTeamId);
  const homeGoals = home?.goalsFor ?? 0;
  const awayGoals = away?.goalsFor ?? 0;
  const homeSog = home?.shotsOnGoal ?? 0;
  const awaySog = away?.shotsOnGoal ?? 0;
  const homeFo = home?.faceoffWins ?? 0;
  const awayFo = away?.faceoffWins ?? 0;
  const homePim = home?.penaltyMinutes ?? 0;
  const awayPim = away?.penaltyMinutes ?? 0;

  return (
    <div className="match-stats">
      <div className="stats-header">
        <div className="team-identity home">
          <div className="team-crest home-team" title={homeName}>
            <span className="team-initials">{getTeamInitials(homeName)}</span>
          </div>
          <span className="team-name">{homeName}</span>
        </div>
        <div className="header-label">{t('matchPage.stats.title')}</div>
        <div className="team-identity away">
          <span className="team-name">{awayName}</span>
          <div className="team-crest away-team" title={awayName}>
            <span className="team-initials">{getTeamInitials(awayName)}</span>
          </div>
        </div>
      </div>
      <div className="stats-content">
        <StatRow
          label={t('hockeyPage.goals', 'Goals')}
          home={homeGoals}
          away={awayGoals}
          homeValue={homeGoals}
          awayValue={awayGoals}
          total={homeGoals + awayGoals}
        />
        <StatRow
          label={t('hockeyPage.shotsOnGoal', 'Shots on goal')}
          home={homeSog}
          away={awaySog}
          homeValue={homeSog}
          awayValue={awaySog}
          total={homeSog + awaySog}
        />
        <StatRow
          label={t('hockeyPage.faceoffs', 'Face-offs')}
          home={homeFo}
          away={awayFo}
          homeValue={homeFo}
          awayValue={awayFo}
          total={homeFo + awayFo}
        />
        <StatRow
          label={t('hockeyPage.penaltyMinutes', 'PIM')}
          home={homePim}
          away={awayPim}
          homeValue={homePim}
          awayValue={awayPim}
          total={homePim + awayPim}
        />
      </div>
    </div>
  );
}

export default HockeyMatchTeamStats;
