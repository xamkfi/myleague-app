import { useTranslation } from 'react-i18next';
import './SearchField.scss';
import SearchIcon from '../../assets/basicIcons/search.svg';
import { useCallback } from 'react';

export interface SearchFieldProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  onClear?: () => void;
  className?: string;
  size?: 'sm' | 'md';
  rounded?: 'md' | 'pill';
  leadingIconSrc?: string;
  showClear?: boolean;
  fullWidth?: boolean;
  id?: string;
  name?: string;
  ariaLabel?: string;
  inputProps?: React.InputHTMLAttributes<HTMLInputElement>;
}

const SearchField = ({
  value,
  onChange,
  placeholder,
  onClear,
  className = '',
  size = 'md',
  rounded = 'pill',
  leadingIconSrc,
  showClear = true,
  fullWidth = true,
  id,
  name,
  ariaLabel,
  inputProps
}: SearchFieldProps) => {
  const { t } = useTranslation();

  const handleClear = useCallback(() => {
    if (onClear) {
      onClear();
    } else {
      onChange('');
    }
  }, [onClear, onChange]);

  return (
    <div
      className={[
        'search-field',
        size ? `search-field--${size}` : '',
        rounded === 'pill' ? `search-field--${rounded}` : 'search-field--rounded-md',
        fullWidth ? 'search-field--full' : '',
        className
      ].filter(Boolean).join(' ')}
    >
      <img
        src={leadingIconSrc || SearchIcon}
        className="search-field__icon"
        alt=""
        aria-hidden="true"
      />
      <input
        type="text"
        id={id}
        name={name}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder ?? t('common.search', 'Search...')}
        aria-label={ariaLabel ?? placeholder ?? t('common.search', 'Search')}
        className="search-field__input"
        {...inputProps}
      />
      {showClear && value && (
        <button
          type="button"
          className="search-field__clear"
          aria-label={t('common.clear', 'Clear search')}
          onClick={handleClear}
        >
          ×
        </button>
      )}
    </div>
  );
};

export default SearchField;


