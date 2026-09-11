import SharedFixturesSection from '../../../components/FixturesSection/FixturesSection';
import type { FootballMatchDto } from '../../../types/football/footballTypes';

interface FixturesSectionProps {
  matchesLoading: boolean;
  matchesError: string | null;
  matches: FootballMatchDto[] | null;
  currentPage: number;
  totalPages: number;
  handlePageChange: (page: number) => void;
}

export default function FixturesSection(props: FixturesSectionProps) {
  return <SharedFixturesSection sport="football" {...props} />;
}
