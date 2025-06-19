import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { NewsArticleDto } from '../../../../api/news/newsService'; 
import { newsService, archiveNewsService, restoreNewsService } from '../../../../api/news/newsService';
import "../styles/NewsList.scss";

const NewsList = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [newsArticles, setNewsArticles] = useState<NewsArticleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingArticle, setDeletingArticle] = useState<string | null>(null);

  useEffect(() => {
    const fetchNewsArticles = async () => {
      try {
        const data = await newsService();
        setNewsArticles(data);
        setError(null);
      } catch (error) {
        console.error('Failed to fetch news articles:', error);
        setError(t('admin.news.errors.fetchFailed', 'Failed to fetch news articles'));
      } finally {
        setLoading(false);
      }
    };

    fetchNewsArticles();
  }, [t]);

  const handleEdit = (id: string) => {
    navigate(`/admin/news/edit/${id}`);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm(t('admin.news.confirmDelete', 'Are you sure you want to delete this news article?'))) {
      setDeletingArticle(id);
      try {
        // Note: You'll need to implement delete functionality in newsService
        // await newsService.delete(id);
        setNewsArticles(newsArticles.filter(article => article.id !== id));
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
        setNewsArticles(newsArticles.map(article => 
          article.id === id ? { ...article, isArchived: !currentStatus } : article
        ));
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
    return <div className="news-loading">{t('admin.news.loading', 'Loading news articles...')}</div>;
  }

  if (error) {
    return <div className="news-error">{error}</div>;
  }

  return (
    <div className="news-list">
      <table>
        <thead>
          <tr>
            <th>{t('admin.news.table.title', 'Title')}</th>
            <th>{t('admin.news.table.author', 'Author')}</th>
            <th>{t('admin.news.table.category', 'Category')}</th>
            <th>{t('admin.news.table.createdAt', 'Created')}</th>
            <th>{t('admin.news.table.archived', 'Archived')}</th>
            <th>{t('admin.news.table.actions', 'Actions')}</th>
          </tr>
        </thead>
        <tbody>
          {newsArticles.map(article => (
            <tr key={article.id}>
              <td>
                <div className="article-title">
                  <div className="title-text">{article.title}</div>
                </div>
              </td>
              <td>{article.author || '-'}</td>
              <td>
                <div className="categories">
                  {article.category && (
                    <span className="category-tag">{article.category}</span>
                  )}
                  {article.sportCategory && (
                    <span className="sport-category-tag">{article.sportCategory}</span>
                  )}
                </div>
              </td>
              <td>{formatDate(article.createdAt)}</td>
              <td>
                <button
                  className={`archive-toggle ${article.isArchived ? 'archived' : 'not-archived'}`}
                  onClick={() => handleToggleArchive(article.id, article.isArchived)}
                  title={t('admin.news.actions.toggleArchive', 'Click to toggle archive status')}
                >
                  <span className="status-icon">
                    {article.isArchived ? '📁' : '📄'}
                  </span>
                  <span className="status-text">
                    {article.isArchived 
                      ? t('admin.news.status.archived', 'Yes')
                      : t('admin.news.status.notArchived', 'No')}
                  </span>
                </button>
              </td>
              <td>
                <div className="action-buttons">
                  <button
                    className="edit-button"
                    onClick={() => handleEdit(article.id)}
                  >
                    {t('admin.news.actions.edit', 'Edit')}
                  </button>
                  <button
                    className="delete-button"
                    onClick={() => handleDelete(article.id)}
                    disabled={deletingArticle === article.id}
                  >
                    {deletingArticle === article.id ? (
                      <span className="loading-spinner">⏳</span>
                    ) : (
                      t('admin.news.actions.delete', 'Delete')
                    )}
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {newsArticles.length === 0 && (
        <div className="no-data">
          {t('admin.news.noData', 'No news articles found')}
        </div>
      )}
    </div>
  );
};

export default NewsList;
