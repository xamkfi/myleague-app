import MatchBrowser from './MatchBrowser';

interface MatchData {
  id: string;
  homeTeam: string;
  awayTeam: string;
  homeScore: string;
  awayScore: string;
  date: string;
  link: string;
}

interface MatchSelectionHeaderProps {
  onInsertMatches: (matches: MatchData[]) => void;
}

export default function MatchSelectionHeader({ onInsertMatches }: MatchSelectionHeaderProps) {
  return (
    <div className="match-selection-header">
      <MatchBrowser onInsertMatches={onInsertMatches} />
    </div>
  );
} 