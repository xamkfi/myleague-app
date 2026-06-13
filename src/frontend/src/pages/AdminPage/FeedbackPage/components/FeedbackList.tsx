import { useCallback, useEffect, useState } from "react";
import type { FeedbackDto } from "../../../../types/feedback/feedbackTypes";
import { getFeedbackService } from "../../../../api/admin/Feedback/GetFeedbackService";
import { useTranslation } from "react-i18next";
import BulkActionsBar from "../../../../components/BulkActionsBar/BulkActionsBar";
import { DeleteFeedbackService } from "../../../../api/admin/Feedback/DeleteFeedbackService";
import ActionsDropdown from "../../../../components/ActionsDropdown/ActionsDropdown";
import '../../../../styles/AdminTable.scss'
import Pagination from "../../../../components/Pagination";

const FeedbackList = () => {
    const { t } = useTranslation();

    const [feedbacksList, setFeedbacksList] = useState<FeedbackDto[]>([])
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [totalCount, setTotalCount] = useState<number>(0);
    const [totalPages, setTotalPages] = useState<number>(1);

    const fetchFeedback = useCallback(async () => {
        try {
            setLoading(true);
            const response = await getFeedbackService.getAll({page: currentPage, pageSize: pageSize});
            setFeedbacksList(response.data);
            setTotalCount(response.pagination.totalCount);
            setTotalPages(response.pagination.totalPages);
            setError(null);
        } catch (error) {
            console.error('Failed to fetch feedback', error);
            setError(t('admin.feedback.errors.fetchFailed', 'Failed to fetch Feedback'));
            setFeedbacksList([]);
            setTotalCount(0);
            setTotalPages(1);
        } finally {
            setLoading(false);
        }
    }, [t, currentPage, pageSize]);

    useEffect(() => {
        fetchFeedback();
    }, [fetchFeedback]);

    const handleToggleSelect = (id:string) => {
        setSelectedIds((prev) => {
            const next = new Set(prev);
            if (next.has(id)){
                next.delete(id);
            } else {
                next.add(id);
            }
            return next;
        });
    };

    const handleSelectAll = () => {
        setSelectedIds(new Set(feedbacksList.map((a) => a.id)));
    };
    
    const handleClearSelection = () => {
        setSelectedIds(new Set());
    };

    const handleDelete = async (id: string) => {
        if(window.confirm(t('admin.feedback.confirmDelete', 'Are you sure you want to delete this feedback?'))) {
            try {
                await DeleteFeedbackService.Delete(id);

                const updatedFeedbackList = feedbacksList.filter(feedback => feedback.id !== id);
                setFeedbacksList(updatedFeedbackList);

                if (updatedFeedbackList.length === 0 && currentPage > 1){
                    setCurrentPage(currentPage - 1);
                } else {
                    fetchFeedback();
                }
                setError(null);
            } catch (error) {
                console.error("Failed to delete feedback", error);
                setError(t('admin.feedback.errors.deleteFailed', 'Failed to delete feedback'));
            }
        }
    };

    const handleBulkDelete = async () => {
        if(selectedIds.size === 0) return;

        const confirmMessage = t('admin.feedback.actions.confirmBulkDelete',
            'Are you sure you want to delete {{count}} selected feedback? This action cannot be undone.',
            {count: selectedIds.size}
        );

        if(window.confirm(confirmMessage)) {
            try {
                for (const feedbackId of selectedIds) {
                    await DeleteFeedbackService.Delete(feedbackId);
                }

                await fetchFeedback();
                
                const successMessage = t('admin.feedback.success.bulkDelete',
                    '{{count}} feedback successfully deleted.',
                   {count: selectedIds.size} 
                );
                setSelectedIds(new Set());
                console.log(successMessage);
            } catch (error) {
                console.error('Failed to delete selected feedback', error);
                setError(t('admin.feedback.errors.bulkDeleteFailed', 'Failed to delete selected feedback'));
            }
        }
    };

    const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
    };

    if (loading) {
        return <div className="admin-table__empty">{t('admin.feedback.loading', 'Loading feedback...')}</div>;
    }

    if (error) {
        return <div className="admin-table__empty" style={{ color: '#b91c1c' }}>{error}</div>;
    }

    const allSelected = feedbacksList.length > 0 && feedbacksList.every((a) => selectedIds.has(a.id));
    return(
        <>
        <div className="feedback-list">
            <BulkActionsBar
                selectedCount={selectedIds.size}
                totalCount={totalCount}
                onSelectAll={handleSelectAll}
                onClearSelection={handleClearSelection}
                actions={[
                    { label: t('admin.feedback.actions.delete', 'Delete'), onClick: handleBulkDelete, variant: 'danger'}
                ]}
            />
            
            <div className="admin-table__wrapper">
                <table className="admin-table">
                    <thead>
                        <tr>
                            <th>
                                <input
                                    type="checkbox"
                                    checked={allSelected}
                                    onChange={() => allSelected ? handleClearSelection() : handleSelectAll()}/>
                            </th>
                            <th>{t('admin.feedback.table.title', 'Title')}</th>
                            <th>{t('admin.feedback.table.email', 'Email')}</th>
                            <th>{t('admin.feedback.table.createdAt', 'CreatedAt')}</th>
                            <th>{t('admin.feedback.table.actions', 'Actions')}</th>
                        </tr>
                    </thead>
                    <tbody>
                        {feedbacksList.map(feedback => (
                            <tr
                                key={feedback.id}
                                className={selectedIds.has(feedback.id) ? 'admin-table__row--selected' : ''}
                            >
                                <td className="admin-table__checkbox-col">
                                    <input 
                                    type="checkbox"
                                    checked={selectedIds.has(feedback.id)}
                                    onChange={() => handleToggleSelect(feedback.id)}/>
                                </td>
                                <td>
                                    <div className="admin-table__name">
                                        {feedback.title}
                                    </div>
                                </td>
                                <td>{feedback.email ? feedback.email : ''}</td>
                                <td>{formatDate(feedback.createdAt)}</td>
                                <td className="admin-table__actions-col">
                                    <ActionsDropdown
                                    ariaLabel={t('admin.feedback.actions.menu', 'Feedback actions menu')}
                                    actions={[
                                        { label: t('admin.feedback.actions.delete', 'Delete'), onClick: () => {handleDelete(feedback.id)} }
                                    ]}/>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {feedbacksList.length === 0 && (
                <div className="admin-table__empty">
                    {t('admin.feedback.noData', 'No feedback available')}
                </div>
            )}

            {totalCount > 0 && (
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    totalCount={totalCount}
                    pageSize={pageSize}
                    onPageChange={setCurrentPage}
                    onPageSizeChange={setPageSize}
                />
            )}
        </div>
        </>
    );
};

export default FeedbackList;