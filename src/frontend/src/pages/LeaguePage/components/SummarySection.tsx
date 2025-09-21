import './SummarySection.scss';

export default function SummarySection() {
  return (
    <div className="summary-section">
      <h2>League Summary</h2>
      <div className="summary-content">
        <div className="league-overview">
          <div className="overview-card">
            <h3>League Information</h3>
            <div className="info-grid">
              <div className="info-item">
                <span className="label">Season:</span>
                <span className="value">2025/2026</span>
              </div>
              <div className="info-item">
                <span className="label">Teams:</span>
                <span className="value">10</span>
              </div>
              <div className="info-item">
                <span className="label">Matches Played:</span>
                <span className="value">45</span>
              </div>
              <div className="info-item">
                <span className="label">Location:</span>
                <span className="value">Mikkeli Sports Center</span>
              </div>
            </div>
          </div>

          <div className="overview-card">
            <h3>Recent Activity</h3>
            <div className="activity-list">
              <div className="activity-item">
                <span className="activity-icon">⚽</span>
                <span className="activity-text">FC Alapiha defeated Aurora 3-1 in the final</span>
                <span className="activity-time">2 hours ago</span>
              </div>
              <div className="activity-item">
                <span className="activity-icon">🏆</span>
                <span className="activity-text">Championship trophy ceremony completed</span>
                <span className="activity-time">1 day ago</span>
              </div>
              <div className="activity-item">
                <span className="activity-icon">📊</span>
                <span className="activity-text">Season statistics updated</span>
                <span className="activity-time">2 days ago</span>
              </div>
            </div>
          </div>
        </div>

        <div className="quick-stats">
          <div className="stat-card">
            <div className="stat-icon">🏆</div>
            <div className="stat-content">
              <div className="stat-value">FC Alapiha</div>
              <div className="stat-label">Champions</div>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">⚽</div>
            <div className="stat-content">
              <div className="stat-value">127</div>
              <div className="stat-label">Goals Scored</div>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">👥</div>
            <div className="stat-content">
              <div className="stat-value">150</div>
              <div className="stat-label">Players</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
