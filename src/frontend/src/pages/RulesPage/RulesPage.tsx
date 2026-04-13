import { useEffect, useState } from 'react';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { getPageContent, type PageContentResponse } from '../../api/page/pageContentService';
import DOMPurify from 'dompurify';
import './RulesPage.scss';

function RulesPage() {

    const [content, setContent] = useState<PageContentResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {

        const fetchRules = async () => {
            try {
                const data = await getPageContent("saannot");
                setContent(data);
            } catch (err) {
                console.error("Failed to load rules:", err);
                setError("Sääntöjä ei voitu ladata.");
            } finally {
                setIsLoading(false);
            }
        };

        fetchRules();

    }, []);

    return (
        <PageTemplate title="Säännöt">
            <div className="rules-container">

                {/* Loading */}
                {isLoading && (
                    <div className="rules-loading">
                        <p>Ladataan sääntöjä...</p>
                    </div>
                )}

                {/* Error */}
                {!isLoading && error && (
                    <div className="rules-error">
                        <p>{error}</p>
                    </div>
                )}

                {/* Content */}
                {!isLoading && !error && content && (
                    <>
                        <h1 className="rules-title">{content.title}</h1>

                        <div
                            className="rules-html-content"
                            dangerouslySetInnerHTML={{
                                __html: DOMPurify.sanitize(content.contentHtml)
                            }}
                        />
                    </>
                )}

                {/* Fallback */}
                {!isLoading && !error && !content && (
                    <div className="rules-empty">
                        <p>Sääntöjä ei löytynyt.</p>
                    </div>
                )}

            </div>
        </PageTemplate>
    );
}

export default RulesPage;
