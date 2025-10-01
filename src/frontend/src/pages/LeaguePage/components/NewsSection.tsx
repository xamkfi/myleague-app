import './NewsSection.scss';

export default function NewsSection() {
  return (
    <div className="news-section">
      <h2>League News</h2>
      <div className="news-content">
        <div className="news-list">
          <div className="news-item">
            <div className="news-image">
              <div className="image-placeholder">📰</div>
            </div>
            <div className="news-content">
              <div className="news-meta">
                <span className="news-date">July 9, 2025</span>
                <span className="news-category">Championship</span>
              </div>
              <h3 className="news-title">FC Alapiha Crowned League Champions</h3>
              <p className="news-excerpt">
                FC Alapiha secured their first league championship with a dominant 3-1 victory over Aurora in the final match. 
                The team showed exceptional performance throughout the season...
              </p>
              <div className="news-footer">
                <span className="read-more">Read More →</span>
              </div>
            </div>
          </div>

          <div className="news-item">
            <div className="news-image">
              <div className="image-placeholder">🏆</div>
            </div>
            <div className="news-content">
              <div className="news-meta">
                <span className="news-date">July 8, 2025</span>
                <span className="news-category">Awards</span>
              </div>
              <h3 className="news-title">Season Awards Ceremony Announced</h3>
              <p className="news-excerpt">
                The annual season awards ceremony will take place next week to honor the best players, coaches, and teams 
                of the 2025/2026 season. All teams are invited to attend...
              </p>
              <div className="news-footer">
                <span className="read-more">Read More →</span>
              </div>
            </div>
          </div>

          <div className="news-item">
            <div className="news-image">
              <div className="image-placeholder">📊</div>
            </div>
            <div className="news-content">
              <div className="news-meta">
                <span className="news-date">July 7, 2025</span>
                <span className="news-category">Statistics</span>
              </div>
              <h3 className="news-title">Season Statistics Released</h3>
              <p className="news-excerpt">
                Complete season statistics have been published, showing impressive numbers across all teams. 
                Top scorers, assist leaders, and defensive records are now available...
              </p>
              <div className="news-footer">
                <span className="read-more">Read More →</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
