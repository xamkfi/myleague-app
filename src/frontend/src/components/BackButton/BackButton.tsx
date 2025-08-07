import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import './BackButton.scss';

interface BackButtonProps {
  to?: string;
  text?: string;
  scrollThreshold?: number;
}

const BackButton = ({ 
  to = '..', 
  text,
  scrollThreshold = 0
}: BackButtonProps) => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [isScrolled, setIsScrolled] = useState(false);

  // Handle scroll detection for floating back button
  useEffect(() => {
    const handleScroll = () => {
      const scrollPosition = window.scrollY;
      setIsScrolled(scrollPosition > scrollThreshold);
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [scrollThreshold]);

  const buttonText = text || t('common.back', 'Back');

  return (
    <div className={`global-back-button-container ${isScrolled ? 'floating' : ''}`}>
      <button
        className="global-back-button"
        onClick={() => navigate(to)}
      >
        {buttonText}
      </button>
    </div>
  );
};

export default BackButton; 