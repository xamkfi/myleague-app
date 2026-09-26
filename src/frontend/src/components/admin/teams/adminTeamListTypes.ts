export const ALL_TEAMS_SEASON = 'all';

export interface NamedOption {
  id: string;
  name: string;
}

export interface AdminSeasonChoice {
  id: string;
  name: string;
  isActive: boolean;
  startDate: string;
  divisions: NamedOption[];
  divisionNameByTeamId: Map<string, string>;
}

export interface AdminTeamListQuery {
  page: number;
  pageSize: number;
  searchTerm?: string;
  clubId?: string;
  categories: string[];
  competitionId?: string;
  competitionDivisionId?: string;
  homeDivisionId?: string;
}

export interface AdminTeamListPage<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
}

export interface AdminTeamsTableSlot<T> {
  teams: T[];
  loading: boolean;
  viewMode: 'season' | 'catalog';
  seasonId: string | null;
  divisionNameByTeamId: Map<string, string>;
  clubNameById: Map<string, string>;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  onEdit: (teamId: string) => void;
  onEditRoster: (teamId: string) => void;
  onDelete: (teamId: string, teamName: string) => void;
  bulkActionLabel: string;
  pagination: {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
  };
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}
