import React from 'react';
import { useTranslation } from 'react-i18next';
import './BulkStatusUpdateModal.scss';

interface BulkStatusUpdateModalProps {
  isOpen: boolean;
  action: 'activate' | 'deactivate';
  selectedCount: number;
  activeCount: number;
  inactiveCount: number;
  onConfirm: () => void;
  onCancel: () => void;
  isUpdating: boolean;
}

const BulkStatusUpdateModal: React.FC<BulkStatusUpdateModalProps> = ({
  isOpen,
  action,
  selectedCount,
  activeCount,
  inactiveCount,
  onConfirm,
  onCancel,
  isUpdating,
}) => {
  const { t } = useTranslation();

  if (!isOpen) return null;

  const actionText = action === 'activate' ? 'activate' : 'deactivate';
  const targetCount = action === 'activate' ? inactiveCount : activeCount;

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="confirm-modal-content" onClick={e => e.stopPropagation()}>
        <div className="confirm-modal-header">
          <h2>
            {t(`floorball.players.bulkStatusUpdate.${actionText}Title`, 
              `${action === 'activate' ? 'Activate' : 'Deactivate'} Players`)}
          </h2>
          <button
            className="modal-close"
            onClick={onCancel}
            type="button"
            aria-label="Close modal"
            disabled={isUpdating}
          >
            ×
          </button>
        </div>
        
        <div className="confirm-modal-body">
          <div className="status-icon">
            {action === 'activate' ? '✅' : '⚠️'}
          </div>
          <div className="confirm-message">
            <p>
              {t(`floorball.players.bulkStatusUpdate.${actionText}Message`, 
                `Are you sure you want to ${actionText} {{count}} selected ${targetCount === 1 ? 'player' : 'players'}?`, 
                { count: targetCount })}
            </p>
            
            {selectedCount > targetCount && (
              <div className="status-breakdown">
                <p className="info-text">
                  {t('floorball.players.bulkStatusUpdate.breakdown', 
                    'Of {{total}} selected players: {{active}} active, {{inactive}} inactive', 
                    { 
                      total: selectedCount, 
                      active: activeCount, 
                      inactive: inactiveCount 
                    })}
                </p>
                <p className="action-text">
                  {t(`floorball.players.bulkStatusUpdate.${actionText}Only`, 
                    `Only {{count}} ${targetCount === 1 ? 'player' : 'players'} will be ${action === 'activate' ? 'activated' : 'deactivated'}.`, 
                    { count: targetCount })}
                </p>
              </div>
            )}
          </div>
        </div>
        
        <div className="confirm-modal-footer">
          <button
            type="button"
            className="cancel-button"
            onClick={onCancel}
            disabled={isUpdating}
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            className={`status-update-button ${action}`}
            onClick={onConfirm}
            disabled={isUpdating || targetCount === 0}
          >
            {isUpdating 
              ? t(`floorball.players.bulkStatusUpdate.${actionText}ing`, `${action === 'activate' ? 'Activating' : 'Deactivating'}...`)
              : t(`floorball.players.bulkStatusUpdate.${actionText}Confirm`, `${action === 'activate' ? 'Activate' : 'Deactivate'} (${targetCount})`)}
          </button>
        </div>
      </div>
    </div>
  );
};

export default BulkStatusUpdateModal;