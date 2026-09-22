import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ActionsDropdown from '../ActionsDropdown/ActionsDropdown';
import PlayerLink from '../SportLinks/PlayerLink';
import { getPlayerPath, type SportKind } from '../../utils/sportRoutes';
import type { ActivePlayerLicence } from '../../types/activePlayerLicence';
import type { AdminAction, AdminPlayerRow, AdminPlayerTableLabels } from './adminTableTypes';
import '../../styles/AdminTable.scss';

interface AdminPlayersTableProps {
  sport: SportKind;
  players: AdminPlayerRow[];
  labels: AdminPlayerTableLabels;
  selectedPlayers: Set<string>;
  onToggleSelection: (playerId: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  onAssignToTeam: (player: AdminPlayerRow) => void;
  onStatusChange: (player: AdminPlayerRow, isActive: boolean) => void;
  onDelete: (player: AdminPlayerRow) => void;
  extraActions?: (player: AdminPlayerRow) => AdminAction[];
}

export default function AdminPlayersTable({
  sport,
  players,
  labels,
  selectedPlayers,
  onToggleSelection,
  onSelectAll,
  onClearSelection,
  onAssignToTeam,
  onStatusChange,
  onDelete,
  extraActions,
}: AdminPlayersTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const formatLicence = (licence: ActivePlayerLicence): string => {
    if (!licence.competitionName) {
      return licence.teamName;
    }
    return `${licence.teamName} — ${licence.competitionName}`;
  };

  if (players.length === 0) {
    return <div className="no-data-state">{labels.noPlayers}</div>;
  }

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th className="admin-table__checkbox-col">
            <input
              type="checkbox"
              checked={players.length > 0 && players.every((player) => selectedPlayers.has(player.id))}
              onChange={(event) => {
                if (event.target.checked) {
                  onSelectAll();
                } else {
                  onClearSelection();
                }
              }}
              title={labels.selectAll}
            />
          </th>
          <th>{labels.firstName}</th>
          <th>{labels.lastName}</th>
          <th>{labels.licences}</th>
          <th className="admin-table__actions-col">{labels.actions}</th>
        </tr>
      </thead>
      <tbody>
        {players.map((player) => {
          const publicPath = getPlayerPath(sport, player.id);

          return (
            <tr
              key={player.rowKey ?? player.id}
              className={`admin-table__row--clickable${selectedPlayers.has(player.id) ? ' admin-table__row--selected' : ''}`}
              onClick={() => onToggleSelection(player.id)}
            >
              <td className="admin-table__checkbox-col">
                <input
                  type="checkbox"
                  checked={selectedPlayers.has(player.id)}
                  onChange={() => onToggleSelection(player.id)}
                  onClick={(event) => event.stopPropagation()}
                />
              </td>
              <td className="admin-table__name">
                <PlayerLink sport={sport} playerId={player.id}>
                  {player.firstName}
                </PlayerLink>
              </td>
              <td className="admin-table__name">
                <PlayerLink sport={sport} playerId={player.id}>
                  {player.lastName}
                </PlayerLink>
              </td>
              <td>
                {player.licences.length === 0 ? (
                  <span className="admin-table__muted">{labels.noActiveLicences}</span>
                ) : (
                  <ul className="admin-table__licences">
                    {player.licences.map((licence) => (
                      <li key={`${licence.teamId}-${licence.competitionId ?? 'base'}`}>
                        {formatLicence(licence)}
                      </li>
                    ))}
                  </ul>
                )}
              </td>
              <td className="admin-table__actions-col" onClick={(event) => event.stopPropagation()}>
                <ActionsDropdown
                  actions={[
                    {
                      label: t('common.viewPublic'),
                      onClick: () => navigate(publicPath),
                    },
                    {
                      label: labels.assignToTeam,
                      onClick: () => onAssignToTeam(player),
                    },
                    {
                      label: player.isActive ? labels.deactivate : labels.activate,
                      onClick: () => onStatusChange(player, !player.isActive),
                      variant: 'status',
                    },
                    ...(extraActions ? extraActions(player) : []),
                    {
                      label: labels.delete,
                      onClick: () => onDelete(player),
                      variant: 'danger',
                    },
                  ]}
                  ariaLabel={labels.actionsMenu}
                />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
