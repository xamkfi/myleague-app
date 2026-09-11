import { useTranslation } from 'react-i18next';
import type { FootballMatchDto } from '../../../../../../types/football/footballTypes';
import { formatDateTime } from '../../../ManageMatchPage/utils/matchFormatters';
import AdminMatchTable from '../../../../../../components/admin/AdminMatchTable';
import type { AdminAction } from '../../../../../../components/admin/adminTableTypes';

interface MatchTableProps {
  matches: FootballMatchDto[];
  loading: boolean;
  onLiveMatch: (match: FootballMatchDto) => void;
  onEditMatch: (match: FootballMatchDto) => void;
  onOpenMatch: (match: FootballMatchDto) => void;
  onStartMatch: (match: FootballMatchDto) => void;
  onCancelMatch: (match: FootballMatchDto) => void;
  onReactivateMatch: (match: FootballMatchDto) => void;
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
        label: t('football.matches.status.scheduled', 'Scheduled'),
      },
      InProgress: {
        className: 'admin-badge admin-badge--active',
        label: t('football.matches.status.inProgress', 'In Progress'),
      },
      Completed: {
        className: 'admin-badge admin-badge--completed',
        label: t('football.matches.status.completed', 'Completed'),
      },
      Cancelled: {
        className: 'admin-badge admin-badge--danger',
        label: t('football.matches.status.cancelled', 'Cancelled'),
      },
      Postponed: {
        className: 'admin-badge admin-badge--warning',
        label: t('football.matches.status.postponed', 'Postponed'),
      },
    };
    return map[status] ?? { className: 'admin-badge', label: status };
  };

  const getActions = (match: FootballMatchDto): AdminAction[] => {
    const actions: AdminAction[] = [
      {
        label: t('football.matches.actions.open', 'Open Match'),
        onClick: () => onOpenMatch(match),
      },
    ];

    if (match.status === 'InProgress') {
      actions.push({
        label: t('football.matches.actions.live', 'Live View'),
        onClick: () => onLiveMatch(match),
      });
      actions.push({
        label: t('common.edit'),
        onClick: () => onEditMatch(match),
      });
    } else {
      actions.push({
        label: t('football.matches.actions.manage', 'Manage'),
        onClick: () => onEditMatch(match),
      });
    }

    if ((match.status === 'Scheduled' || match.status === 'Postponed') && onStartMatch) {
      actions.push({
        label: t('football.matches.actions.start', 'Start Match'),
        onClick: () => onStartMatch(match),
        variant: 'status',
      });
    }

    if (match.status === 'Cancelled') {
      actions.push({
        label: t('football.matches.actions.reactivate', 'Reactivate Match'),
        onClick: () => onReactivateMatch(match),
      });
    }

    if (
      match.status !== 'Cancelled' &&
      match.status !== 'Completed' &&
      match.status !== 'InProgress'
    ) {
      actions.push({
        label: t('football.matches.actions.cancel', 'Cancel Match'),
        onClick: () => onCancelMatch(match),
        variant: 'danger',
      });
    }

    return actions;
  };

  return (
    <AdminMatchTable
      sport="football"
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
        loading: t('football.matches.loading', 'Loading matches...'),
        noMatchesFound: t('football.matches.noMatchesFound', 'No matches found'),
        match: t('football.matches.columns.match', 'Match'),
        season: t('football.matches.columns.season', 'Season'),
        dateTime: t('football.matches.columns.dateTime', 'Date & Time'),
        venue: t('football.matches.columns.venue', 'Venue'),
        score: t('football.matches.columns.score', 'Score'),
        status: t('football.matches.columns.status', 'Status'),
        tbd: t('football.matches.tbd', 'TBD'),
        actionsMenu: t('football.matches.actions.menu', 'Match actions menu'),
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
