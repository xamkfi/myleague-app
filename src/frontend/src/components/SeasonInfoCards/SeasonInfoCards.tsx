import DOMPurify from 'dompurify';
import type { SeasonContentBlockDto } from '../../types/common/seasonContent';
import './SeasonInfoCards.scss';

export interface SeasonInfoCardsProps {
  blocks: SeasonContentBlockDto[];
  className?: string;
}

export default function SeasonInfoCards({ blocks, className }: SeasonInfoCardsProps) {
  if (blocks.length === 0) {
    return null;
  }

  return (
    <div className={className}>
      {blocks.map((block) => (
        <article key={block.id} className="season-info-card">
          <h2 className="season-info-card__title">{block.title}</h2>
          <div
            className="season-info-card__body"
            dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(block.contentHtml) }}
          />
        </article>
      ))}
    </div>
  );
}
