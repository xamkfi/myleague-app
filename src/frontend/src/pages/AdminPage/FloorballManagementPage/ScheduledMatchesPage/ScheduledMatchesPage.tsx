import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';

const ScheduledMatchesPage = () => {
  return (
    <MatchesByStatusPage
      status={FloorballMatchStatus.Scheduled}
      title="Scheduled Matches"
      sectionType="scheduled"
    />
  );
};

export default ScheduledMatchesPage;