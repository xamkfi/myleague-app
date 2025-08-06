import { useTranslation } from 'react-i18next';
import './Pagination.scss';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  pageSizeOptions?: number[];
  showPageSizeSelector?: boolean;
  showSummary?: boolean;
  className?: string;
}

const Pagination = ({
  currentPage,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [5, 10, 25, 50],
  showPageSizeSelector = true,
  showSummary = true,
  className = ''
}: PaginationProps) => {
  const { t } = useTranslation();

  const renderPageNumbers = () => {
    const pages = [];
    const maxVisiblePages = 5;
    
    if (totalPages <= maxVisiblePages) {
      // Show all pages if total is small
      for (let i = 1; i <= totalPages; i++) {
        pages.push(
          <button
            key={i}
            className={`pagination-page-button ${currentPage === i ? 'active' : ''}`}
            onClick={() => onPageChange(i)}
          >
            {i}
          </button>
        );
      }
    } else {
      // Show first page
      pages.push(
        <button
          key={1}
          className={`pagination-page-button ${currentPage === 1 ? 'active' : ''}`}
          onClick={() => onPageChange(1)}
        >
          1
        </button>
      );

      // Show ellipsis if needed
      if (currentPage > 3) {
        pages.push(<span key="ellipsis1" className="pagination-ellipsis">...</span>);
      }

      // Show pages around current page
      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);
      
      for (let i = start; i <= end; i++) {
        if (i > 1 && i < totalPages) {
          pages.push(
            <button
              key={i}
              className={`pagination-page-button ${currentPage === i ? 'active' : ''}`}
              onClick={() => onPageChange(i)}
            >
              {i}
            </button>
          );
        }
      }

      // Show ellipsis if needed
      if (currentPage < totalPages - 2) {
        pages.push(<span key="ellipsis2" className="pagination-ellipsis">...</span>);
      }

      // Show last page
      if (totalPages > 1) {
        pages.push(
          <button
            key={totalPages}
            className={`pagination-page-button ${currentPage === totalPages ? 'active' : ''}`}
            onClick={() => onPageChange(totalPages)}
          >
            {totalPages}
          </button>
        );
      }
    }

    return pages;
  };

  return (
    <div className={`pagination-container ${className}`}>
      {showPageSizeSelector && (
        <div className="page-size-selector">
          <label htmlFor="pageSize">{t('common.itemsPerPage', 'Items per page:')}</label>
          <select 
            id="pageSize"
            value={pageSize} 
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="page-size-select"
          >
            {pageSizeOptions.map(size => (
              <option key={size} value={size}>{size}</option>
            ))}
          </select>
        </div>
      )}

      {totalPages > 1 && (
        <div className="pagination">
          <button
            className="pagination-button pagination-prev"
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            aria-label={t('common.previous', 'Previous')}
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="15,18 9,12 15,6"></polyline>
            </svg>
            <span>{t('common.previous', 'Previous')}</span>
          </button>
          
          <div className="pagination-pages">
            {renderPageNumbers()}
          </div>
          
          <button
            className="pagination-button pagination-next"
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
            aria-label={t('common.next', 'Next')}
          >
            <span>{t('common.next', 'Next')}</span>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="9,18 15,12 9,6"></polyline>
            </svg>
          </button>
        </div>
      )}

      {showSummary && (
        <div className="pagination-summary">
          <span>
            {totalCount > 0 
              ? t('common.showingItems', 'Showing {{start}}-{{end}} of {{total}} items', {
                  start: Math.min((currentPage - 1) * pageSize + 1, totalCount),
                  end: Math.min(currentPage * pageSize, totalCount),
                  total: totalCount
                })
              : t('common.noItemsToShow', 'No items to show')
            }
          </span>
        </div>
      )}
    </div>
  );
};

export default Pagination; 