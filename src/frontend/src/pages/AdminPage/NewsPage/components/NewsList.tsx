import { useEffect, useRef, useState, type MouseEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useLocation, useNavigate } from 'react-router-dom';
import { createAdminFromState } from '../../../../utils/adminReturnTo';
import { newsCategoryLabel, newsSportLabel } from '../Utils/newsTaxonomyLabels';
import { hasActiveNewsFilters, type NewsFilters } from '../Utils/newsAdminFilters';
import {
  archiveNewsService,
  deleteNewsService,
  newsService,
  restoreNewsService,
  type NewsArticleDto,
} from '../../../../api/news/newsService';
import Button from '../../../../components/Button/Button';
import Pagination from '../../../../components/Pagination';
import ActionsDropdown from '../../../../components/ActionsDropdown/ActionsDropdown';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import ConfirmationDialog from '../../../../components/ConfirmationDialog/ConfirmationDialog';
import SportIcon from '../../../../components/SportIcon/SportIcon';
import TeamCategoryBadge from '../../../../components/TeamCategoryBadge/TeamCategoryBadge';
import '../../../../styles/AdminTable.scss';
import '../styles/NewsList.scss';

type NewsAction = 'delete' | 'archive' | 'restore';

interface PendingAction {
  kind: NewsAction;
  ids: string[];
}

interface NewsListProps {
  filters: NewsFilters;
  onClearFilters: () => void;
  onCreate: () => void;
  /** Reports the number of articles that match the filters. */
  onTotalChange: (total: number) => void;
}

const SKELETON_ROWS = 5;
const SUCCESS_TOAST_MS = 3500;

const runNewsAction = (kind: NewsAction, id: string): Promise<unknown> => {
  switch (kind) {
    case 'delete':
      return deleteNewsService(id);
    case 'archive':
      return archiveNewsService(id);
    default:
      return restoreNewsService(id);
  }
};

