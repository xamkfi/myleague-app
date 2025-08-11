import type { Club, FloorballTeam, FloorballTeamNameResult } from '../types/floorball/floorballTypes';

/**
 * Convert text to URL-friendly slug
 */
export const slugify = (text: string): string => {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, '') // Remove special characters
    .replace(/\s+/g, '-')         // Replace spaces with hyphens
    .replace(/-+/g, '-')          // Replace multiple hyphens with single
    .replace(/^-+|-+$/g, '')      // Remove leading/trailing hyphens
    .trim();
};

/**
 * Create team slugs with smart duplicate handling
 * Only adds ID suffix if there are name conflicts
 */
export const createTeamSlugs = (teams: FloorballTeamNameResult[]): Map<string, string> => {
  const slugMap = new Map<string, string>();
  
  // Group teams by base name (before slugifying)
  const teamsByBaseName = new Map<string, FloorballTeamNameResult[]>();
  
  teams.forEach(team => {
    const baseName = team.name.toLowerCase();
    if (!teamsByBaseName.has(baseName)) {
      teamsByBaseName.set(baseName, []);
    }
    teamsByBaseName.get(baseName)!.push(team);
  });
  
  // Process each group
  teamsByBaseName.forEach((teamGroup, baseName) => {
    const baseSlug = slugify(baseName);
    
    if (teamGroup.length === 1) {
      // No duplicates - use simple slug
      const team = teamGroup[0];
      slugMap.set(team.id, baseSlug);
    } else {
      // Handle duplicates - add ID suffix for uniqueness
      teamGroup.forEach((team, index) => {
        if (index === 0) {
          // First team gets the base slug
          slugMap.set(team.id, baseSlug);
        } else {
          // Other teams get ID suffix for uniqueness
          const slugWithSuffix = `${baseSlug}-${team.id.slice(0, 8)}`;
          slugMap.set(team.id, slugWithSuffix);
        }
      });
    }
  });
  
  return slugMap;
};

/**
 * Create a single team slug (used when you have individual team)
 */
export const createTeamSlug = (team: FloorballTeamNameResult, allTeams?: FloorballTeamNameResult[]): string => {
  if (!allTeams) {
    // Fallback - just use team name
    return slugify(team.name);
  }
  
  const slugMap = createTeamSlugs(allTeams);
  return slugMap.get(team.id) || slugify(team.name);
};

/**
 * Create a simple slug from club name
 */
export const createClubSlug = (club: Club): string => {
  return slugify(club.name);
};

/**
 * Find club by slug
 */
export const findClubBySlug = (clubs: Club[], slug: string): Club | undefined => {
  return clubs.find(club => createClubSlug(club) === slug);
};

/**
 * Find team by slug
 */
export const findTeamBySlug = (teams: FloorballTeamNameResult[], slug: string): FloorballTeamNameResult | undefined => {
  const slugMap = createTeamSlugs(teams);
  
  // Find team ID that matches the slug
  for (const [teamId, teamSlug] of slugMap.entries()) {
    if (teamSlug === slug) {
      return teams.find(team => team.id === teamId);
    }
  }
  
  return undefined;
};

/**
 * Get slug for a specific team (useful for navigation)
 */
export const getTeamSlug = (team: FloorballTeamNameResult, allTeams: FloorballTeamNameResult[]): string => {
  const slugMap = createTeamSlugs(allTeams);
  return slugMap.get(team.id) || slugify(team.name);
}; 