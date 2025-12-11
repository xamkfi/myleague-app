import type { ChangeEvent } from 'react';
import './OfficialsSelectorSection.scss';

export interface OfficialOption {
  id: string;
  name: string;
}

interface OfficialsSelectorSectionProps {
  selectedOfficials: string[];
  options: OfficialOption[];
  saving: boolean;
  onAddRow: () => void;
  onSelect: (index: number, refereeId: string) => void;
  onRemove: (index: number, refereeId: string) => void;
}

const OfficialsSelectorSection = ({
  selectedOfficials,
  options,
  saving,
  onAddRow,
  onSelect,
  onRemove
}: OfficialsSelectorSectionProps) => {
  const handleChange = (index: number, event: ChangeEvent<HTMLSelectElement>) => {
    onSelect(index, event.target.value);
  };

  return (
    <div className="officials-selector-section">
      <div className="officials-title-row">
        <div className="officials-title">MATCH OFFICIALS</div>
        <button type="button" className="officials-add-btn" onClick={onAddRow} disabled={saving}>
          + Add referee
        </button>
      </div>
      <div className="officials-rows">
        {selectedOfficials.map((refId, idx) => (
          <div className="officials-row" key={`${idx}-${refId || 'empty'}`}>
            <select
              value={refId}
              onChange={(e) => handleChange(idx, e)}
              disabled={saving}
            >
              <option value="">{saving ? 'Saving...' : 'SELECT REFEREE'}</option>
              {options.map(option => (
                <option key={option.id} value={option.id} disabled={selectedOfficials.includes(option.id) && option.id !== refId}>
                  {option.name}
                </option>
              ))}
            </select>
            <button
              type="button"
              className="officials-remove-btn"
              onClick={() => onRemove(idx, refId)}
              disabled={saving || !refId}
              aria-label="Remove referee"
            >
              ×
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default OfficialsSelectorSection;

