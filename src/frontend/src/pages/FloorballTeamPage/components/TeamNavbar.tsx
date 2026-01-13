import { useState } from 'react';
import './TeamNavbar.scss'
import { useTranslation } from 'react-i18next';

interface TeamNavbarProps {
   currentTab: string;
   onTabChange?: (activeTab: string) => void;
}

export default function TeamNavbar({ currentTab, onTabChange }: TeamNavbarProps) {
   const [activeTab, setActiveTab] = useState<string>(currentTab);
   const { t } = useTranslation();

   const tabs = [
      { id: 'summary', label: t('teamUserPage.summary'), icon: '🏠' },
      { id: 'results', label: t('teamUserPage.results') , icon: '📅' },
      { id: 'roster', label: t('teamUserPage.roster'), icon: '📋' },
      { id: 'stats', label: t('teamUserPage.stats.Stats'), icon: '📊' },
      { id: 'standings', label: t('teamUserPage.standings'), icon: '🏆' }
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
               data-icon={tab.icon}
            >
               <span>{tab.icon} {tab.label}</span>
            </div>
         ))}
      </div>
   )
}