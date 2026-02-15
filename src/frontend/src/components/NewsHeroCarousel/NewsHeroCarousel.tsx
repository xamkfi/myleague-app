import { useState, useEffect, useCallback, useRef } from 'react';
import MainNewsCard from '../MainNewsCard/MainNewsCard';
import type { NewsArticleDto } from '../../api/news/newsService';
import './NewsHeroCarousel.scss';

interface NewsHeroCarouselProps {
  newsArticles: NewsArticleDto[];
}

const AUTO_ROTATE_INTERVAL = 10000;

function NewsHeroCarousel({ newsArticles }: NewsHeroCarouselProps) {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isHovered, setIsHovered] = useState(false);
  const [isFading, setIsFading] = useState(false);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const articleCount = newsArticles.length;

  const changeSlide = useCallback((newIndex: number) => {
    setIsFading(true);
    setTimeout(() => {
      setCurrentIndex(newIndex);
      setIsFading(false);
    }, 300);
  }, []);

  const goToNext = useCallback(() => {
    const nextIndex = (currentIndex + 1) % articleCount;
    changeSlide(nextIndex);
  }, [currentIndex, articleCount, changeSlide]);

  const goToPrev = useCallback(() => {
    const prevIndex = (currentIndex - 1 + articleCount) % articleCount;
    changeSlide(prevIndex);
  }, [currentIndex, articleCount, changeSlide]);

  const goToSlide = useCallback((index: number) => {
    if (index !== currentIndex) {
      changeSlide(index);
    }
  }, [currentIndex, changeSlide]);

  // Auto-rotation
  useEffect(() => {
    if (isHovered || articleCount <= 1) {
      if (timerRef.current) {
        clearInterval(timerRef.current);
        timerRef.current = null;
      }
      return;
    }

    timerRef.current = setInterval(() => {
      setIsFading(true);
      setTimeout(() => {
        setCurrentIndex((prev) => (prev + 1) % articleCount);
        setIsFading(false);
      }, 300);
    }, AUTO_ROTATE_INTERVAL);

    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
        timerRef.current = null;
      }
    };
  }, [isHovered, articleCount]);

  // Reset timer on manual navigation
  const resetTimer = useCallback(() => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
    if (!isHovered && articleCount > 1) {
      timerRef.current = setInterval(() => {
        setIsFading(true);
        setTimeout(() => {
          setCurrentIndex((prev) => (prev + 1) % articleCount);
          setIsFading(false);
        }, 300);
      }, AUTO_ROTATE_INTERVAL);
    }
  }, [isHovered, articleCount]);

  const handleNext = useCallback(() => {
    goToNext();
    resetTimer();
  }, [goToNext, resetTimer]);

  const handlePrev = useCallback(() => {
    goToPrev();
    resetTimer();
  }, [goToPrev, resetTimer]);

  const handleDotClick = useCallback((index: number) => {
    goToSlide(index);
    resetTimer();
  }, [goToSlide, resetTimer]);

  if (articleCount === 0) {
    return null;
  }

  return (
    <div
      className="news-hero-carousel"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div className={`news-hero-carousel__slide ${isFading ? 'news-hero-carousel__slide--fading' : ''}`}>
        <MainNewsCard news={newsArticles[currentIndex]} />
      </div>

      {/* Navigation arrows */}
      {articleCount > 1 && (
        <>
          <button
            className="news-hero-carousel__arrow news-hero-carousel__arrow--prev"
            onClick={handlePrev}
            aria-label="Previous news"
            type="button"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M15 18L9 12L15 6" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </button>
          <button
            className="news-hero-carousel__arrow news-hero-carousel__arrow--next"
            onClick={handleNext}
            aria-label="Next news"
            type="button"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M9 18L15 12L9 6" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </button>
        </>
      )}

      {/* Dot indicators */}
      {articleCount > 1 && (
        <div className="news-hero-carousel__dots">
          {newsArticles.map((_, index) => (
            <button
              key={newsArticles[index].id}
              className={`news-hero-carousel__dot ${index === currentIndex ? 'news-hero-carousel__dot--active' : ''}`}
              onClick={() => handleDotClick(index)}
              aria-label={`Go to news ${index + 1}`}
              type="button"
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default NewsHeroCarousel;
