import './SearchBar.scss';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useRef } from 'react';
import { createClubSlug } from '../../utils/slugUtils';
import { slugify } from '../../utils/slugUtils';
import { globalSearchService } from '../../api/common/globalSearchService';
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
   const [selectedIndex, setSelectedIndex] = useState(-1);
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

   // Debounced search effect
   useEffect(() => {
    const handler = setTimeout(() => {
      if (searchQuery.length >= 1) {
        onSeacrhClick();
      } else {
        // Clear results if query is too short
        setHasSearched(false);
        setSearchResults([]);
        setPeopleResults([]);
        setClubResults([]);
      }
    }, 300); // 500ms delay

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
   }, [isVisible, hasSearched, peopleResults.length, searchResults.length, clubResults.length, selectedIndex]);

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
     const action = () => navigate(`/floorballplayer/${person.personId}`);
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

   const handleSelectedItem = () => {
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
   };

   const getItemClassName = (index: number) => {
     const baseClass = 'search-result-item';
     return selectedIndex === index ? `${baseClass} selected` : baseClass;
   };

   // Add handleTransitionEnd before return
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

   return (
      <div className="search-bar-container" ref={searchContainerRef}>

         {/* Search input */}
         <input
            type="text"
            placeholder={t('searchBar.placeholder')}
            className="search-bar-input"
            onChange={(e) => setSearchQuery(e.target.value)}
            onFocus={() => setIsSearchFocused(true)}
         />

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
                        <div className="search-result-item-details">⌊{p.teamName ?? "No data"}</div>
                     </div>
                   </div>
                ))}
                {/* Team results second */}
                {searchResults.length > 0 && (
                  <div className="search-section-header">
                    <h4>Teams</h4>
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
                    <h4>Clubs</h4>
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
