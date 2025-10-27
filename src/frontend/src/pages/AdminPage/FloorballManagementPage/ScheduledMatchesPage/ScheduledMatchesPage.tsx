import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

const ScheduledMatchesPage = () => {
  return (
    <PageTemplate title={'Scheduled matches'}>
    <MatchesByStatusPage
      status={FloorballMatchStatus.Scheduled}
      title="Scheduled Matches"
      sectionType="scheduled"
    />
    </PageTemplate>
  );
};

export default ScheduledMatchesPage;