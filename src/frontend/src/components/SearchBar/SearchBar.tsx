import './SearchBar.scss';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useRef } from 'react';
import { createTeamSlug, createClubSlug } from '../../utils/slugUtils';
import { slugify } from '../../utils/slugUtils';
import { floorballTeamService } from '../../api/floorball/floorballTeamService';
import type { FloorballTeam } from '../../types/floorball/floorballTypes';
import { globalSearchService } from '../../api/common/globalSearchService';
import type { GlobalSearchResult } from '../../api/common/globalSearchService';
import { getClubs } from '../../api/common/clubService';
import type { Club } from '../../api/common/clubService';

function SearchBar(props: any) {
   const { t } = useTranslation();
   const [searchQuery, setSearchQuery] = useState('');
   const [searchResults, setSearchResults] = useState<any[]>([]);
   const [peopleResults, setPeopleResults] = useState<any[]>([]);
   const [clubResults, setClubResults] = useState<string[]>([]);
   const [allClubs, setAllClubs] = useState<Club[]>([]);
   const [isSearchFocused, setIsSearchFocused] = useState(false);
   const searchContainerRef = useRef<HTMLDivElement>(null);
   const navigate = useNavigate();

   // Add new states and ref
   const [isVisible, setIsVisible] = useState(false);
   const [isAnimatedIn, setIsAnimatedIn] = useState(false);
   const [pendingAction, setPendingAction] = useState<(() => void) | null>(null);
   const [hasSearched, setHasSearched] = useState(false);
   const resultsRef = useRef<HTMLDivElement>(null);

   // Load all clubs for slug resolution
   useEffect(() => {
     const loadClubs = async () => {
       try {
         const clubs = await getClubs();
         setAllClubs(clubs);
       } catch (err) {
         console.error('Failed to load clubs:', err);
       }
     };
     loadClubs();
   }, []);

   // Handle clicks outside the search container
   useEffect(() => {
     const handleClickOutside = (event: MouseEvent) => {
       if (searchContainerRef.current && !searchContainerRef.current.contains(event.target as Node)) {
         setIsSearchFocused(false);
       }
     };

     document.addEventListener('mousedown', handleClickOutside);
     return () => {
       document.removeEventListener('mousedown', handleClickOutside);
     };
   }, []);

   // Add new useEffect after the existing useEffects
   useEffect(() => {
     const hasResults = searchResults.length > 0 || peopleResults.length > 0 || clubResults.length > 0;
     const shouldShow = isSearchFocused && hasSearched;
     if (shouldShow && !isVisible) {
       setIsVisible(true);
       setTimeout(() => {
         setIsAnimatedIn(true);
       }, 10);
     } else if (!shouldShow && isVisible) {
       setIsAnimatedIn(false);
     }
   }, [isSearchFocused, searchResults, peopleResults, clubResults, isVisible, hasSearched]);

   const onSeacrhClick = async () => {
      if (!searchQuery.trim()) return;
      setHasSearched(true);
      try {
         const response = await globalSearchService.search(searchQuery.trim());
         if (response.data) {
            const teamResults = response.data.team.slice(0, 5);
            setSearchResults(teamResults);
            setPeopleResults(response.data.person.slice(0,5));
            setClubResults(response.data.clubNames.slice(0,5));
            console.log('Search results:', {
              people: response.data.person.length,
              teams: response.data.team.length,
              clubs: response.data.clubNames.length
            });
         }
      } catch (err) {
         console.error(err);
      }
   };

   const handlePersonClick = (person: any) => {
     const action = () => navigate(`/person/${person.personId}`);
     setPendingAction(() => action);
     setIsSearchFocused(false);
   };

   const handleTeamClick = (team: any) => {
     // Create a simple slug from teamName for search results
     const teamSlug = slugify(team.teamName);
     const action = () => navigate(`/team/${teamSlug}`);
     setPendingAction(() => action);
     setIsSearchFocused(false);
   };

   const handleClubClick = (clubName: string) => {
     const club = allClubs.find(c => c.name === clubName);
     if (club) {
       const action = () => navigate(`/club/${createClubSlug(club)}`);
       setPendingAction(() => action);
       setIsSearchFocused(false);
     }
   };

   const onKeyPress = async (e: React.KeyboardEvent<HTMLElement>, functionality: string, teamId?: string) => {
      if (e.key === 'Enter') {
         switch (functionality) {
            case 'search':
               onSeacrhClick();
               break;
            default:
               break;
         }
      }
   };

   // Add handleTransitionEnd before return
   const handleTransitionEnd = (e: React.TransitionEvent<HTMLDivElement>) => {
     if (!isAnimatedIn && e.propertyName === 'opacity' && e.target === resultsRef.current) {
       setIsVisible(false);
       setSearchResults([]);
       setPeopleResults([]);
       setClubResults([]);
       setHasSearched(false);
       if (pendingAction) {
         pendingAction();
         setPendingAction(null);
       }
     }
   };

   return (
      <div className="search-bar-container" ref={searchContainerRef}>

         {/* Search input */}
         <input
            type="text"
            placeholder={t('searchBar.placeholder')}
            className="search-bar-input"
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyDown={(e) => onKeyPress(e, 'search')}
            onFocus={() => setIsSearchFocused(true)}
         />

         <button 
            onClick={onSeacrhClick} 
            className="search-bar-button">
               {t('searchBar.button')}
         </button>

         {/* Search results */}
         {isVisible && (
            <div 
              ref={resultsRef}
              className={`search-results ${isAnimatedIn ? 'is-open' : ''}`}
              onTransitionEnd={handleTransitionEnd}
            >
               {/* Show no results message if no results */}
               {searchResults.length === 0 && peopleResults.length === 0 && clubResults.length === 0 && (
                 <div className="search-no-results">
                   <div className="search-result-item-content">
                     <div className="search-result-item-name">No results...</div>
                   </div>
                 </div>
               )}
               
               {/* People results first */}
               {peopleResults.length > 0 && (
                 <div className="search-section-header">
                   <h4>People</h4>
                 </div>
               )}
               {peopleResults.map((p)=>(
                  <div 
                    className="search-result-item" 
                    key={p.personId}
                    tabIndex={0}
                    onClick={() => handlePersonClick(p)}
                    onKeyDown={(e) => e.key === 'Enter' && handlePersonClick(p)}>
                     <div className="search-result-item-content">
                        <div className="search-result-item-name">{p.firstName} {p.lastName}</div>
                     </div>
                   </div>
                ))}
                {/* Team results second */}
                {searchResults.length > 0 && (
                  <div className="search-section-header">
                    <h4>Teams</h4>
                  </div>
                )}
                {searchResults.map((result) => (
                   <div 
                   tabIndex={0} 
                   className="search-result-item" 
                   key={result.teamId || result.id} 
                   onClick={() => handleTeamClick(result)}
                  onKeyDown={(e) => e.key === 'Enter' && handleTeamClick(result)}>
                     <div className="search-result-item-content">
                        <div className="search-result-item-name">{result.teamName}</div>
                     </div>
                   </div>
                ))}
                {/* Club results third */}
                {clubResults.length > 0 && (
                  <div className="search-section-header">
                    <h4>Clubs</h4>
                  </div>
                )}
                {clubResults.map((clubName) => (
                  <div 
                    className="search-result-item" 
                    key={clubName}
                    tabIndex={0}
                    onClick={() => handleClubClick(clubName)}
                    onKeyDown={(e) => e.key === 'Enter' && handleClubClick(clubName)}>
                     <div className="search-result-item-content">
                        <div className="search-result-item-name">{clubName}</div>
                     </div>
                   </div>
                ))}
            </div>
         )}

      </div>
   );
}

export default SearchBar;
