import MatchBrowser from './MatchBrowser';
import type { NewsMatchOption } from '../../../../api/news/newsMatchService';
import type { SportKind } from '../../../../utils/sportRoutes';
import '../styles/MatchSelectionHeader.scss';

interface MatchSelectionHeaderProps {
  onInsertMatches: (matches: NewsMatchOption[]) => void;
  defaultSport?: SportKind;
}

export default function MatchSelectionHeader({ onInsertMatches, defaultSport }: MatchSelectionHeaderProps) {
  return (
    <div className="match-selection-header">
      <MatchBrowser onInsertMatches={onInsertMatches} defaultSport={defaultSport} />
    </div>
  );
}
