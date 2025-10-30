
import type { FloorballMatchDto, FloorballTeam } from "../../../types/floorball/floorballTypes";
import MatchesList from '../../../components/MatchesList/MatchesList';

interface ResultsSectionProps {
   matchesLoading: boolean,
   matchesError: string | null,
   matches: FloorballMatchDto[] | null
   team: FloorballTeam | null
   currentPage: number
   totalPages: number
   handlePageChange: (page: number) => void
}

export default function ResultsSection(props: ResultsSectionProps) {
  return (
    <MatchesList
      variant="results"
      matchesLoading={props.matchesLoading}
      matchesError={props.matchesError}
      matches={props.matches}
      currentPage={props.currentPage}
      totalPages={props.totalPages}
      handlePageChange={props.handlePageChange}
      team={props.team}
    />
  );
}