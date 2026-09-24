export type LicenceFilterValue = 'all' | 'active' | 'inactive';

export function licenceFilterToHasActive(value: LicenceFilterValue): boolean | undefined {
  if (value === 'active') {
    return true;
  }
  if (value === 'inactive') {
    return false;
  }
  return undefined;
}
