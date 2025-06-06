import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './AdminPage.scss';

const AdminPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('admin.title', 'Admin Dashboard')}>
      <div className="admin-container">
        <div className="admin-actions">
          <button
            className="admin-action-button"
            onClick={() => navigate('/admin/persons')}
          >
            {t('admin.actions.managePerson', 'Edit/Add Person')}
          </button>
          {/* More admin actions will be added here */}
        </div>
      </div>
    </PageTemplate>
  );
};

export default AdminPage; 