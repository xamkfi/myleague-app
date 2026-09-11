import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import { formatDateTime } from '../../../ManageMatchPage/utils/matchFormatters';
import AdminMatchTable from '../../../../../../components/admin/AdminMatchTable';
import type { AdminAction } from '../../../../../../components/admin/adminTableTypes';

interface MatchTableProps {
  matches: FloorballMatchDto[];
  loading: boolean;
  onLiveMatch: (match: FloorballMatchDto) => void;
  onEditMatch: (match: FloorballMatchDto) => void;
  onOpenMatch: (match: FloorballMatchDto) => void;
  onStartMatch: (match: FloorballMatchDto) => void;
  onCancelMatch: (match: FloorballMatchDto) => void;
  onReactivateMatch: (match: FloorballMatchDto) => void;
  hideActions?: boolean;
}

const MatchTable = ({
  matches,
  loading,
  onLiveMatch,
  onEditMatch,
  onOpenMatch,
  onStartMatch,
  onCancelMatch,
  onReactivateMatch,
  hideActions = false,
}: MatchTableProps) => {
  const { t } = useTranslation();
  const byId = new Map(matches.map((match) => [match.id, match]));

  const getMatchStatusBadge = (status: string) => {
    const map: Record<string, { className: string; label: string }> = {
      Scheduled: {
        className: 'admin-badge admin-badge--info',
        label: t('floorball.matches.status.scheduled', 'Scheduled'),
      },
      InProgress: {
        className: 'admin-badge admin-badge--active',
        label: t('floorball.matches.status.inProgress', 'In Progress'),
      },
      Completed: {
        className: 'admin-badge admin-badge--completed',
        label: t('floorball.matches.status.completed', 'Completed'),
      },
      Cancelled: {
        className: 'admin-badge admin-badge--danger',
        label: t('floorball.matches.status.cancelled', 'Cancelled'),
      },
      Postponed: {
        className: 'admin-badge admin-badge--warning',
        label: t('floorball.matches.status.postponed', 'Postponed'),
      },
    };
    return map[status] ?? { className: 'admin-badge', label: status };
  };

  const getActions = (match: FloorballMatchDto): AdminAction[] => {
    const actions: AdminAction[] = [
      {
        label: t('floorball.matches.actions.open', 'Open Match'),
        onClick: () => onOpenMatch(match),
      },
    ];

    if (match.status === 'InProgress') {
      actions.push({
        label: t('floorball.matches.actions.live', 'Live View'),
        onClick: () => onLiveMatch(match),
      });
      actions.push({
        label: t('common.edit'),
        onClick: () => onEditMatch(match),
      });
    } else {
      actions.push({
        label: t('floorball.matches.actions.manage', 'Manage'),
        onClick: () => onEditMatch(match),
      });
    }

    if ((match.status === 'Scheduled' || match.status === 'Postponed') && onStartMatch) {
      actions.push({
        label: t('floorball.matches.actions.start', 'Start Match'),
        onClick: () => onStartMatch(match),
        variant: 'status',
      });
    }

    if (match.status === 'Cancelled') {
      actions.push({
        label: t('floorball.matches.actions.reactivate', 'Reactivate Match'),
        onClick: () => onReactivateMatch(match),
      });
    }

    if (
      match.status !== 'Cancelled' &&
      match.status !== 'Completed' &&
      match.status !== 'InProgress'
    ) {
      actions.push({
        label: t('floorball.matches.actions.cancel', 'Cancel Match'),
        onClick: () => onCancelMatch(match),
        variant: 'danger',
      });
    }

    return actions;
  };

  return (
    <AdminMatchTable
      sport="floorball"
      matches={matches.map((match) => ({
        id: match.id,
        homeTeamName: match.homeTeamName ?? '',
        awayTeamName: match.awayTeamName ?? '',
        homeTeamId: match.homeTeamId,
        awayTeamId: match.awayTeamId,
        competitionName: match.competitionName || '-',
        scheduledDateTime: match.scheduledDateTime,
        venue: match.venue,
        homeScore: match.homeScore,
        awayScore: match.awayScore,
        status: match.status,
      }))}
      labels={{
        loading: t('floorball.matches.loading', 'Loading matches...'),
        noMatchesFound: t('floorball.matches.noMatchesFound', 'No matches found'),
        match: t('floorball.matches.columns.match', 'Match'),
        season: t('floorball.matches.columns.season', 'Season'),
        dateTime: t('floorball.matches.columns.dateTime', 'Date & Time'),
        venue: t('floorball.matches.columns.venue', 'Venue'),
        score: t('floorball.matches.columns.score', 'Score'),
        status: t('floorball.matches.columns.status', 'Status'),
        tbd: t('floorball.matches.tbd', 'TBD'),
        actionsMenu: t('floorball.matches.actions.menu', 'Match actions menu'),
      }}
      loading={loading}
      hideActions={hideActions}
      formatDateTime={formatDateTime}
      getStatusBadge={getMatchStatusBadge}
      getActions={(row) => {
        const match = byId.get(row.id);
        return match ? getActions(match) : [];
      }}
      onRowClick={(row) => {
        const match = byId.get(row.id);
        if (match) onLiveMatch(match);
      }}
    />
  );
};

export default MatchTable;
