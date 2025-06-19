import type { Club, FloorballTeam, FloorballDivision } from '../types/floorball/floorballTypes';

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
 * Division priority order (higher number = higher priority)
 * Premier division gets highest priority, then Division1, etc.
 */
const DIVISION_PRIORITY: Record<FloorballDivision, number> = {
  Premier: 9,
  Division1: 8,
  Division2: 7,
  Division3: 6,
  Division4: 5,
  None: 4,
  Junior: 3,
  Youth: 2,
  Veterans: 1
};

/**
 * Division suffix mapping for URLs
 */
const DIVISION_SLUGS: Record<FloorballDivision, string> = {
  Premier: '', // No suffix for premier (highest priority)
  Division1: 'div1',
  Division2: 'div2', 
  Division3: 'div3',
  Division4: 'div4',
  None: 'none',
  Junior: 'junior',
  Youth: 'youth',
  Veterans: 'veterans'
};

/**
 * Create a simple slug from club name
 */
export const createClubSlug = (club: Club): string => {
  return slugify(club.name);
};

/**
 * Create team slugs with smart duplicate handling
 * Only adds division suffix if there are name conflicts
 */
export const createTeamSlugs = (teams: FloorballTeam[]): Map<string, string> => {
  const slugMap = new Map<string, string>();
  
  // Group teams by base name (before slugifying)
  const teamsByBaseName = new Map<string, FloorballTeam[]>();
  
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
      // Handle duplicates - sort by division priority (highest first)
      const sortedTeams = teamGroup.sort((a, b) => 
        DIVISION_PRIORITY[b.division] - DIVISION_PRIORITY[a.division]
      );
      
      // Highest priority team gets the base slug (no suffix)
      const highestPriorityTeam = sortedTeams[0];
      slugMap.set(highestPriorityTeam.id, baseSlug);
      
      // Other teams get division suffixes
      sortedTeams.slice(1).forEach(team => {
        const divisionSuffix = DIVISION_SLUGS[team.division];
        const slugWithSuffix = divisionSuffix ? `${baseSlug}-${divisionSuffix}` : `${baseSlug}-${team.id.slice(0, 8)}`;
        slugMap.set(team.id, slugWithSuffix);
      });
    }
  });
  
  return slugMap;
};

/**
 * Create a single team slug (used when you have individual team)
 */
export const createTeamSlug = (team: FloorballTeam, allTeams?: FloorballTeam[]): string => {
  if (!allTeams) {
    // Fallback - just use team name
    return slugify(team.name);
  }
  
  const slugMap = createTeamSlugs(allTeams);
  return slugMap.get(team.id) || slugify(team.name);
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
export const findTeamBySlug = (teams: FloorballTeam[], slug: string): FloorballTeam | undefined => {
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
export const getTeamSlug = (team: FloorballTeam, allTeams: FloorballTeam[]): string => {
  const slugMap = createTeamSlugs(allTeams);
  return slugMap.get(team.id) || slugify(team.name);
}; 