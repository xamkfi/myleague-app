import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import type { Club } from '../../api/clubService';
import { getClubs } from '../../api/clubService';
//import clubData from '../../sampledata/club_data.json';
//import { slugify } from '../../utils/helpers';
import './ClubPage.scss';

function ClubPage() {
  const { id } = useParams<{ id: string }>();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchClubs = async () => {
      try {
        const clubsData = await getClubs();
        setClubs(clubsData);
        setLoading(false);
      } catch {
        setError('Failed to load clubs. Please try again later.');
        setLoading(false);
      }
    };

    fetchClubs();
  }, []);

  if (loading) {
    return (
      <PageTemplate title="Loading...">
        <div style={{ padding: '2rem', textAlign: 'center' }}>
          <h2>Loading club information...</h2>
        </div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title="Error">
        <div style={{ padding: '2rem', textAlign: 'center' }}>
          <h2>Error</h2>
          <p>{error}</p>
        </div>
      </PageTemplate>
    );
  }

  const club = clubs.find((club) => club.id === id);

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

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  return (
    <PageTemplate title={club.name}>
      <div className="club-page">
        <div className="club-info">
          {club.logoUrl && (
            <div className="club-logo">
              <img src={club.logoUrl} alt={`${club.name} logo`} />
            </div>
          )}
          <h2>{club.name}</h2>
          <ul>
            <li><strong>Founded:</strong> {formatDate(club.foundingDate)}</li>
            <li><strong>Location:</strong> {club.city}, {club.country}</li>
            <li><strong>Website:</strong> <a href={club.websiteUrl} target="_blank" rel="noopener noreferrer">{club.websiteUrl}</a></li>
            <li><strong>Contact:</strong> <a href={`mailto:${club.contactEmail}`}>{club.contactEmail}</a></li>
          </ul>
        </div>
      </div>
    </PageTemplate>
  );
}

export default ClubPage; 