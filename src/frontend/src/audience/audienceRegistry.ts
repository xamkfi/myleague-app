import { TeamCategory } from '../types/floorball/floorballTypes';

export type AudienceThemeId = 'adult' | 'youth' | 'women';

export interface AudienceDefinition {
  id: AudienceThemeId;
  themeId: AudienceThemeId;
  /** Maps to TeamCategory string values: 'Adult' | 'Youth' | 'Women' */
  teamCategory: TeamCategory;
  i18nKey: string;
  isDefault: boolean;
}

export const AUDIENCE_REGISTRY: readonly AudienceDefinition[] = [
  {
    id: 'adult',
    themeId: 'adult',
    teamCategory: TeamCategory.Adult,
    i18nKey: 'audience.adult',
    isDefault: true,
  },
  {
    id: 'youth',
    themeId: 'youth',
    teamCategory: TeamCategory.Youth,
    i18nKey: 'audience.youth',
    isDefault: false,
  },
  {
    id: 'women',
    themeId: 'women',
    teamCategory: TeamCategory.Women,
    i18nKey: 'audience.women',
    isDefault: false,
  },
] as const;

export const DEFAULT_AUDIENCE: AudienceDefinition =
  AUDIENCE_REGISTRY.find((entry) => entry.isDefault) ?? AUDIENCE_REGISTRY[0];

export function getAudienceById(id: string | null | undefined): AudienceDefinition | undefined {
  return AUDIENCE_REGISTRY.find((entry) => entry.id === id);
}

export function getAudienceByThemeId(themeId: string | null | undefined): AudienceDefinition | undefined {
  return AUDIENCE_REGISTRY.find((entry) => entry.themeId === themeId);
}

export function getAudienceByTeamCategory(
  category: TeamCategory | string | null | undefined,
): AudienceDefinition | undefined {
  return AUDIENCE_REGISTRY.find((entry) => entry.teamCategory === category);
}
