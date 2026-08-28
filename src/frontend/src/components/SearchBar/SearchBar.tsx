import './SearchBar.scss';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useState, useEffect, useCallback } from 'react';
import { useRef } from 'react';
import { createClubSlug } from '../../utils/slugUtils';
import { getPlayerPath, getTeamPath, type SportKind } from '../../utils/sportRoutes';

function toSportKind(sport?: string | null): SportKind {
  if (sport === 'football' || sport === 'hockey') {
    return sport;
  }
  return 'floorball';
}
import { slugify } from '../../utils/slugUtils';
import { globalSearchService } from '../../api/common/globalSearchService';
import { getClubs } from '../../api/common/clubService';
import type { Club } from '../../api/common/clubService';
import SearchIcon from '../../assets/basicIcons/search.svg';

// Add interfaces
interface SearchPerson {
  personId: string;
  firstName: string;
  lastName: string;
  teamName?: string | null;
  sport?: string | null;
}

interface SearchTeam {
  teamId?: string;
  id?: string;
  teamName: string;
  sport?: string | null;
}

function SearchBar() {
   const { t } = useTranslation();
   const [searchQuery, setSearchQuery] = useState('');
   const [searchResults, setSearchResults] = useState<SearchTeam[]>([]);
   const [peopleResults, setPeopleResults] = useState<SearchPerson[]>([]);
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
   const [selectedIndex, setSelectedIndex] = useState(-1);
   const resultsRef = useRef<HTMLDivElement>(null);

   // Move handlers here
   const handlePersonClick = useCallback((person: SearchPerson) => {
     const action = () => navigate(getPlayerPath(toSportKind(person.sport), person.personId));
     setPendingAction(() => action);
     setIsSearchFocused(false);
   }, [navigate]);

   const handleTeamClick = useCallback((team: SearchTeam) => {
     const teamSlug = slugify(team.teamName);
     const action = () => navigate(getTeamPath(toSportKind(team.sport), teamSlug));
     setPendingAction(() => action);
     setIsSearchFocused(false);
   }, [navigate]);

   const handleClubClick = useCallback((clubName: string) => {
     const club = allClubs.find(c => c.name === clubName);
     if (club) {
       const action = () => navigate(`/club/${createClubSlug(club)}`);
       setPendingAction(() => action);
       setIsSearchFocused(false);
     }
   }, [allClubs, navigate]);

   const handleSelectedItem = useCallback(() => {
     const totalPeople = peopleResults.length;
     const totalTeams = searchResults.length;
     
     if (selectedIndex < totalPeople) {
       // Person result
       handlePersonClick(peopleResults[selectedIndex]);
     } else if (selectedIndex < totalPeople + totalTeams) {
       // Team result
       const teamIndex = selectedIndex - totalPeople;
       handleTeamClick(searchResults[teamIndex]);
     } else {
       // Club result
       const clubIndex = selectedIndex - totalPeople - totalTeams;
       handleClubClick(clubResults[clubIndex]);
     }
   }, [peopleResults, searchResults, clubResults, selectedIndex, handlePersonClick, handleTeamClick, handleClubClick]);

   const getItemClassName = (index: number) => {
     const baseClass = 'search-result-item';
     return selectedIndex === index ? `${baseClass} selected` : baseClass;
   };

   const handleTransitionEnd = (e: React.TransitionEvent<HTMLDivElement>) => {
     if (!isAnimatedIn && e.propertyName === 'opacity' && e.target === resultsRef.current) {
       setIsVisible(false);
       setSearchResults([]);
       setPeopleResults([]);
       setClubResults([]);
       setHasSearched(false);
       setSelectedIndex(-1);
       if (pendingAction) {
         pendingAction();
         setPendingAction(null);
       }
     }
   };

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

   // Debounced search effect
   useEffect(() => {
    const handler = setTimeout(() => {
      const doSearch = async () => {
        if (!searchQuery.trim()) return;
        setHasSearched(true);
        try {
           const response = await globalSearchService.search(searchQuery.trim());
           if (response.data) {
              const teamResults = response.data.team.slice(0, 5);
              setSearchResults(teamResults);
              setPeopleResults(response.data.person.slice(0,5));
              setClubResults(response.data.clubNames.slice(0,5));
           }
        } catch (err) {
           console.error(err);
        }
      };

      if (searchQuery.length >= 1) {
        doSearch();
      } else {
        // Clear results if query is too short
        setHasSearched(false);
        setSearchResults([]);
        setPeopleResults([]);
        setClubResults([]);
      }
    }, 300); // 300ms delay

    return () => {
      clearTimeout(handler);
    };
  }, [searchQuery]);

   // Handle clicks outside the search container
   useEffect(() => {
     const handleClickOutside = (event: MouseEvent) => {
       if (searchContainerRef.current && !searchContainerRef.current.contains(event.target as Node)) {
         setIsSearchFocused(false);
       }
     };

     const handleKeyDown = (event: KeyboardEvent) => {
       if (!isVisible || !hasSearched) return;

       const totalResults = peopleResults.length + searchResults.length + clubResults.length;
       
       switch (event.key) {
         case 'ArrowDown':
           event.preventDefault();
           setSelectedIndex(prev => 
             prev < totalResults - 1 ? prev + 1 : 0
           );
           break;
         case 'ArrowUp':
           event.preventDefault();
           setSelectedIndex(prev => 
             prev > 0 ? prev - 1 : totalResults - 1
           );
           break;
         case 'Enter':
           if (selectedIndex >= 0) {
             event.preventDefault();
             handleSelectedItem();
           }
           break;
         case 'Escape':
          setHasSearched(false);
          break;
       }
     };

     document.addEventListener('mousedown', handleClickOutside);
     document.addEventListener('keydown', handleKeyDown);
     return () => {
       document.removeEventListener('mousedown', handleClickOutside);
       document.removeEventListener('keydown', handleKeyDown);
     };
   }, [isVisible, hasSearched, peopleResults.length, searchResults.length, clubResults.length, selectedIndex, handleSelectedItem]);

   // Add new useEffect after the existing useEffects
   useEffect(() => {
     const shouldShow = isSearchFocused && hasSearched;
     if (shouldShow && !isVisible) {
       setIsVisible(true);
       setSelectedIndex(-1); // Reset selection when showing results
       setTimeout(() => {
         setIsAnimatedIn(true);
       }, 10);
     } else if (!shouldShow && isVisible) {
       setIsAnimatedIn(false);
     }
   }, [isSearchFocused, searchResults, peopleResults, clubResults, isVisible, hasSearched]);

   // Reset hasSearched when isSearchFocused becomes false
   useEffect(() => {
     if (!isSearchFocused) {
       setHasSearched(false);
     }
   }, [isSearchFocused]);

   return (
      <div className="search-bar-container" ref={searchContainerRef}>

         {/* Search input */}
         <div className="search-bar-input-wrapper">
           <img src={SearchIcon} className="search-icon" alt="Search" />
           <input
              type="text"
              placeholder={t('searchBar.placeholder')}
              className="search-bar-input"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onFocus={() => setIsSearchFocused(true)}
           />
         </div>

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
                     <div className="search-result-item-name">{t('searchBar.noResults')}</div>
                   </div>
                 </div>
               )}
               
               {/* People results first */}
               {peopleResults.length > 0 && (
                 <div className="search-section-header">
                   <h4>{t('searchBar.people')}</h4>
                 </div>
               )}
               {peopleResults.map((p, index)=>(
                  <div 
                    className={getItemClassName(index)}
                    key={p.personId}
                    tabIndex={0}
                    onClick={() => handlePersonClick(p)}
                    onKeyDown={(e) => e.key === 'Enter' && handlePersonClick(p)}
                    onMouseEnter={() => setSelectedIndex(index)}>
                     <div className="search-result-item-content">
                        <div className="search-result-item-name">{p.firstName} {p.lastName}</div>
                        <div className="search-result-item-details">
                          <span className="search-result-item-details-icon">⌊</span>
                          {p.teamName ?? t('searchBar.noData')}
                        </div>
                     </div>
                   </div>
                ))}
                {/* Team results second */}
                {searchResults.length > 0 && (
                  <div className="search-section-header">
                    <h4>{t('searchBar.teams')}</h4>
                  </div>
                )}
                {searchResults.map((result, index) => {
                   const actualIndex = peopleResults.length + index;
                   return (
                     <div 
                     tabIndex={0} 
                     className={getItemClassName(actualIndex)}
                     key={result.teamId || result.id} 
                     onClick={() => handleTeamClick(result)}
                     onKeyDown={(e) => e.key === 'Enter' && handleTeamClick(result)}
                     onMouseEnter={() => setSelectedIndex(actualIndex)}>
                        <div className="search-result-item-content">
                           <div className="search-result-item-name">{result.teamName}</div>
                        </div>
                      </div>
                   );
                })}
                {/* Club results third */}
                {clubResults.length > 0 && (
                  <div className="search-section-header">
                    <h4>{t('searchBar.clubs')}</h4>
                  </div>
                )}
                {clubResults.map((clubName, index) => {
                   const actualIndex = peopleResults.length + searchResults.length + index;
                   return (
                     <div 
                       className={getItemClassName(actualIndex)}
                       key={clubName}
                       tabIndex={0}
                       onClick={() => handleClubClick(clubName)}
                       onKeyDown={(e) => e.key === 'Enter' && handleClubClick(clubName)}
                       onMouseEnter={() => setSelectedIndex(actualIndex)}>
                      <div className="search-result-item-content">
                         <div className="search-result-item-name">{clubName}</div>
                      </div>
                    </div>
                   );
                })}
            </div>
         )}

      </div>
   );
}

export default SearchBar;
