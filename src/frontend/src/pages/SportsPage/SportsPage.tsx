import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './SportsPage.scss';

function SportsPage() {
  const { t } = useTranslation();
  
  const sports = [
    {
      id: 1,
      name: 'Hockey',
      image: 'https://via.placeholder.com/300x200?text=Hockey',
      description: 'Ice hockey leagues and tournaments for all age groups'
    },
    {
      id: 2,
      name: 'Football',
      image: 'https://via.placeholder.com/300x200?text=Football',
      description: 'Football (soccer) leagues and events throughout the year'
    },
    {
      id: 3,
      name: 'Basketball',
      image: 'https://via.placeholder.com/300x200?text=Basketball',
      description: 'Basketball leagues for youth and adult teams'
    },
    {
      id: 4,
      name: 'Volleyball',
      image: 'https://via.placeholder.com/300x200?text=Volleyball',
      description: 'Indoor and beach volleyball tournaments and leagues'
    }
  ];
  
  return (
    <PageTemplate title={t('nav.sports')}>
      <div className="sports-container">
        <p className="sports-intro">Explore the different sports offered by our league.</p>
        
        <div className="sports-grid">
          {sports.map(sport => (
            <div key={sport.id} className="sport-card">
              <div className="sport-image">
                <img src={sport.image} alt={sport.name} />
              </div>
              <div className="sport-content">
                <h2 className="sport-title">{sport.name}</h2>
                <p className="sport-description">{sport.description}</p>
                <button className="sport-button">Learn More</button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </PageTemplate>
  );
}

export default SportsPage; 