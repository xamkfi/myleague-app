import { useId, useState } from 'react';
import { createPortal } from 'react-dom';
import './StatAbbr.scss';

interface StatAbbrProps {
  abbr: string;
  title: string;
}

function StatAbbr({ abbr, title }: StatAbbrProps) {
  const tooltipId = useId();
  const [anchor, setAnchor] = useState<{ x: number; y: number } | null>(null);

  const showTip = (element: HTMLElement): void => {
    const rect = element.getBoundingClientRect();
    setAnchor({ x: rect.left + rect.width / 2, y: rect.top });
  };

  return (
    <>
      <span
        className="stat-abbr"
        tabIndex={0}
        aria-describedby={anchor ? tooltipId : undefined}
        onMouseEnter={(event) => showTip(event.currentTarget)}
        onMouseLeave={() => setAnchor(null)}
        onFocus={(event) => showTip(event.currentTarget)}
        onBlur={() => setAnchor(null)}
      >
        {abbr}
      </span>
      {anchor && createPortal(
        <span
          id={tooltipId}
          className="stat-abbr-tip"
          role="tooltip"
          style={{ left: anchor.x, top: anchor.y }}
        >
          {title}
        </span>,
        document.body,
      )}
    </>
  );
}

export default StatAbbr;
