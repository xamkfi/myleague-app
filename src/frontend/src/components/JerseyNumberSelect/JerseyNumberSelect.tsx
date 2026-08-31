import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { JERSEY_NUMBER_OPTIONS } from './jerseyNumbers';
import './JerseyNumberSelect.scss';

export interface JerseyNumberSelectProps {
  value: number | null | undefined;
  takenNumbers: Iterable<number>;
  onChange: (value: number | null) => void;
  disabled?: boolean;
  id?: string;
  name?: string;
  className?: string;
  title?: string;
  prefixHash?: boolean;
}

export default function JerseyNumberSelect({
  value,
  takenNumbers,
  onChange,
  disabled = false,
  id,
  name,
  className,
  title,
  prefixHash = false,
}: JerseyNumberSelectProps) {
  const { t } = useTranslation();
  const current = value ?? null;
  const taken = useMemo(() => new Set(takenNumbers), [takenNumbers]);

  const formatNumber = (num: number): string => (prefixHash ? `#${num}` : String(num));

  return (
    <select
      id={id}
      name={name}
      className={['jersey-number-select', className].filter(Boolean).join(' ')}
      value={current ?? ''}
      disabled={disabled}
      title={title}
      onChange={(event) => {
        onChange(event.target.value === '' ? null : Number(event.target.value));
      }}
    >
      <option value="">{t('common.noJerseyNumber', '—')}</option>
      {JERSEY_NUMBER_OPTIONS.map((num) => {
        const isTaken = taken.has(num) && num !== current;
        const label = formatNumber(num);
        return (
          <option
            key={num}
            value={num}
            disabled={isTaken}
            className={isTaken ? 'jersey-number-select__option--taken' : undefined}
          >
            {isTaken
              ? t('common.jerseyNumberInUse', '{{number}} · in use', { number: label })
              : label}
          </option>
        );
      })}
    </select>
  );
}
