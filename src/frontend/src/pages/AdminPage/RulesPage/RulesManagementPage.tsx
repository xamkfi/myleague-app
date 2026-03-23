import { useState, useEffect } from "react";
import PageTemplate from "../../../components/PageTemplate/AdminPageTemplate";
import QuillEditor from "../../../components/QuillEditor/QuillEditor";
import { getPageContent, updatePageContent } from "../../../api/page/pageContentService";
import LoadingSpinner from "../../../components/LoadingSpinner/LoadingSpinner";
import RulesPreview from "./components/RulesPreview.tsx";
import "./styles/RulesManagementPage.scss";

export default function RulesManagementPage() {
  const [value, setValue] = useState("");
  const [title, setTitle] = useState("");
  const [preview, setPreview] = useState(false);
  const [loadingAnimation, setLoadingAnimation] = useState(false);
  const [isLoadingContent, setIsLoadingContent] = useState(true);
  const [metadata, setMetadata] = useState<{ lastModifiedBy: string | null; updatedAt: string } | null>(null);

  useEffect(() => {
    const fetchRulesContent = async () => {
      try {
        setIsLoadingContent(true);
        const data = await getPageContent("saannot");
        setTitle(data.title || '');
        setValue(data.contentHtml || '');
        setMetadata({
            lastModifiedBy: data.lastModifiedBy,
            updatedAt: data.updatedAt
        });
      } catch (error) {
        console.error('Failed to fetch rules content:', error);
      } finally {
        setIsLoadingContent(false);
      }
    };
    fetchRulesContent();
  }, []);

  const handleSave = async () => {
    if (!title.trim() || !value.trim()) {
      alert("Otsikko ja sisältö eivät voi olla tyhjiä.");
      return;
    }

    const confirmAction = window.confirm("Haluatko varmasti tallentaa säännöt?");
    if (!confirmAction) return;

    try {
      setLoadingAnimation(true);
      const data = await updatePageContent("saannot", {
        title: title.trim(),
        contentHtml: value.trim()
      });
      setMetadata({
          lastModifiedBy: data.lastModifiedBy,
          updatedAt: data.updatedAt
      });
      alert("Säännöt tallennettu onnistuneesti!");
    } catch (err) {
      console.error("Failed to save rules:", err);
      alert("Sääntöjen tallentaminen epäonnistui. Yritä uudelleen.");
    } finally {
      setLoadingAnimation(false);
    }
  };

  if (isLoadingContent) {
    return (
      <PageTemplate title="Ladataan...">
        <div className="flex justify-center items-center min-h-screen">
          <LoadingSpinner text="Ladataan sisältöä..." />
        </div>
      </PageTemplate>
    );
  }

  if (preview) {
    return (
      <PageTemplate title="Sääntöjen hallinta (Esikatselu)">
        <div className="rules-management-page min-h-screen pb-10">
          <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
            <div className="max-w-6xl mx-auto px-4 py-3">
              <div className="flex justify-between items-center">
                <div className="flex items-center gap-3">
                  <span className="rules-preview-badge text-sm font-medium px-3 py-1 rounded-full">
                    Esikatselutila
                  </span>
                </div>
                <button 
                  onClick={() => setPreview(false)}
                  className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors flex items-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                  Muokkaa sisältöä
                </button>
              </div>
            </div>
          </div>
          <RulesPreview title={title} value={value} />
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title="Sääntöjen hallinta">
      <div className="rules-management-page max-w-6xl mx-auto space-y-8">
        <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
          <div className="max-w-6xl mx-auto px-4 py-3">
            <div className="flex justify-between items-center flex-wrap gap-4">
              <div className="flex items-center gap-3">
                <span className="text-gray-600 text-sm">
                  Muokkaa MAHL-sääntösivun sisältöä
                </span>
                {metadata && (
                  <span className="rules-meta text-xs text-gray-500 px-2 py-1 rounded">
                    Viimeksi muokannut: {metadata.lastModifiedBy || 'Nimetön'} ({new Date(metadata.updatedAt).toLocaleString('fi-FI')})
                  </span>
                )}
              </div>
              <div className="flex gap-3">
                <button
                  onClick={handleSave}
                  disabled={loadingAnimation}
                  className={`px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2 ${loadingAnimation ? 'opacity-50 cursor-not-allowed' : ''}`}
                >
                  {loadingAnimation ? 'Tallennetaan...' : 'Tallenna säännöt'}
                </button>
                <button 
                  onClick={() => setPreview(true)}
                  className="px-4 py-2 bg-green-200 text-green-700 hover:bg-green-200 rounded-lg font-medium transition-colors flex items-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                  Esikatselu
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="rules-editor-card bg-white rounded-xl shadow-sm p-6 mb-8 mt-4">
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">Otsikko</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500"
              placeholder="Esim. MAHL Säännöt"
            />
          </div>

          <div className="mb-2">
            <label className="block text-sm font-medium text-gray-700 mb-2">Sisältö</label>
            <QuillEditor
              value={value}
              setValue={setValue}
              setLoading={setLoadingAnimation}
              showMatchSelection={false}
            />
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}
