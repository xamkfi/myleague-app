import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';

const CompletedMatchesPage = () => {
  return (
    <MatchesByStatusPage
      status={FloorballMatchStatus.Completed}
      title="Completed Matches"
      sectionType="completed"
    />
  );
};

export default CompletedMatchesPage;