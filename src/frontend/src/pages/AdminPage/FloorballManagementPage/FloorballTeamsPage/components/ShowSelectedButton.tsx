import { useTranslation } from 'react-i18next';
import './ShowSelectedButton.scss';

interface ShowSelectedButtonProps {
  showOnlySelected: boolean;
  onToggle: () => void;
  selectionCount: number;
}

const ShowSelectedButton = ({ showOnlySelected, onToggle, selectionCount }: ShowSelectedButtonProps) => {
  const { t } = useTranslation();

  return (
    <button
      className={`show-selected-btn ${showOnlySelected ? 'active' : ''}`}
      onClick={onToggle}
      disabled={selectionCount === 0 && !showOnlySelected}
    >
      {showOnlySelected
        ? t('common.showAll', 'Show All')
        : `${t('common.showSelected', 'Show Selected')} (${selectionCount})`
      }
    </button>
  );
};

export default ShowSelectedButton; 