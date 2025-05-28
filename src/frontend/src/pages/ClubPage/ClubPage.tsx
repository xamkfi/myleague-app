import React from 'react';
import { useParams } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import clubData from '../../sampledata/club_data.json';
import { slugify } from '../../utils/helpers';
import './ClubPage.scss';

interface TeamCardProps {
  teamName: string;
  division: string;
  headCoach: string;
  assistantCoach: string;
  squad: Array<{
    id: string;
    name: string;
    position: string;
    age: number;
    nationality: string;
    jerseyNumber: number;
  }>;
  trainingSchedule: {
    weekdays: string[];
    time: string;
  };
  ageGroup?: string;
}

const TeamCard = ({
  teamName,
  division,
  headCoach,
  assistantCoach,
  squad,
  trainingSchedule,
  ageGroup
}: TeamCardProps) => (
  <div className="team-card">
    <h3 className="team-name">{teamName}</h3>
    <div className="team-info">
      <p><strong>Division:</strong> {division}</p>
      {ageGroup && <p><strong>Age Group:</strong> {ageGroup}</p>}
      <p><strong>Head Coach:</strong> {headCoach}</p>
      <p><strong>Assistant Coach:</strong> {assistantCoach}</p>
      
      <div className="squad-section">
        <h4>Squad ({squad.length} players)</h4>
        <div className="squad-grid">
          {squad.map(player => (
            <div key={player.id} className="player-item">
              <span className="jersey-number">#{player.jerseyNumber}</span>
              <span className="player-name">{player.name}</span>
              <span className="player-position">{player.position}</span>
            </div>
          ))}
        </div>
      </div>

      <div className="training-schedule">
        <h4>Training Schedule</h4>
        <p><strong>Days:</strong> {trainingSchedule.weekdays.join(', ')}</p>
        <p><strong>Time:</strong> {trainingSchedule.time}</p>
      </div>
    </div>
  </div>
);

const ClubPage: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();

  // Find the club by slug
  const club = clubData.clubs.find((club) => slugify(club.clubInfo.name) === slug);

  if (!club) {
    return (
      <PageTemplate title="Club Not Found">
        <div style={{ padding: '2rem', textAlign: 'center' }}>
          <h2>Club not found</h2>
          <p>The club you are looking for does not exist.</p>
        </div>
      </PageTemplate>
    );
  }

  const { name, established, location, homeStadium, stadiumCapacity, clubColors, website, contactEmail } = club.clubInfo;

  return (
    <PageTemplate title={name}>
      <div className="club-page">
        <div className="club-info">
          <h2>{name}</h2>
          <ul>
            <li><strong>Established:</strong> {established}</li>
            <li><strong>Location:</strong> {location}</li>
            <li><strong>Home Stadium:</strong> {homeStadium} ({stadiumCapacity} seats)</li>
            <li><strong>Club Colors:</strong> {clubColors.join(', ')}</li>
            <li><strong>Website:</strong> <a href={`https://${website}`} target="_blank" rel="noopener noreferrer">{website}</a></li>
            <li><strong>Contact Email:</strong> <a href={`mailto:${contactEmail}`}>{contactEmail}</a></li>
          </ul>
        </div>

        <div className="teams-section">
          <h2>Teams</h2>
          <div className="teams-grid">
            <TeamCard {...club.teams.primary} />
            <TeamCard {...club.teams.secondary} />
            <TeamCard {...club.teams.junior} />
          </div>
        </div>
      </div>
    </PageTemplate>
  );
};

export default ClubPage; 