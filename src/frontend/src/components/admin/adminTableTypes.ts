export interface AdminAction {
  label: string;
  onClick: () => void;
  variant?: 'default' | 'danger' | 'status';
  disabled?: boolean;
}

export interface AdminTablePagination {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
}

export interface AdminSeasonRow {
  id: string;
  name: string;
  teamCategory?: string | null;
  startDate: string;
  endDate: string;
  teamCount: number;
  isActive: boolean;
  isCompleted: boolean;
  divisions: Array<{ id: string; name: string }>;
}

export interface AdminSeasonTableLabels {
  name: string;
  division: string;
  startDate: string;
  endDate: string;
  teams: string;
  status: string;
  completed: string;
  active: string;
  inactive: string;
  deactivate: string;
  activate: string;
  complete: string;
  noDivisions: string;
  teamsCount: string;
  matchesInProgress: (count: number) => string;
  openEdit: string;
  actionsMenu: string;
}

export interface AdminTournamentRow {
  id: string;
  name: string;
  teamCategory?: string | null;
  startDate: string;
  endDate: string;
  teamCount: number;
  matchCount?: number;
  status: string;
  statusLabel: string;
  statusClassName: string;
  groups: Array<{ id: string; name: string }>;
}

export interface AdminTournamentTableLabels {
  name: string;
  groups: string;
  startDate: string;
  endDate: string;
  teams: string;
  matches: string;
  status: string;
  noGroups: string;
  teamsCount: (count: number) => string;
  matchesCount: (count: number) => string;
  matchesInProgress: (count: number) => string;
  openEdit: string;
  actionsMenu: string;
}

export interface AdminTeamRow {
  id: string;
  name: string;
  teamCategory?: string | null;
  clubName: string;
  divisionName: string;
  homeArena: string;
  hasActiveMembers: boolean;
  primaryJerseyColor: string;
  secondaryJerseyColor?: string | null;
}

export interface AdminTeamTableLabels {
  noTeams: string;
  selectAll: string;
  teamName: string;
  club: string;
  division: string;
  homeArena: string;
  activeMembers: string;
  actions: string;
  primary: string;
  secondary: string;
  hasMembers: string;
  noMembers: string;
  editTeamInfo: string;
  editRoster: string;
  delete: string;
  actionsMenu: string;
}

export interface AdminPlayerRow {
  id: string;
  rowKey?: string;
  teamId?: string;
  name: string;
  teamName: string;
  positionLabel: string;
  isActive: boolean;
}

export interface AdminPlayerTableLabels {
  noPlayers: string;
  selectAll: string;
  name: string;
  team: string;
  position: string;
  status: string;
  actions: string;
  assignToTeam: string;
  deactivate: string;
  activate: string;
  delete: string;
  actionsMenu: string;
}

export interface AdminMatchRow {
  id: string;
  homeTeamName: string;
  awayTeamName: string;
  homeTeamId?: string | null;
  awayTeamId?: string | null;
  competitionName: string;
  scheduledDateTime: string;
  venue?: string | null;
  homeScore?: number | null;
  awayScore?: number | null;
  status: string;
}

export interface AdminMatchTableLabels {
  loading: string;
  noMatchesFound: string;
  match: string;
  season: string;
  dateTime: string;
  venue: string;
  score: string;
  status: string;
  tbd: string;
  actionsMenu: string;
}
