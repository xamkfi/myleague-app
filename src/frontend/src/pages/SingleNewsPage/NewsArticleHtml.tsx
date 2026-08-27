import { useEffect, useMemo, useState } from 'react';
import { hydrateMatchResultHtml } from '../../components/RichTextEditor/hydrateMatchResultHtml';
import { extractRelatedNewsTeams, type RelatedNewsTeam } from './extractRelatedNewsTeams';

type NewsArticleHtmlProps = {
  html: string;
};

export function useHydratedNewsHtml(html: string): { displayHtml: string; relatedTeams: RelatedNewsTeam[] } {
  const [displayHtml, setDisplayHtml] = useState(html);

  useEffect(() => {
    let cancelled = false;
    setDisplayHtml(html);
    if (!html) {
      return;
    }

    hydrateMatchResultHtml(html).then((nextHtml) => {
      if (!cancelled) {
        setDisplayHtml(nextHtml);
      }
    });

    return () => {
      cancelled = true;
    };
  }, [html]);

  const relatedTeams = useMemo(
    () => extractRelatedNewsTeams(displayHtml),
    [displayHtml]
  );

  return { displayHtml, relatedTeams };
}

export default function NewsArticleHtml({ html }: NewsArticleHtmlProps) {
  return (
    <div
      dangerouslySetInnerHTML={{ __html: html }}
      className="single-news-page__content-html"
    />
  );
}
