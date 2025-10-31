import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

const InProgressMatchesPage = () => {
  return (
    <PageTemplate title={'In progress matches'}>
    <MatchesByStatusPage
      status={FloorballMatchStatus.InProgress}
      title="In Progress Matches"
      sectionType="ongoing"
    />
    </PageTemplate>
  );
};

export default InProgressMatchesPage;