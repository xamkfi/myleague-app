import DOMPurify from 'dompurify';

interface RulesPreviewProps {
  title: string;
  value: string;
}

export default function RulesPreview({ title, value }: RulesPreviewProps) {
  return (
    <div className="max-w-4xl mx-auto mt-8 px-4">
      <div className="rules-container bg-white p-8 rounded-lg shadow-sm w-full">
        <h1 className="rules-title m-0 mb-6 text-3xl font-bold">{title}</h1>
        <div
          className="rules-html-content prose max-w-none"
          dangerouslySetInnerHTML={{
            __html: DOMPurify.sanitize(value),
          }}
        />
      </div>
    </div>
  );
}
