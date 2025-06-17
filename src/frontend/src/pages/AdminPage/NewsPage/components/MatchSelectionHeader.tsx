import MatchBrowser from './MatchBrowser';
import { type FloorballMatch } from '../../../../api/admin/News/GetMatchesService';

interface MatchSelectionHeaderProps {
  onInsertMatches: (matches: FloorballMatch[]) => void;
}

export default function MatchSelectionHeader({ onInsertMatches }: MatchSelectionHeaderProps) {
  return (
    <div className="match-selection-header">
      <MatchBrowser onInsertMatches={onInsertMatches} />
    </div>
  );
} 