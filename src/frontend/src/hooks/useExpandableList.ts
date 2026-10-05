import { useState } from 'react';

export interface ExpandableList<T> {
  visible: T[];
  isExpanded: boolean;
  canExpand: boolean;
  toggle: () => void;
}

/** Shows the first `limit` items until the user expands the list. */
export function useExpandableList<T>(items: readonly T[], limit: number): ExpandableList<T> {
  const [isExpanded, setIsExpanded] = useState<boolean>(false);
  const canExpand: boolean = items.length > limit;
  return {
    visible: isExpanded || !canExpand ? [...items] : items.slice(0, limit),
    isExpanded,
    canExpand,
    toggle: () => setIsExpanded((current) => !current),
  };
}
