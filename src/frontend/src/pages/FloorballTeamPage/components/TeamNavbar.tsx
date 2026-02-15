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
      { id: 'summary', label: t('teamUserPage.summary') },
      { id: 'results', label: t('teamUserPage.results') },
      { id: 'roster', label: t('teamUserPage.roster') },
      { id: 'stats', label: t('teamUserPage.stats.Stats') },
      { id: 'standings', label: t('teamUserPage.standings') }
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