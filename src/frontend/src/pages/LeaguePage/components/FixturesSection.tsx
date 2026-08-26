import SharedFixturesSection from '../../../components/FixturesSection/FixturesSection';
import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';

interface FixturesSectionProps {
  matchesLoading: boolean;
  matchesError: string | null;
  matches: FloorballMatchDto[] | null;
  currentPage: number;
  totalPages: number;
  handlePageChange: (page: number) => void;
}

export default function FixturesSection(props: FixturesSectionProps) {
  return <SharedFixturesSection sport="floorball" {...props} />;
}
