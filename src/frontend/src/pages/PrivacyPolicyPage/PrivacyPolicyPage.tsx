import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './PrivacyPolicyPage.scss';

const SECTION_IDS = [
  'controller',
  'contact',
  'scope',
  'purposes',
  'data',
  'sources',
  'disclosure',
  'storage',
  'retention',
  'security',
  'rights',
  'complaints',
  'changes',
] as const;

/**
 * Sections that contain a highlighted TODO note reminding the site
 * administrator to fill in organisation-specific details.
 */
const SECTIONS_WITH_TODO: ReadonlySet<string> = new Set([
  'controller',
  'contact',
  'disclosure',
  'retention',
  'rights',
  'changes',
]);

interface BodyBlocksProps {
  body: string;
}

/**
 * Renders a translated body string. Paragraphs are separated by blank lines;
 * consecutive lines starting with "- " are rendered as a bullet list.
 */
function BodyBlocks({ body }: BodyBlocksProps) {
  const blocks = body.split('\n\n');

  return (
    <>
      {blocks.map((block, blockIndex) => {
        const lines = block.split('\n');
        const isList = lines.every((line) => line.startsWith('- '));

        if (isList) {
          return (
            <ul key={blockIndex}>
              {lines.map((line, lineIndex) => (
                <li key={lineIndex}>{line.slice(2)}</li>
              ))}
            </ul>
          );
        }

        return <p key={blockIndex}>{block}</p>;
      })}
    </>
  );
}

export default function PrivacyPolicyPage() {
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('privacyPage.title', 'Tietosuojaseloste')}>
      <div className="privacy-page">
        <h1 className="privacy-page__title">
          {t('privacyPage.title', 'Tietosuojaseloste')}
        </h1>

        <p className="privacy-page__intro">{t('privacyPage.intro')}</p>

        <div className="privacy-page__todo">{t('privacyPage.updatedTodo')}</div>

        {SECTION_IDS.map((sectionId, index) => (
          <section key={sectionId} className="privacy-page__section">
            <h2 className="privacy-page__section-title">
              {index + 1}. {t(`privacyPage.sections.${sectionId}.title`)}
            </h2>

            <BodyBlocks body={t(`privacyPage.sections.${sectionId}.body`)} />

            {SECTIONS_WITH_TODO.has(sectionId) && (
              <div className="privacy-page__todo">
                {t(`privacyPage.sections.${sectionId}.todo`)}
              </div>
            )}
          </section>
        ))}
      </div>
    </PageTemplate>
  );
}
