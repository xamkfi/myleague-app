import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import './MatchStatsCards.scss';
import CheckIcon from '../../../../../../assets/basicIcons/check_circle.svg';
import ScheduleIcon from '../../../../../../assets/basicIcons/schedule.svg';
import PendingIcon from '../../../../../../assets/basicIcons/pending.svg';
import CancelIcon from '../../../../../../assets/basicIcons/cancel.svg';
import HistoryIcon from '../../../../../../assets/basicIcons/history.svg';

interface MatchStatsCardsProps {
  allMatches: FloorballMatchDto[];
  filteredMatches: {
    ongoing: FloorballMatchDto[];
    scheduled: FloorballMatchDto[];
    completed: FloorballMatchDto[];
    cancelled: FloorballMatchDto[];
  };
  selectedSeasonId: string;
  onCompletedClick?: () => void;
  onScheduledClick?: () => void;
  onInProgressClick?: () => void;
  onCancelledClick?: () => void;
}

const MatchStatsCards = ({
  allMatches,
  filteredMatches,
  selectedSeasonId,
  onCompletedClick,
  onScheduledClick,
  onInProgressClick,
  onCancelledClick
}: MatchStatsCardsProps) => {
  const { t } = useTranslation();
  const getMatchCountByStatus = (status: string) => {
    return allMatches.filter(m => m.status === status).length;
  };

  return (
    <div className="stats-grid">
      <div className="stat-card">
        <div className="stat-number">{filteredMatches.ongoing.length + filteredMatches.scheduled.length + filteredMatches.completed.length + filteredMatches.cancelled.length}</div>
        <div className="stat-label"><img className="stat-icon" src={HistoryIcon} alt="" aria-hidden="true" />{selectedSeasonId ? 'Season Matches' : 'Total Matches'}</div>
      </div>
      <div className="stat-card" onClick={onCompletedClick}>
        <div className="stat-number">{getMatchCountByStatus('Completed')}</div>
        <div className="stat-label"><img className="stat-icon" src={CheckIcon} alt="" aria-hidden="true" />{t('floorball.matches.stats.completed', 'Completed')}</div>
      </div>
      <div className="stat-card" onClick={onScheduledClick}>
        <div className="stat-number">{getMatchCountByStatus('Scheduled')}</div>
        <div className="stat-label"><img className="stat-icon" src={ScheduleIcon} alt="" aria-hidden="true" />{t('floorball.matches.stats.scheduled', 'Scheduled')}</div>
      </div>
      <div className="stat-card" onClick={onInProgressClick}>
        <div className="stat-number">{getMatchCountByStatus('InProgress')}</div>
        <div className="stat-label"><img className="stat-icon" src={PendingIcon} alt="" aria-hidden="true" />{t('floorball.matches.stats.inProgress', 'In Progress')}</div>
      </div>
      <div className="stat-card" onClick={onCancelledClick}>
        <div className="stat-number">{getMatchCountByStatus('Cancelled')}</div>
        <div className="stat-label"><img className="stat-icon" src={CancelIcon} alt="" aria-hidden="true" />{t('floorball.matches.stats.cancelled', 'Cancelled')}</div>
      </div>
    </div>
  );
};

export default MatchStatsCards; 