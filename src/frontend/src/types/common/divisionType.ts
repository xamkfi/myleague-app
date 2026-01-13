import type { SportsCategory } from './sports';

export interface DivisionType {
  id: string;
  name: string;
  description: string;
  level: number;
  sportType: SportsCategory;
  isActive: boolean;
  createdDate: string;
}

export interface CreateDivisionInput {
  name: string;
  description: string;
  level: number;
  sportType: SportsCategory;
}

export interface UpdateDivisionInput {
  name: string;
  description: string;
  level: number;
}