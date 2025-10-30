import React from 'react';
import type { FloorballMatchDto } from "../../../types/floorball/floorballTypes";
import MatchesList from '../../../components/MatchesList/MatchesList';

interface FixturesSectionProps {
  matchesLoading: boolean;
  matchesError: string | null;
  matches: FloorballMatchDto[] | null;
  currentPage: number;
  totalPages: number;
  handlePageChange: (page: number) => void;
}

export default function FixturesSection(props: FixturesSectionProps) {
  return (
    <MatchesList
      variant="fixtures"
      matchesLoading={props.matchesLoading}
      matchesError={props.matchesError}
      matches={props.matches}
      currentPage={props.currentPage}
      totalPages={props.totalPages}
      handlePageChange={props.handlePageChange}
    />
  );
}
