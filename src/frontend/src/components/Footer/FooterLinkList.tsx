import type { FooterContact } from '../../types/admin/footerContactTypes';

interface FooterLinkListProps {
  items: FooterContact[];
  emptyLabel: string;
}

function FooterLinkList({ items, emptyLabel }: FooterLinkListProps) {
  if (items.length === 0) {
    return <p className="footer-contact-empty">{emptyLabel}</p>;
  }

  return (
    <div className="footer-links">
      {items.map((item) =>
        item.url ? (
          <a
            key={item.id}
            href={item.url}
            target="_blank"
            rel="noreferrer noopener"
          >
            {item.title}
          </a>
        ) : (
          <span key={item.id}>{item.title}</span>
        ),
      )}
    </div>
  );
}

export default FooterLinkList;
