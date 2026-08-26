import { useTranslation } from 'react-i18next';
import type { HockeyMatchDto } from '../../../../../../types/hockey/hockeyTypes';
import { isHockeyMatchFinished, isHockeyMatchLive } from '../../../../../../types/hockey/hockeyTypes';
import { formatHockeyDateTime } from '../../../../../../utils/hockeyLookups';
import AdminMatchTable from '../../../../../../components/admin/AdminMatchTable';
import type { AdminAction } from '../../../../../../components/admin/adminTableTypes';

interface MatchTableProps {
  matches: HockeyMatchDto[];
  teamNames: Map<string, string>;
  competitionNames: Map<string, string>;
  loading: boolean;
  onLiveMatch: (match: HockeyMatchDto) => void;
  onEditMatch: (match: HockeyMatchDto) => void;
  onOpenMatch: (match: HockeyMatchDto) => void;
  onStartMatch: (match: HockeyMatchDto) => void;
  onCancelMatch: (match: HockeyMatchDto) => void;
  onReactivateMatch: (match: HockeyMatchDto) => void;
  hideActions?: boolean;
}

function MatchTable({
  matches,
  teamNames,
  competitionNames,
  loading,
  onLiveMatch,
  onEditMatch,
  onOpenMatch,
  onStartMatch,
  onCancelMatch,
  onReactivateMatch,
  hideActions = false,
}: MatchTableProps) {
  const { t } = useTranslation();
  const byId = new Map(matches.map((match) => [match.id, match]));

  const getMatchStatusBadge = (status: string): { className: string; label: string } => {
    if (isHockeyMatchLive(status)) {
      return {
        className: 'admin-badge admin-badge--active',
        label: status === 'InProgress'
          ? t('hockey.matches.status.inProgress', 'In Progress')
          : t(`hockey.matches.status.${status.charAt(0).toLowerCase()}${status.slice(1)}`, status),
      };
    }
    const map: Record<string, { className: string; label: string }> = {
      Scheduled: {
        className: 'admin-badge admin-badge--info',
        label: t('hockey.matches.status.scheduled', 'Scheduled'),
      },
      Completed: {
        className: 'admin-badge admin-badge--completed',
        label: t('hockey.matches.status.completed', 'Completed'),
      },
      Cancelled: {
        className: 'admin-badge admin-badge--danger',
        label: t('hockey.matches.status.cancelled', 'Cancelled'),
      },
      Postponed: {
        className: 'admin-badge admin-badge--warning',
        label: t('hockey.matches.status.postponed', 'Postponed'),
      },
    };
    return map[status] ?? { className: 'admin-badge', label: status };
  };

  const getActions = (match: HockeyMatchDto): AdminAction[] => {
    const actions: AdminAction[] = [
      {
        label: t('hockey.matches.actions.open', 'Open Match'),
        onClick: () => onOpenMatch(match),
      },
    ];

    if (isHockeyMatchLive(match.status)) {
      actions.push({
        label: t('hockey.matches.actions.live', 'Live View'),
        onClick: () => onLiveMatch(match),
      });
      actions.push({
        label: t('common.edit'),
        onClick: () => onEditMatch(match),
      });
    } else {
      actions.push({
        label: t('hockey.matches.actions.manage', 'Manage'),
        onClick: () => onEditMatch(match),
      });
    }

    if (match.status === 'Scheduled' || match.status === 'Postponed') {
      actions.push({
        label: t('hockey.matches.actions.start', 'Start Match'),
        onClick: () => onStartMatch(match),
        variant: 'status',
      });
    }

    if (match.status === 'Cancelled') {
      actions.push({
        label: t('hockey.matches.actions.reactivate', 'Reactivate Match'),
        onClick: () => onReactivateMatch(match),
      });
    }

    if (match.status !== 'Cancelled' && !isHockeyMatchFinished(match.status) && !isHockeyMatchLive(match.status)) {
      actions.push({
        label: t('hockey.matches.actions.cancel', 'Cancel Match'),
        onClick: () => onCancelMatch(match),
        variant: 'danger',
      });
    }

    return actions;
  };

  return (
    <AdminMatchTable
      sport="hockey"
      matches={matches.map((match) => ({
        id: match.id,
        homeTeamName: match.homeTeamId ? teamNames.get(match.homeTeamId) ?? '' : '',
        awayTeamName: match.awayTeamId ? teamNames.get(match.awayTeamId) ?? '' : '',
        homeTeamId: match.homeTeamId,
        awayTeamId: match.awayTeamId,
        competitionName: match.competitionId ? competitionNames.get(match.competitionId) ?? '-' : '-',
        scheduledDateTime: match.scheduledStartTime,
        venue: match.venue,
        homeScore: match.homeScore,
        awayScore: match.awayScore,
        status: String(match.status),
      }))}
      labels={{
        loading: t('hockey.matches.loading', 'Loading matches...'),
        noMatchesFound: t('hockey.matches.noMatchesFound', 'No matches found'),
        match: t('hockey.matches.columns.match', 'Match'),
        season: t('hockey.matches.columns.season', 'Season'),
        dateTime: t('hockey.matches.columns.dateTime', 'Date & Time'),
        venue: t('hockey.matches.columns.venue', 'Venue'),
        score: t('hockey.matches.columns.score', 'Score'),
        status: t('hockey.matches.columns.status', 'Status'),
        tbd: t('hockey.matches.tbd', 'TBD'),
        actionsMenu: t('hockey.matches.actions.menu', 'Match actions menu'),
      }}
      loading={loading}
      hideActions={hideActions}
      formatDateTime={formatHockeyDateTime}
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
}

export default MatchTable;
