import React from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate';
import './TournamentsPage.css';

const TournamentsPage: React.FC = () => {
  const { t } = useTranslation();
  
  const tournaments = [
    {
      id: 1,
      name: 'Summer Cup 2023',
      date: 'June 15-20, 2023',
      location: 'Helsinki',
      description: 'A summer tournament for all age groups',
      registrationOpen: true
    },
    {
      id: 2,
      name: 'Fall Tournament',
      date: 'September 5-10, 2023',
      location: 'Tampere',
      description: 'A fall tournament focusing on youth teams',
      registrationOpen: true
    },
    {
      id: 3,
      name: 'Winter Cup',
      date: 'December 15-20, 2023',
      location: 'Oulu',
      description: 'Our traditional winter tournament for all age groups',
      registrationOpen: false
    }
  ];
  
  return (
    <PageTemplate title={t('nav.tournaments')}>
      <div className="tournaments-container">
        <p className="tournaments-intro">Browse our upcoming tournaments and register your team.</p>
        
        <div className="tournaments-list">
          {tournaments.map(tournament => (
            <div key={tournament.id} className="tournament-card">
              <div className="tournament-header">
                <h2 className="tournament-title">{tournament.name}</h2>
                {tournament.registrationOpen ? (
                  <span className="registration-open">Registration Open</span>
                ) : (
                  <span className="registration-closed">Registration Closed</span>
                )}
              </div>
              
              <div className="tournament-details">
                <p><strong>Date:</strong> {tournament.date}</p>
                <p><strong>Location:</strong> {tournament.location}</p>
                <p>{tournament.description}</p>
              </div>
              
              <div className="tournament-actions">
                <button className="view-details-button">View Details</button>
                {tournament.registrationOpen && (
                  <button className="register-button">Register Team</button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </PageTemplate>
  );
};

export default TournamentsPage; 