import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { NewsArticleDto, PaginatedNewsResponse } from '../../../../api/news/newsService'; 
import { newsService, archiveNewsService, restoreNewsService, deleteNewsService } from '../../../../api/news/newsService';
import Pagination from '../../../../components/Pagination';
import ActionsDropdown from '../../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../styles/AdminTable.scss';
import "../styles/NewsList.scss";

interface NewsListProps {
  filters?: {
    category: string;
    sportCategory: string;
    searchTerm: string;
    includeArchived: boolean;
  };
}

const NewsList = ({ filters }: NewsListProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [newsArticles, setNewsArticles] = useState<NewsArticleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingArticle, setDeletingArticle] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  const fetchNewsArticles = useCallback(async (page: number = 1, size: number = 10) => {
    try {
      setLoading(true);
      const response = await newsService({
        category: filters?.category ?? '',
        sportCategory: filters?.sportCategory ?? '',
        searchTerm: filters?.searchTerm ?? '',
        includeArchived: filters?.includeArchived ?? true,
        page,
        pageSize: size
      });
      
      // Handle the paginated response structure
      if (response && typeof response === 'object' && 'pagination' in response) {
        // New paginated response format with pagination object
        const paginatedResponse = response as PaginatedNewsResponse;
        setNewsArticles(paginatedResponse.data);
        setTotalCount(paginatedResponse.pagination.totalCount);
        setTotalPages(paginatedResponse.pagination.totalPages);
        setCurrentPage(paginatedResponse.pagination.currentPage);
        setPageSize(paginatedResponse.pagination.pageSize);
      } else {
        // Fallback for old format
        const oldResponse = response as NewsArticleDto[];
        setNewsArticles(oldResponse);
        setTotalCount(oldResponse.length);
        setTotalPages(Math.ceil(oldResponse.length / pageSize));
      }
      setError(null);
    } catch (error) {
      console.error('Failed to fetch news articles:', error);
      setError(t('admin.news.errors.fetchFailed', 'Failed to fetch news articles'));
    } finally {
      setLoading(false);
    }
  }, [filters, pageSize, t]);

  useEffect(() => {
    fetchNewsArticles(currentPage, pageSize);
  }, [currentPage, pageSize, fetchNewsArticles]);

  // Reset to first page when filters change (only if filters are provided)
  useEffect(() => {
    if (filters) {
      setCurrentPage(1);
    }
  }, [filters]);

  // Clear selection when articles change
  useEffect(() => {
    setSelectedIds(new Set());
  }, [newsArticles]);

  // ── Selection handlers ──
  const handleToggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const handleSelectAll = () => {
    setSelectedIds(new Set(newsArticles.map((a) => a.id)));
  };

  const handleClearSelection = () => {
    setSelectedIds(new Set());
  };

  // ── Bulk action handlers ──
  const handleBulkArchive = async () => {
    const ids = Array.from(selectedIds);
    if (ids.length === 0) return;
    if (!window.confirm(t('admin.news.confirmBulkArchive', 'Are you sure you want to archive the selected articles?'))) return;

    try {
      await Promise.all(ids.map((id) => archiveNewsService(id)));
      setSelectedIds(new Set());
      fetchNewsArticles(currentPage, pageSize);
    } catch (err) {
      console.error('Bulk archive failed:', err);
      setError(t('admin.news.errors.bulkArchiveFailed', 'Failed to archive selected articles'));
    }
  };

  const handleBulkUnarchive = async () => {
    const ids = Array.from(selectedIds);
    if (ids.length === 0) return;
    if (!window.confirm(t('admin.news.confirmBulkUnarchive', 'Are you sure you want to unarchive the selected articles?'))) return;

    try {
      await Promise.all(ids.map((id) => restoreNewsService(id)));
      setSelectedIds(new Set());
      fetchNewsArticles(currentPage, pageSize);
    } catch (err) {
      console.error('Bulk unarchive failed:', err);
      setError(t('admin.news.errors.bulkUnarchiveFailed', 'Failed to unarchive selected articles'));
    }
  };

  const handleBulkDelete = async () => {
    const ids = Array.from(selectedIds);
    if (ids.length === 0) return;
    if (!window.confirm(t('admin.news.confirmBulkDelete', 'Are you sure you want to delete the selected articles?'))) return;

    try {
      await Promise.all(ids.map((id) => deleteNewsService(id)));
      setSelectedIds(new Set());
      fetchNewsArticles(currentPage, pageSize);
    } catch (err) {
      console.error('Bulk delete failed:', err);
      setError(t('admin.news.errors.bulkDeleteFailed', 'Failed to delete selected articles'));
    }
  };

  const handleEdit = (id: string) => {
    navigate(`/admin/news/edit/${id}`);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm(t('admin.news.confirmDelete', 'Are you sure you want to delete this news article?'))) {
      setDeletingArticle(id);
      try {
        await deleteNewsService(id);
        
        // Remove the article from current page
        const updatedArticles = newsArticles.filter(article => article.id !== id);
        setNewsArticles(updatedArticles);
        
        // If this was the last item on the page and not the first page, go to previous page
        if (updatedArticles.length === 0 && currentPage > 1) {
          setCurrentPage(currentPage - 1);
        } else {
          // Refresh the current page to get updated data
          fetchNewsArticles(currentPage, pageSize);
        }
        
        setError(null);
        console.log(t('admin.news.success.deleted', 'News article deleted successfully'));
      } catch (error) {
        console.error('Failed to delete news article:', error);
        setError(t('admin.news.errors.deleteFailed', 'Failed to delete news article'));
      } finally {
        setDeletingArticle(null);
      }
    }
  };

  const handleToggleArchive = async (id: string, currentStatus: boolean) => {
    const confirmMessage = currentStatus 
      ? t('admin.news.confirmUnarchive', 'Are you sure you want to unarchive this news article?')
      : t('admin.news.confirmArchive', 'Are you sure you want to archive this news article?');
    if (window.confirm(confirmMessage)) {
      try {
        if (currentStatus) {
          await restoreNewsService(id);
        } else {
          await archiveNewsService(id);
        }
        
        // Refresh the current page to get updated data
        fetchNewsArticles(currentPage, pageSize);
        
        setError(null);
        
        const successMessage = !currentStatus
          ? t('admin.news.success.archived', 'News article archived successfully')
          : t('admin.news.success.unarchived', 'News article unarchived successfully');
        console.log(successMessage);
      } catch (error) {
        console.error('Failed to update archive status:', error);
        setError(t('admin.news.errors.updateArchiveFailed', 'Failed to update archive status'));
      }
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  if (loading) {
    return <div className="admin-table__empty">{t('admin.news.loading', 'Loading news articles...')}</div>;
  }

  if (error) {
    return <div className="admin-table__empty" style={{ color: '#b91c1c' }}>{error}</div>;
  }

  const allSelected = newsArticles.length > 0 && newsArticles.every((a) => selectedIds.has(a.id));

  return (
    <div className="news-list">
      {/* Add Filtering here */}

      <BulkActionsBar
        selectedCount={selectedIds.size}
        totalCount={newsArticles.length}
        onSelectAll={handleSelectAll}
        onClearSelection={handleClearSelection}
        actions={[
          { label: t('admin.news.actions.archive', 'Archive'), onClick: handleBulkArchive, variant: 'status' },
          { label: t('admin.news.actions.unarchive', 'Unarchive'), onClick: handleBulkUnarchive, variant: 'status' },
          { label: t('admin.news.actions.delete', 'Delete'), onClick: handleBulkDelete, variant: 'danger' },
        ]}
      />

      <div className="admin-table__wrapper">
        <table className="admin-table">
          <thead>
            <tr>
              <th className="admin-table__checkbox-col">
                <input
                  type="checkbox"
                  checked={allSelected}
                  onChange={() => (allSelected ? handleClearSelection() : handleSelectAll())}
                />
              </th>
              <th>{t('admin.news.table.title', 'Title')}</th>
              <th>{t('admin.news.table.author', 'Author')}</th>
              <th>{t('admin.news.table.category', 'Category')}</th>
              <th>{t('admin.news.table.createdAt', 'Created')}</th>
              <th>{t('admin.news.table.archived', 'Archived')}</th>
              <th className="admin-table__actions-col">{t('admin.news.table.actions', 'Actions')}</th>
            </tr>
          </thead>
          <tbody>
            {newsArticles.map(article => (
              <tr
                key={article.id}
                className={selectedIds.has(article.id) ? 'admin-table__row--selected' : ''}
              >
                <td className="admin-table__checkbox-col">
                  <input
                    type="checkbox"
                    checked={selectedIds.has(article.id)}
                    onChange={() => handleToggleSelect(article.id)}
                  />
                </td>
                <td>
                  <div className="admin-table__name">{article.title}</div>
                </td>
                <td>{article.author || '-'}</td>
                <td>
                  <div className="categories">
                    {article.category && (
                      <span className="admin-tag admin-tag--blue">{article.category}</span>
                    )}
                    {article.sportCategory && (
                      <span className="admin-tag admin-tag--purple">{article.sportCategory}</span>
                    )}
                  </div>
                </td>
                <td>{formatDate(article.createdAt)}</td>
                <td>
                  <button
                    className={`admin-table__toggle-btn ${article.isArchived ? 'admin-table__toggle-btn--off' : 'admin-table__toggle-btn--on'}`}
                    onClick={() => handleToggleArchive(article.id, article.isArchived)}
                    title={t('admin.news.actions.toggleArchive', 'Click to toggle archive status')}
                  >
                    <span>{article.isArchived ? '📁' : '📄'}</span>
                    <span>
                      {article.isArchived 
                        ? t('admin.news.status.archived', 'Yes')
                        : t('admin.news.status.notArchived', 'No')}
                    </span>
                  </button>
                </td>
                <td className="admin-table__actions-col">
                  <ActionsDropdown
                    ariaLabel={t('admin.news.actions.menu', 'News actions menu')}
                    actions={[
                      { label: t('admin.news.actions.edit', 'Edit'), onClick: () => handleEdit(article.id) },
                      {
                        label: article.isArchived
                          ? t('admin.news.actions.unarchive', 'Unarchive')
                          : t('admin.news.actions.archive', 'Archive'),
                        onClick: () => handleToggleArchive(article.id, article.isArchived),
                        variant: 'status',
                      },
                      {
                        label: t('admin.news.actions.delete', 'Delete'),
                        onClick: () => handleDelete(article.id),
                        variant: 'danger',
                        disabled: deletingArticle === article.id,
                      },
                    ]}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      
      {newsArticles.length === 0 && (
        <div className="admin-table__empty">
          {t('admin.news.noData', 'No news articles found')}
        </div>
      )}
      
      {totalCount > 0 && (
        <div className="pagination-container">
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
            onPageSizeChange={setPageSize}
          />
        </div>
      )}
    </div>
  );
};

export default NewsList;
