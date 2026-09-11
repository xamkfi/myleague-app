import { useTranslation } from 'react-i18next';
import './StatsBar.scss';

interface MatchStats {
  total: number;
  completed: number;
  scheduled: number;
  inProgress: number;
  cancelled: number;
}

interface StatsBarProps {
  stats: MatchStats;
  isSeasonFiltered: boolean;
}

const StatsBar = ({ stats, isSeasonFiltered }: StatsBarProps) => {
  const { t } = useTranslation();

  const items: { label: string; value: number; variant: string }[] = [
    {
      label: isSeasonFiltered
        ? t('floorball.matches.stats.seasonMatches', 'Season Matches')
        : t('floorball.matches.stats.totalMatches', 'Total Matches'),
      value: stats.total,
      variant: 'total',
    },
    {
      label: t('floorball.matches.stats.inProgress', 'In Progress'),
      value: stats.inProgress,
      variant: 'active',
    },
    {
      label: t('floorball.matches.stats.scheduled', 'Scheduled'),
      value: stats.scheduled,
      variant: 'info',
    },
    {
      label: t('floorball.matches.stats.completed', 'Completed'),
      value: stats.completed,
      variant: 'completed',
    },
    {
      label: t('floorball.matches.stats.cancelled', 'Cancelled'),
      value: stats.cancelled,
      variant: 'danger',
    },
  ];

  return (
    <div className="stats-bar">
      {items.map((item) => (
        <div key={item.variant} className={`stats-bar__item stats-bar__item--${item.variant}`}>
          <span className="stats-bar__value">{item.value}</span>
          <span className="stats-bar__label">{item.label}</span>
        </div>
      ))}
    </div>
  );
};

export default StatsBar;
