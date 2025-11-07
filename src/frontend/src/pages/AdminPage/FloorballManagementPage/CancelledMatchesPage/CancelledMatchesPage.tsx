import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

const CancelledMatchesPage = () => {
  return (
    <PageTemplate title={'Cancelled matches'}>
    <MatchesByStatusPage
      status={FloorballMatchStatus.Cancelled}
      title="Cancelled Matches"
      sectionType="cancelled"
    />
    </PageTemplate>
  );
};

export default CancelledMatchesPage;