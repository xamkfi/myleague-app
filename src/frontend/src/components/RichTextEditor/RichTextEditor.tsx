import { useCallback, useEffect, useMemo, useRef } from 'react';
import ReactQuill from 'react-quill';
import { useTranslation } from 'react-i18next';
import 'react-quill/dist/quill.snow.css';

import { handleImageUploadService } from '../../api/admin/News/handleImageUploadService';
import { handleImageDeleteService } from '../../api/admin/News/handleImageDeleteService';
import MatchSelectionHeader from '../../pages/AdminPage/NewsPage/components/MatchSelectionHeader';
import type { FloorballMatch } from '../../api/admin/News/GetMatchesService';
import {
  ensureMatchResultBlotRegistered,
  type MatchResultValue,
} from './MatchResultTableBlot';

import './RichTextEditor.scss';
import '../../pages/AdminPage/NewsPage/styles/MatchResult.scss';

// react-quill@2 ships typings that reference Quill v1 names (Sources,
// DeltaStatic, UnprivilegedEditor). Quill v2 no longer exposes those names
// directly, so we re-declare the small subset we need locally to avoid
// pulling in mismatched ambient types.
type QuillChangeSource = 'user' | 'api' | 'silent';

ensureMatchResultBlotRegistered();

export type RichTextEditorVariant = 'default' | 'compact';

export interface RichTextEditorProps {
  /** Current HTML content. */
  value: string;
  /** Called with the updated HTML whenever the user edits the content. */
  onChange: (value: string) => void;
  /** Optional callback fired while an image is being uploaded. */
  onUploadingChange?: (uploading: boolean) => void;
  /** Show the "Lisää otteluita / Add matches" button above the editor. */
  showMatchInsert?: boolean;
  /** Stable id for the editor host element (useful for labels). */
  id?: string;
  /** Placeholder shown in the editor when empty. */
  placeholder?: string;
  /** Visual size preset. */
  variant?: RichTextEditorVariant;
  /** Read-only mode. */
  readOnly?: boolean;
  /** Extra className applied to the outer wrapper. */
  className?: string;
}

/** Parses editor content as XML to avoid reinterpreting untrusted text as HTML. */
const parseEditorXmlDocument = (html: string): XMLDocument => {
  return new DOMParser().parseFromString(`<root>${html}</root>`, 'application/xml');
};

const extractImageUrls = (html: string): string[] => {
  if (!html) return [];
  const doc = parseEditorXmlDocument(html);
  return Array.from(doc.getElementsByTagName('img'))
    .map((img) => img.getAttribute('src') ?? '')
    .filter(Boolean);
};

const extractMatchResults = (html: string): MatchResultValue[] => {
  if (!html) return [];
  const doc = parseEditorXmlDocument(html);
  const containers = Array.from(doc.getElementsByTagName('span')).filter((element) =>
    (element.getAttribute('class') ?? '').split(/\s+/).includes('match-result-table-container')
  );
  const results: MatchResultValue[] = [];
  containers.forEach((element) => {
    const dataElement = Array.from(element.getElementsByTagName('span')).find((child) =>
      (child.getAttribute('class') ?? '').split(/\s+/).includes('match-result-data')
    );
    if (!dataElement?.textContent) return;
    try {
      const parsed = JSON.parse(dataElement.textContent) as { matches?: MatchResultValue[] };
      if (parsed?.matches) {
        results.push(...parsed.matches);
      }
    } catch {
      // Ignore malformed embeds — they will be removed on the next save.
    }
  });
  return results;
};

/**
 * Shared rich-text editor used across admin pages (news, tournaments, …).
 *
 * Built on Quill. Adds:
 *  - Inline image upload via the admin News upload endpoint
 *  - Automatic deletion of orphan images on the server when the user removes
 *    them from the editor (with a confirm prompt)
 *  - Optional "insert match results" button that embeds a custom Quill blot
 */
