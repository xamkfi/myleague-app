import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';

const InProgressMatchesPage = () => {
  return (
    <MatchesByStatusPage
      status={FloorballMatchStatus.InProgress}
      title="In Progress Matches"
      sectionType="ongoing"
    />
  );
};

export default InProgressMatchesPage;