
const NewsCardSkeleton = () => {
  return (
    <div className="news-card skeleton-card">
      <div className="news-card-image-container skeleton-image">
        <div className="skeleton-placeholder"></div>
      </div>
      <div className="news-card-content">
        <div className="skeleton-date"></div>
        <div className="skeleton-title"></div>
        <div className="skeleton-tags">
          <div className="skeleton-tag"></div>
          <div className="skeleton-tag"></div>
        </div>
      </div>
    </div>
  );
};

export default NewsCardSkeleton; 