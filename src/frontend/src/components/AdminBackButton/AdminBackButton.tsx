import { useTranslation } from 'react-i18next';
import { useAdminReturnTo } from '../../hooks/useAdminReturnTo';
import './AdminBackButton.scss';

function AdminBackButton() {
  const { t } = useTranslation();
  const { showBack, goBack } = useAdminReturnTo();

  if (!showBack) {
    return null;
  }

  return (
    <div className="admin-back-bar">
      <button type="button" className="admin-back-bar__button" onClick={goBack}>
        <span aria-hidden="true">&larr;</span>
        {t('common.back')}
      </button>
    </div>
  );
}

export default AdminBackButton;