const RichTextEditor = ({
  value,
  onChange,
  onUploadingChange,
  showMatchInsert = true,
  id,
  placeholder,
  variant = 'default',
  readOnly = false,
  className,
}: RichTextEditorProps) => {
  const { t } = useTranslation();
  const quillRef = useRef<ReactQuill | null>(null);

  // Track the previous user-visible content so we can detect deletions that
  // happen as a result of *user* actions (not programmatic prop updates).
  // Kept in sync with `value` so external loads (e.g. fetching an article in
  // edit mode) and programmatic clears (after publishing) don't trigger the
  // "are you sure you want to delete N images?" prompt.
  const lastUserHtmlRef = useRef<string>(value);
  useEffect(() => {
    lastUserHtmlRef.current = value;
  }, [value]);

  const confirmDeletions = useCallback(
    (deletedImages: string[], deletedMatches: MatchResultValue[]): boolean => {
      if (deletedImages.length === 0 && deletedMatches.length === 0) return true;

      let message: string;
      if (deletedImages.length > 0 && deletedMatches.length > 0) {
        message = t(
          'admin.editor.confirmDeleteBoth',
          'Are you sure you want to delete {{images}} image(s) and {{matches}} match result(s)?',
          { images: deletedImages.length, matches: deletedMatches.length }
        );
      } else if (deletedImages.length > 0) {
        message = t(
          'admin.editor.confirmDeleteImages',
          'Are you sure you want to delete {{count}} image(s)?',
          { count: deletedImages.length }
        );
      } else {
        message = t(
          'admin.editor.confirmDeleteMatches',
          'Are you sure you want to delete {{count}} match result(s)?',
          { count: deletedMatches.length }
        );
      }

      return window.confirm(message);
    },
    [t]
  );

  const reinsertElements = useCallback(
    (images: string[], matches: MatchResultValue[]): void => {
      const editor = quillRef.current?.getEditor();
      if (!editor) return;
      const range = editor.getSelection();
      const index = range?.index ?? editor.getLength();
      images.forEach((url) => editor.insertEmbed(index, 'image', url));
      matches.forEach((match) => editor.insertEmbed(index, 'matchResultTable', { matches: [match] }));
    },
    []
  );

  const handleChange = useCallback(
    (content: string, _delta: unknown, source: QuillChangeSource) => {
      // Only react to user-driven changes. Programmatic updates (e.g. parent
      // setting `value` after loading an article) come through with source
      // 'api' and must not trigger a destructive confirm dialog.
      if (source !== 'user') {
        lastUserHtmlRef.current = content;
        onChange(content);
        return;
      }

      const previousImages = extractImageUrls(lastUserHtmlRef.current);
      const previousMatches = extractMatchResults(lastUserHtmlRef.current);
      const currentImages = extractImageUrls(content);
      const currentMatches = extractMatchResults(content);

      const deletedImages = previousImages.filter((url) => !currentImages.includes(url));
      const deletedMatches = previousMatches.filter(
        (prev) => !currentMatches.some((curr) => JSON.stringify(curr) === JSON.stringify(prev))
      );

      if (deletedImages.length === 0 && deletedMatches.length === 0) {
        lastUserHtmlRef.current = content;
        onChange(content);
        return;
      }

      const confirmed = confirmDeletions(deletedImages, deletedMatches);
      if (!confirmed) {
        // Restore the deleted embeds. We deliberately commit `content` first
        // because Quill has already applied the deletion internally; the
        // re-insert call below puts the embeds back in.
        lastUserHtmlRef.current = content;
        onChange(content);
        reinsertElements(deletedImages, deletedMatches);
        return;
      }

      // User confirmed — delete the now-orphaned uploaded files server-side.
      deletedImages.forEach((url) => {
        handleImageDeleteService(url).catch((err) => {
          console.error('Failed to delete image:', err);
        });
      });

      lastUserHtmlRef.current = content;
      onChange(content);
    },
    [confirmDeletions, onChange, reinsertElements]
  );

  const openImageUploader = useCallback(() => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = async () => {
      if (!input.files?.length) return;
      const file = input.files[0];
      try {
        onUploadingChange?.(true);
        const imageUrl = await handleImageUploadService(file);
        const editor = quillRef.current?.getEditor();
        if (editor) {
          const range = editor.getSelection(true);
          const index = range?.index ?? editor.getLength();
          editor.insertEmbed(index, 'image', imageUrl);
          editor.setSelection({ index: index + 1, length: 0 });
        }
      } catch (error) {
        console.error('Image upload error:', error);
        window.alert(t('admin.editor.uploadFailed', 'Image upload failed.'));
      } finally {
        onUploadingChange?.(false);
      }
    };
    input.click();
  }, [onUploadingChange, t]);

  const modules = useMemo(
    () => ({
      toolbar: {
        container: [
          [{ header: [1, 2, 3, 4, 5, 6, false] }],
          ['bold', 'italic', 'underline', 'strike'],
          ['blockquote'],
          [{ list: 'ordered' }, { list: 'bullet' }, { indent: '-1' }, { indent: '+1' }],
          ['link', 'image'],
          ['clean'],
        ],
        handlers: {
          image: openImageUploader,
        },
      },
    }),
    [openImageUploader]
  );

  const handleInsertMatches = useCallback((matches: FloorballMatch[]) => {
    const editor = quillRef.current?.getEditor();
    if (!editor || matches.length === 0) return;

    const range = editor.getSelection(true);
    const index = range?.index ?? editor.getLength();

    const matchesData: MatchResultValue[] = matches.map((match) => ({
      homeTeam: match.homeTeamName,
      awayTeam: match.awayTeamName,
      homeScore: match.homeScore,
      awayScore: match.awayScore,
      date: match.scheduledDateTime,
      status: match.status,
      link: match.id,
      homeTeamImage: match.homeTeamLogo ?? undefined,
      awayTeamImage: match.awayTeamLogo ?? undefined,
    }));

    editor.insertEmbed(index, 'matchResultTable', { matches: matchesData });
    editor.setSelection({ index: index + 1, length: 0 });
  }, []);

  return (
    <div
      id={id}
      className={[
        'rich-text-editor',
        `rich-text-editor--${variant}`,
        className ?? '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      {showMatchInsert && !readOnly && (
        <div className="rich-text-editor__toolbar-row">
          <MatchSelectionHeader onInsertMatches={handleInsertMatches} />
        </div>
      )}
      <ReactQuill
        ref={(element) => {
          if (element != null) {
            quillRef.current = element;
          }
        }}
        className="rich-text-editor__quill"
        theme="snow"
        value={value}
        onChange={handleChange}
        modules={modules}
        readOnly={readOnly}
        placeholder={placeholder}
      />
    </div>
  );
};

export default RichTextEditor;
