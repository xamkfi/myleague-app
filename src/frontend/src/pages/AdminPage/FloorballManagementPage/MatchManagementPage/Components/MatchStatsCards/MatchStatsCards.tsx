import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import './MatchStatsCards.scss';

interface MatchStatsCardsProps {
  allMatches: FloorballMatchDto[];
  filteredMatches: {
    ongoing: FloorballMatchDto[];
    scheduled: FloorballMatchDto[];
    completed: FloorballMatchDto[];
    cancelled: FloorballMatchDto[];
  };
  selectedSeasonId: string;
  onCreateNew?: () => void;
  onCompletedClick?: () => void;
  onScheduledClick?: () => void;
  onInProgressClick?: () => void;
  onCancelledClick?: () => void;
}

const MatchStatsCards = ({
  allMatches,
  filteredMatches,
  selectedSeasonId,
  onCreateNew,
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
        <div className="stat-label">{selectedSeasonId ? 'Season Matches' : 'Total Matches'}</div>
      </div>
      <div className="stat-card" onClick={onCompletedClick}>
        <div className="stat-number">{getMatchCountByStatus('Completed')}</div>
        <div className="stat-label">{t('floorball.matches.stats.completed', 'Completed')}</div>
        <div className="stat-indicator completed"></div>
      </div>
      <div className="stat-card" onClick={onScheduledClick}>
        <div className="stat-number">{getMatchCountByStatus('Scheduled')}</div>
        <div className="stat-label">{t('floorball.matches.stats.scheduled', 'Scheduled')}</div>
        <div className="stat-indicator scheduled"></div>
      </div>
      <div className="stat-card" onClick={onInProgressClick}>
        <div className="stat-number">{getMatchCountByStatus('InProgress')}</div>
        <div className="stat-label">{t('floorball.matches.stats.inProgress', 'In Progress')}</div>
        <div className="stat-indicator progress"></div>
      </div>
      <div className="stat-card" onClick={onCancelledClick}>
        <div className="stat-number">{getMatchCountByStatus('Cancelled')}</div>
        <div className="stat-label">{t('floorball.matches.stats.cancelled', 'Cancelled')}</div>
        <div className="stat-indicator cancelled"></div>
      </div>
      <div className="stat-card stat-card--create" onClick={onCreateNew}>
        <div className="stat-number stat-number--plus">+</div>
        <div className="stat-label">{t('floorball.matches.stats.createNew', 'Create New')}</div>
      </div>
    </div>
  );
};

export default MatchStatsCards; 