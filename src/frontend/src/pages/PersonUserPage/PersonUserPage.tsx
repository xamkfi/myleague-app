import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import type { Player, TeamsData, Team, Goalkeeper, FieldPlayer } from "../../types/playerTypes";
import PageTemplate from "../../components/PageTemplate/PageTemplate";
import './PersonUserPage.scss';

interface PersonWithTeams {
  Id: number;
  Name: string;
  Age: number;
  teams: Team[];
  totalMatchesPlayed: number;
}

// API function to fetch person's team data
const fetchPersonTeams = async (personId: number): Promise<PersonWithTeams | null> => {
  try {
    // Example API endpoint - replace with your actual backend URL
    const response = await fetch(`/api/persons/${personId}/teams`);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching person teams:', error);
    throw error;
  }
};

const PersonUserPage = () => {
  const { id } = useParams<{ id: string }>();
  const [person, setPerson] = useState<PersonWithTeams | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadPersonData = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        setError(null);
        
        const personId = parseInt(id, 10);
        
        const personData = await fetchPersonTeams(personId);
        
        setPerson(personData);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    loadPersonData();
  }, [id]);

  if (loading) return <PageTemplate title="Henkilö"><div>Ladataan...</div></PageTemplate>;
  if (error) return <PageTemplate title="Henkilö"><div>Virhe: {error}</div></PageTemplate>;
  if (!person) return <PageTemplate title="Henkilö"><div>Henkilöä ei löytynyt</div></PageTemplate>;

  return (
    <PageTemplate title={person.Name}>
      <div className="person-container">
        <div className="person-header">
          <div className="person-avatar"></div>
          <div className="person-info">
            <div className="person-name">{person.Name}</div>
            <div className="person-subtitle">Ikä: {person.Age}</div>
            <div className="person-subtitle">Joukkueita: {person.teams.length}</div>
          </div>
        </div>

        <div className="teams-section">
          <h3>Joukkueet</h3>
          <div className="teams-list">
            {person.teams.map(team => {
              const allPlayers: (Goalkeeper | FieldPlayer)[] = [...team.Players.Goalkeepers, ...team.Players.Fieldplayers];
              const playerInTeam = allPlayers.find(p => p.Id === person.Id);
              const isGoalkeeper = team.Players.Goalkeepers.some(p => p.Id === person.Id);
              
              return (
                <div key={team.Id} className="team-card">
                  <div className="team-header">
                    <div className="team-name">{team.Name}</div>
                    <div className="team-role">{isGoalkeeper ? "Maalivahti" : "Kenttäpelaaja"}</div>
                  </div>
                  <div className="team-stats">
                    <div className="team-stat">
                      <span className="stat-label">Pelinumero:</span>
                      <span className="stat-value">{playerInTeam?.Number}</span>
                    </div>
                    <div className="team-stat">
                      <span className="stat-label">Ottelut:</span>
                      <span className="stat-value">{playerInTeam?.MatchesPlayed}</span>
                    </div>
                    {!isGoalkeeper && playerInTeam && 'GoalsScored' in playerInTeam && (
                      <>
                        <div className="team-stat">
                          <span className="stat-label">Maalit:</span>
                          <span className="stat-value">{playerInTeam.GoalsScored}</span>
                        </div>
                        <div className="team-stat">
                          <span className="stat-label">Syötöt:</span>
                          <span className="stat-value">{playerInTeam.Assists}</span>
                        </div>
                      </>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </PageTemplate>
  );
};

export default PersonUserPage;
