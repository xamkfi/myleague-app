import { useTranslation } from 'react-i18next';

interface PaginationControlsProps {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

const PaginationControls = ({
  currentPage,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  onPageSizeChange
}: PaginationControlsProps) => {
  const { t } = useTranslation();

  return (
    <div className="pagination-container">
      <div className="page-size-selector">
        <label htmlFor="pageSize">{t('common.itemsPerPage', 'Items per page:')}</label>
        <select 
          id="pageSize"
          value={pageSize} 
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
          className="page-size-select"
        >
          <option value={5}>5</option>
          <option value={10}>10</option>
          <option value={25}>25</option>
          <option value={50}>50</option>
          <option value={100}>100</option>
        </select>
      </div>

      {totalPages > 1 && (
        <div className="pagination">
          <button
            className="pagination-button"
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
          >
            {t('common.previous', 'Previous')}
          </button>
          
          <span className="pagination-info">
            {t('common.pageInfo', { current: currentPage, total: totalPages })}
          </span>
          
          <button
            className="pagination-button"
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
          >
            {t('common.next', 'Next')}
          </button>
        </div>
      )}

      <div className="pagination-summary">
        <span>
          {totalCount > 0 
            ? `Showing ${Math.min((currentPage - 1) * pageSize + 1, totalCount)}-${Math.min(currentPage * pageSize, totalCount)} of ${totalCount} items`
            : 'No items to show'
          }
        </span>
      </div>
    </div>
  );
};

export default PaginationControls; 