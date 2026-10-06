export interface MatchStatusCountsDto {
  total: number;
  scheduled: number;
  postponed: number;
  inProgress: number;
  completed: number;
  cancelled: number;
}

export interface MatchStatusCountsRequest {
  competitionId?: string;
  searchQuery?: string;
  competitionType?: 'Season' | 'Tournament';
  includeDrafts?: boolean;
}
