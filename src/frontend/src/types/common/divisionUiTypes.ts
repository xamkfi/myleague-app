import type { SportsCategory } from './sports';

export type DivisionStatusFilter = 'all' | 'active' | 'inactive';

export interface DivisionFormState {
  name: string;
  description: string;
  level: string;
  sportType: SportsCategory | '';
}

