export interface StatRowProps {
  label: string;
  home: number | string;
  away: number | string;
  homeValue: number;
  awayValue: number;
  total: number;
  isCentered?: boolean;
}

export default function StatRow({
  label,
  home,
  away,
  homeValue,
  awayValue,
  total,
  isCentered = label === 'Save Percentage'
}: StatRowProps) {
  return (
    <div className="stat-row">
      <div className="stat-values">
        <div className="home-value">{home}</div>
        <div className="stat-label">{label}</div>
        <div className="away-value">{away}</div>
      </div>
      <div className="stat-bars">
        <div className={`bar-container ${label === 'Save Percentage' ? 'centered' : ''}`}>
          {label === 'Save Percentage' && <div className="center-line" />}
          <div
            className="home-bar"
            style={{
              width: label === 'Save Percentage'
                ? `${(homeValue / 2)}%`
                : `${(homeValue / (total || 1)) * 100}%`
            }}
          />
          <div
            className="away-bar"
            style={{
              width: label === 'Save Percentage'
                ? `${(awayValue / 2)}%`
                : `${(awayValue / (total || 1)) * 100}%`
            }}
          />
        </div>
        <div className="percentage-values">
          {total > 0 ? (
            <>
              <span className="home-percentage">{(homeValue / (isCentered ? 100 : total) * 100).toFixed(1)}%</span>
              <span className="away-percentage">{(awayValue / (isCentered ? 100 : total) * 100).toFixed(1)}%</span>
            </>
          ) : (
            <>
              <span className="home-percentage">0.0%</span>
              <span className="away-percentage">0.0%</span>
            </>
          )}
        </div>
      </div>
    </div>
  );
}


