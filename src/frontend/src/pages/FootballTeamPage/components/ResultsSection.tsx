
import type { FootballMatchDto, FootballTeam } from "../../../types/football/footballTypes";
import MatchesList from '../../FootballLeaguePage/components/FootballMatchesList';

interface ResultsSectionProps {
   matchesLoading: boolean,
   matchesError: string | null,
   matches: FootballMatchDto[] | null
   team: FootballTeam | null
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