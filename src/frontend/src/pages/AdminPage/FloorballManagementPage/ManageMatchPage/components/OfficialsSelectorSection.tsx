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
  onChange: (ids: string[]) => void;
}

const OfficialsSelectorSection = ({
  selectedOfficials,
  options,
  saving,
  onChange
}: OfficialsSelectorSectionProps) => {
  const handleChange = (event: ChangeEvent<HTMLSelectElement>) => {
    const value = event.target.value;
    if (!value) {
      onChange([]);
      return;
    }
    onChange([value]);
  };

  const selectedValue = selectedOfficials[0] ?? '';

  return (
    <div className="officials-selector-section">
      <div className="officials-title">MATCH OFFICIALS</div>
      <div className="officials-dropdown">
        <div className="officials-header">REFEREE</div>
        <select
          id="officials-select"
          value={selectedValue}
          onChange={handleChange}
          disabled={saving}
        >
          <option value="">{saving ? 'Saving...' : 'SELECT REFEREE'}</option>
          {options.map(option => (
            <option key={option.id} value={option.id}>
              {option.name}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
};

export default OfficialsSelectorSection;

