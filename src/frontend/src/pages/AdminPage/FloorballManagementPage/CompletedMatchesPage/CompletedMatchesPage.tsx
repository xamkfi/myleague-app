import MatchesByStatusPage from '../Components/MatchesByStatusPage';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

const CompletedMatchesPage = () => {
  return (
    <PageTemplate title={'Completed matches'}>
    <MatchesByStatusPage
      status={FloorballMatchStatus.Completed}
      title="Completed Matches"
      sectionType="completed"
    />
    </PageTemplate>
  );
};

export default CompletedMatchesPage;