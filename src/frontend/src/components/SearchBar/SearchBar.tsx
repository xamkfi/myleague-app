import './SearchBar.scss';
import { useTranslation } from 'react-i18next';
import { personApi } from '../../api/admin/personApi';
import { floorballTeamNameSearchService } from '../../api/floorball/floorballTeamNameSearchService';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { createTeamSlug } from '../../utils/slugUtils';
import { floorballTeamService } from '../../api/floorball/floorballTeamService';
import type { FloorballTeam } from '../../types/floorball/floorballTypes';

function SearchBar(props: any) {
   const { t } = useTranslation();
   const [searchQuery, setSearchQuery] = useState('');
   const [searchResults, setSearchResults] = useState<any[]>([]);
   const navigate = useNavigate();

   const onSeacrhClick = async () => {
      const response = await floorballTeamNameSearchService.getTeamNames(searchQuery);
      const results = response.data.slice(0, 5);
      setSearchResults(results);
   };

   const getTeam = async (teamId: string) => {
      const response = await floorballTeamService.getById(teamId);
      return response;
   }

   const onKeyPress = async (e: React.KeyboardEvent<HTMLInputElement>, functionality: string, teamId?: string) => {
      if (e.key === 'Enter') {
         switch (functionality) {
            case 'search':
               onSeacrhClick();
               break;
            case 'navigate':
               if (!teamId)
                  break;
            
               const team = await getTeam(teamId);
               navigate(`/team/${createTeamSlug(team)}`);
               break;
            default:
               break;
         }
      }
   };

   return (
      <div className="search-bar-container">

         {/* Search input */}
         <input
            type="text"
            placeholder={t('searchBar.placeholder')}
            className="search-bar-input"
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyDown={(e) => onKeyPress(e, 'search')}
         />

         <button 
            onClick={onSeacrhClick} 
            className="search-bar-button">
               {t('searchBar.button')}
         </button>

         {/* Search results */}
         {searchResults.length > 0 && (
            <div className="search-results">
               {searchResults.map((result) => (
                  <div 
                  tabIndex={0} 
                  className="search-result-item" 
                  key={result.id} 
                  onClick={() => navigate(`/team/${result.id}`)}
                  onKeyDown={(e) => onKeyPress(e, 'navigate', result.id)}>
                     <div className="search-result-item-content">
                        <div className="search-result-item-name">{result.name}</div>
                     </div>
                  </div>
               ))}
            </div>
         )}

      </div>
   );
}

export default SearchBar;
