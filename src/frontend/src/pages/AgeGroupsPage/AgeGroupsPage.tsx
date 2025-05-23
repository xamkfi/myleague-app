import React from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './AgeGroupsPage.css';

function AgeGroupsPage() {
  const { t } = useTranslation();
  
  const ageGroups = [
    { id: 1, name: 'U12', description: 'Under 12 years old' },
    { id: 2, name: 'U15', description: 'Under 15 years old' },
    { id: 3, name: 'U18', description: 'Under 18 years old' },
    { id: 4, name: 'Adult', description: '18+ years old' },
    { id: 5, name: 'Senior', description: '40+ years old' }
  ];
  
  return (
    <PageTemplate title={t('nav.ageGroups')}>
      <div className="age-groups-container">
        <p className="intro-text">Select your age group to see relevant information and teams.</p>
        
        <div className="age-groups-grid">
          {ageGroups.map(group => (
            <div key={group.id} className="age-group-card">
              <h2 className="age-group-title">{group.name}</h2>
              <p className="age-group-description">{group.description}</p>
              <button className="age-group-button">View Details</button>
            </div>
          ))}
        </div>
      </div>
    </PageTemplate>
  );
}

export default AgeGroupsPage; 