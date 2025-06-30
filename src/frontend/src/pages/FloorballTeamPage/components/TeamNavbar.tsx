import { useState } from 'react';
import './TeamNavbar.scss'

interface TeamNavbarProps {
   onTabChange?: (activeTab: string) => void;
}

export default function TeamNavbar({ onTabChange }: TeamNavbarProps) {
   const [activeTab, setActiveTab] = useState<string>('results');

   const tabs = [
      { id: 'results', label: '📅 Results', icon: '📅' },
      { id: 'roster', label: '📋 Roster', icon: '📋' },
      { id: 'stats', label: '📊 Stats', icon: '📊' },
      { id: 'standings', label: '🏆 Standings', icon: '🏆' }
   ];

   const handleTabClick = (tabId: string) => {
      setActiveTab(tabId);
      onTabChange?.(tabId);
   };

   return (
      <div className='team-navbar'>
         {tabs.map((tab) => (
            <div 
               key={tab.id}
               className={`team-navbar-btn ${activeTab === tab.id ? 'active' : ''}`}
               onClick={() => handleTabClick(tab.id)}
            >
               <span>{tab.label}</span>
            </div>
         ))}
      </div>
   )
}