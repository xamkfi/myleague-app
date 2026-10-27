import { useEffect, useRef } from 'react';
import { bindTeamLogoFallbacks } from '../../components/RichTextEditor/matchResultRender';
import '../AdminPage/NewsPage/styles/MatchResult.scss';

type NewsArticleHtmlProps = {
  html: string;
};

export default function NewsArticleHtml({ html }: NewsArticleHtmlProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (containerRef.current) {
      bindTeamLogoFallbacks(containerRef.current);
    }
  }, [html]);

  return (
    <div
      ref={containerRef}
      dangerouslySetInnerHTML={{ __html: html }}
      className="single-news-page__content-html"
    />
  );
}