/** Admin table of news articles with selection, bulk actions and paging. */
export default function NewsList({ filters, onClearFilters, onCreate, onTotalChange }: NewsListProps) {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();

  const filtersKey: string = JSON.stringify(filters);
  // Paging belongs to one set of filters: new filters start from page 1 without an extra fetch.
  const [paging, setPaging] = useState<{ key: string; page: number }>({ key: filtersKey, page: 1 });
  const page: number = paging.key === filtersKey ? paging.page : 1;
  const [pageSize, setPageSize] = useState(10);
  const [reloadToken, setReloadToken] = useState(0);

  const [articles, setArticles] = useState<NewsArticleDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [loadFailed, setLoadFailed] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [pending, setPending] = useState<PendingAction | null>(null);
  const [actionBusy, setActionBusy] = useState(false);
  const toastTimerRef = useRef<number | null>(null);

  const setPage = (nextPage: number): void => setPaging({ key: filtersKey, page: nextPage });

  useEffect(() => {
    let cancelled = false;
    const current: NewsFilters = JSON.parse(filtersKey) as NewsFilters;
    setLoading(true);
    setLoadFailed(false);
    newsService({
      category: current.category,
      sportCategory: current.sportCategory,
      searchTerm: current.searchTerm,
      includeArchived: current.includeArchived,
      teamCategories: current.teamCategories,
      page,
      pageSize,
    })
      .then((response) => {
        if (cancelled) {
          return;
        }
        if (Array.isArray(response)) {
          setArticles(response);
          setTotalCount(response.length);
          setTotalPages(1);
        } else {
          setArticles(response.data);
          setTotalCount(response.pagination.totalCount);
          setTotalPages(response.pagination.totalPages);
        }
        setSelectedIds(new Set());
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          console.error('Failed to fetch news articles:', err);
          setArticles([]);
          setLoadFailed(true);
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [filtersKey, page, pageSize, reloadToken]);

  useEffect(() => {
    onTotalChange(totalCount);
  }, [onTotalChange, totalCount]);

  useEffect(() => {
    return () => {
      if (toastTimerRef.current !== null) {
        window.clearTimeout(toastTimerRef.current);
      }
    };
  }, []);

  const showSuccess = (message: string): void => {
    setSuccessMessage(message);
    if (toastTimerRef.current !== null) {
      window.clearTimeout(toastTimerRef.current);
    }
    toastTimerRef.current = window.setTimeout(() => setSuccessMessage(null), SUCCESS_TOAST_MS);
  };

  const toggleSelect = (id: string): void => {
    setSelectedIds((previous) => {
      const next = new Set(previous);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const selectAll = (): void => setSelectedIds(new Set(articles.map((article) => article.id)));
  const clearSelection = (): void => setSelectedIds(new Set());

  const editArticle = (id: string): void => {
    navigate(`/admin/news/edit/${id}`, { state: createAdminFromState(location.pathname, location.search) });
  };

  const viewArticle = (id: string): void => {
    window.open(`/uutiset/${id}`, '_blank', 'noopener,noreferrer');
  };

  const successText = (kind: NewsAction, count: number): string => {
    if (count > 1) {
      return t(`admin.news.success.bulk_${kind}`, { count });
    }
    switch (kind) {
      case 'delete':
        return t('admin.news.success.deleted');
      case 'archive':
        return t('admin.news.success.archived');
      default:
        return t('admin.news.success.unarchived');
    }
  };

  const confirmPending = async (): Promise<void> => {
    if (!pending) {
      return;
    }
    const { kind, ids } = pending;
    setActionBusy(true);
    setActionError(null);
    const results = await Promise.allSettled(ids.map((id) => runNewsAction(kind, id)));
    const failed: number = results.filter((result) => result.status === 'rejected').length;
    results.forEach((result) => {
      if (result.status === 'rejected') {
        console.error(`News ${kind} failed:`, result.reason);
      }
    });
    setActionBusy(false);
    setPending(null);

    if (failed > 0) {
      setActionError(
        kind === 'delete'
          ? t('admin.news.errors.deleteFailed')
          : t('admin.news.errors.updateArchiveFailed'),
      );
    } else {
      showSuccess(successText(kind, ids.length));
    }

    // Deleting the whole last page moves back one page; everything else reloads in place.
    if (kind === 'delete' && failed === 0 && ids.length >= articles.length && page > 1) {
      setPage(page - 1);
    } else {
      setReloadToken((value) => value + 1);
    }
  };

  const confirmTexts = (action: PendingAction): { title: string; message: string; confirm: string } => {
    const bulk: boolean = action.ids.length > 1;
    const count: number = action.ids.length;
    switch (action.kind) {
      case 'delete':
        return {
          title: t('admin.news.confirmDeleteTitle'),
          message: bulk ? t('admin.news.confirmBulkDelete', { count }) : t('admin.news.confirmDelete'),
          confirm: t('admin.news.actions.delete'),
        };
      case 'archive':
        return {
          title: t('admin.news.confirmArchiveTitle'),
          message: bulk ? t('admin.news.confirmBulkArchive', { count }) : t('admin.news.confirmArchive'),
          confirm: t('admin.news.actions.archive'),
        };
      default:
        return {
          title: t('admin.news.confirmRestoreTitle'),
          message: bulk ? t('admin.news.confirmBulkUnarchive', { count }) : t('admin.news.confirmUnarchive'),
          confirm: t('admin.news.actions.unarchive'),
        };
    }
  };

  const formatDate = (iso: string): string => {
    const date = new Date(iso);
    return Number.isNaN(date.getTime())
      ? '–'
      : date.toLocaleDateString(i18n.language, { day: 'numeric', month: 'numeric', year: 'numeric' });
  };

  const stopRowClick = (event: MouseEvent): void => event.stopPropagation();

  const allSelected: boolean = articles.length > 0 && articles.every((article) => selectedIds.has(article.id));
  const selectedArticles: NewsArticleDto[] = articles.filter((article) => selectedIds.has(article.id));
  const filtersActive: boolean = hasActiveNewsFilters(filters);
  const pendingTexts = pending ? confirmTexts(pending) : null;

  const renderRows = () => {
    if (loading) {
      return Array.from({ length: SKELETON_ROWS }, (_, index) => (
        <tr key={`skeleton-${index}`} className="news-table__skeleton-row" aria-hidden="true">
          <td className="admin-table__checkbox-col" />
          <td colSpan={5}>
            <span className="news-table__skeleton" />
          </td>
          <td />
        </tr>
      ));
    }

    return articles.map((article) => {
      const selected: boolean = selectedIds.has(article.id);
      return (
        <tr
          key={article.id}
          className={`admin-table__row--clickable${selected ? ' admin-table__row--selected' : ''}`}
          onClick={() => editArticle(article.id)}
        >
          <td className="admin-table__checkbox-col" onClick={stopRowClick}>
            <input
              type="checkbox"
              checked={selected}
              onChange={() => toggleSelect(article.id)}
              aria-label={t('admin.news.table.select', { title: article.title })}
            />
          </td>
          <td className="news-table__article">
            {article.mainImage ? (
              <img className="news-table__thumb" src={article.mainImage} alt="" loading="lazy" />
            ) : (
              <span className="news-table__thumb news-table__thumb--empty" aria-hidden="true" />
            )}
            <span className="news-table__text">
              <span className="news-table__title">{article.title}</span>
              {article.summary && <span className="news-table__summary">{article.summary}</span>}
            </span>
          </td>
          <td className="news-table__classification">
            <span className="news-table__tags">
              {article.sportCategory && (
                <span className="news-table__sport">
                  <SportIcon sport={article.sportCategory} size="sm" decorative />
                  {newsSportLabel(t, article.sportCategory)}
                </span>
              )}
              {article.category && (
                <span className="admin-tag admin-tag--blue">{newsCategoryLabel(t, article.category)}</span>
              )}
              <TeamCategoryBadge category={article.teamCategory} />
            </span>
          </td>
          <td className="news-table__author">{article.author || '–'}</td>
          <td className="news-table__date">{formatDate(article.createdAt)}</td>
          <td className="news-table__status">
            <span className={`news-status news-status--${article.isArchived ? 'archived' : 'published'}`}>
              <span className="news-status__dot" aria-hidden="true" />
              {article.isArchived ? t('admin.news.status.archivedLabel') : t('admin.news.status.publishedLabel')}
            </span>
          </td>
          <td className="admin-table__actions-col" onClick={stopRowClick}>
            <ActionsDropdown
              ariaLabel={t('admin.news.actions.menu')}
              actions={[
                { label: t('admin.news.actions.edit'), onClick: () => editArticle(article.id) },
                ...(article.isArchived
                  ? []
                  : [{ label: t('common.viewPublic'), onClick: () => viewArticle(article.id) }]),
                {
                  label: article.isArchived ? t('admin.news.actions.unarchive') : t('admin.news.actions.archive'),
                  onClick: () => setPending({ kind: article.isArchived ? 'restore' : 'archive', ids: [article.id] }),
                  variant: 'status',
                },
                {
                  label: t('admin.news.actions.delete'),
                  onClick: () => setPending({ kind: 'delete', ids: [article.id] }),
                  variant: 'danger',
                },
              ]}
            />
          </td>
        </tr>
      );
    });
  };

  return (
    <div className="news-list">
      {successMessage && (
        <div className="news-list__toast" role="status">
          {successMessage}
        </div>
      )}

      {actionError && (
        <div className="news-list__alert" role="alert">
          <span>{actionError}</span>
          <button
            type="button"
            className="news-list__alert-close"
            onClick={() => setActionError(null)}
            aria-label={t('common.close')}
          >
            ×
          </button>
        </div>
      )}

      <BulkActionsBar
        selectedCount={selectedIds.size}
        totalCount={articles.length}
        onSelectAll={selectAll}
        onClearSelection={clearSelection}
        actions={[
          {
            label: t('admin.news.actions.archive'),
            onClick: () =>
              setPending({
                kind: 'archive',
                ids: selectedArticles.filter((article) => !article.isArchived).map((article) => article.id),
              }),
            variant: 'status',
            disabled: selectedArticles.every((article) => article.isArchived),
          },
          {
            label: t('admin.news.actions.unarchive'),
            onClick: () =>
              setPending({
                kind: 'restore',
                ids: selectedArticles.filter((article) => article.isArchived).map((article) => article.id),
              }),
            variant: 'status',
            disabled: selectedArticles.every((article) => !article.isArchived),
          },
          {
            label: t('admin.news.actions.delete'),
            onClick: () => setPending({ kind: 'delete', ids: Array.from(selectedIds) }),
            variant: 'danger',
          },
        ]}
      />

      {loadFailed ? (
        <div className="news-list__state" role="alert">
          <p className="news-list__state-title">{t('admin.news.errors.fetchFailed')}</p>
          <Button variant="secondary" size="sm" onClick={() => setReloadToken((value) => value + 1)}>
            {t('common.retry')}
          </Button>
        </div>
      ) : !loading && articles.length === 0 ? (
        <div className="news-list__state">
          <p className="news-list__state-title">
            {filtersActive ? t('admin.news.noResults') : t('admin.news.noData')}
          </p>
          <p className="news-list__state-hint">
            {filtersActive ? t('admin.news.noResultsHint') : t('admin.news.noDataHint')}
          </p>
          {filtersActive ? (
            <Button variant="secondary" size="sm" onClick={onClearFilters}>
              {t('admin.news.filter.clear')}
            </Button>
          ) : (
            <Button size="sm" rounded="pill" onClick={onCreate}>
              {t('admin.news.createNew')}
            </Button>
          )}
        </div>
      ) : (
        <div className="admin-table__wrapper news-table__wrapper">
          <table className="admin-table news-table" aria-busy={loading}>
            <thead>
              <tr>
                <th className="admin-table__checkbox-col">
                  <input
                    type="checkbox"
                    checked={allSelected}
                    disabled={loading || articles.length === 0}
                    onChange={() => (allSelected ? clearSelection() : selectAll())}
                    aria-label={t('admin.news.table.selectAll')}
                  />
                </th>
                <th>{t('admin.news.table.article')}</th>
                <th>{t('admin.news.table.classification')}</th>
                <th>{t('admin.news.table.author')}</th>
                <th>{t('admin.news.table.createdAt')}</th>
                <th>{t('admin.news.table.status')}</th>
                <th className="admin-table__actions-col">
                  <span className="admin-table__visually-hidden">{t('admin.news.table.actions')}</span>
                </th>
              </tr>
            </thead>
            <tbody>{renderRows()}</tbody>
          </table>
        </div>
      )}

      {!loadFailed && totalCount > 0 && (
        <Pagination
          className="news-list__pagination"
          currentPage={page}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPage(1);
          }}
          pageSizeOptions={[10, 25, 50]}
        />
      )}

      <ConfirmationDialog
        isOpen={pending !== null}
        icon={pending?.kind === 'delete' ? '🗑️' : '📁'}
        title={pendingTexts?.title ?? ''}
        message={pendingTexts?.message ?? ''}
        confirmText={pendingTexts?.confirm ?? ''}
        cancelText={t('common.cancel')}
        isLoading={actionBusy}
        onConfirm={() => {
          void confirmPending();
        }}
        onCancel={() => setPending(null)}
      />
    </div>
  );
}
