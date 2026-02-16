import { useTranslation } from 'react-i18next';
import './MatchStatsCards.scss';
import CheckIcon from '../../../../../../assets/basicIcons/check_circle.svg';
import ScheduleIcon from '../../../../../../assets/basicIcons/schedule.svg';
import PendingIcon from '../../../../../../assets/basicIcons/pending.svg';
import CancelIcon from '../../../../../../assets/basicIcons/cancel.svg';
import HistoryIcon from '../../../../../../assets/basicIcons/history.svg';

interface MatchStats {
  total: number;
  completed: number;
  scheduled: number;
  inProgress: number;
  cancelled: number;
}

interface MatchStatsCardsProps {
  stats: MatchStats;
  isSeasonFiltered: boolean;
  onCompletedClick?: () => void;
  onScheduledClick?: () => void;
  onInProgressClick?: () => void;
  onCancelledClick?: () => void;
}

const MatchStatsCards = ({
  stats,
  isSeasonFiltered,
  onCompletedClick,
  onScheduledClick,
  onInProgressClick,
  onCancelledClick,
}: MatchStatsCardsProps) => {
  const { t } = useTranslation();

  return (
    <div className="stats-grid">
      <div className="stat-card">
        <div className="stat-number">{stats.total}</div>
        <div className="stat-label">
          <img className="stat-icon" src={HistoryIcon} alt="" aria-hidden="true" />
          {isSeasonFiltered
            ? t('floorball.matches.stats.seasonMatches', 'Season Matches')
            : t('floorball.matches.stats.totalMatches', 'Total Matches')}
        </div>
      </div>
      <div className="stat-card stat-card--clickable" onClick={onCompletedClick}>
        <div className="stat-number">{stats.completed}</div>
        <div className="stat-label">
          <img className="stat-icon" src={CheckIcon} alt="" aria-hidden="true" />
          {t('floorball.matches.stats.completed', 'Completed')}
        </div>
      </div>
      <div className="stat-card stat-card--clickable" onClick={onScheduledClick}>
        <div className="stat-number">{stats.scheduled}</div>
        <div className="stat-label">
          <img className="stat-icon" src={ScheduleIcon} alt="" aria-hidden="true" />
          {t('floorball.matches.stats.scheduled', 'Scheduled')}
        </div>
      </div>
      <div className="stat-card stat-card--clickable" onClick={onInProgressClick}>
        <div className="stat-number">{stats.inProgress}</div>
        <div className="stat-label">
          <img className="stat-icon" src={PendingIcon} alt="" aria-hidden="true" />
          {t('floorball.matches.stats.inProgress', 'In Progress')}
        </div>
      </div>
      <div className="stat-card stat-card--clickable" onClick={onCancelledClick}>
        <div className="stat-number">{stats.cancelled}</div>
        <div className="stat-label">
          <img className="stat-icon" src={CancelIcon} alt="" aria-hidden="true" />
          {t('floorball.matches.stats.cancelled', 'Cancelled')}
        </div>
      </div>
    </div>
  );
};

export default MatchStatsCards;
