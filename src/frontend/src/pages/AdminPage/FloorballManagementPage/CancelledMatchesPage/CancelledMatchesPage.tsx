import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';

const CancelledMatchesPage = () => {
  return (
    <MatchesByStatusPage
      status={FloorballMatchStatus.Cancelled}
      title="Cancelled Matches"
      sectionType="cancelled"
    />
  );
};

export default CancelledMatchesPage;