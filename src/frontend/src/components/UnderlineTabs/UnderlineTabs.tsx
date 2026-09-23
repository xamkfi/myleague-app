import './UnderlineTabs.scss';

export interface UnderlineTab {
  id: string;
  label: string;
}

interface UnderlineTabsProps {
  tabs: UnderlineTab[];
  activeId: string;
  onChange: (id: string) => void;
  ariaLabel: string;
}

export default function UnderlineTabs({ tabs, activeId, onChange, ariaLabel }: UnderlineTabsProps) {
  return (
    <div className="underline-tabs" role="tablist" aria-label={ariaLabel}>
      {tabs.map((tab) => {
        const isActive = activeId === tab.id;
        return (
          <button
            key={tab.id}
            type="button"
            className={`underline-tabs__tab${isActive ? ' underline-tabs__tab--active' : ''}`}
            onClick={() => onChange(tab.id)}
            role="tab"
            aria-selected={isActive}
            aria-controls={`tabpanel-${tab.id}`}
            id={`tab-${tab.id}`}
          >
            {tab.label}
          </button>
        );
      })}
    </div>
  );
}
