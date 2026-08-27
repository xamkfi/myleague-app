import type { SportsCategory } from '../common/sports';

export interface SeasonContentBlockDto {
  id: string;
  sport: SportsCategory;
  competitionId: string;
  seasonYear: string;
  title: string;
  contentHtml: string;
  sortOrder: number;
  lastModifiedBy: string | null;
  updatedAt: string;
}

export interface CreateSeasonContentBlockRequest {
  sport: SportsCategory;
  competitionId: string;
  seasonYear: string;
  title: string;
  contentHtml: string;
  sortOrder: number;
}

export interface UpdateSeasonContentBlockRequest {
  title: string;
  contentHtml: string;
  sortOrder: number;
}
