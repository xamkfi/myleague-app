import { useTranslation } from 'react-i18next';
import RichTextEditor from '../RichTextEditor';
import {
  createEmptyContentBlockDraft,
  type SeasonContentBlockDraft,
} from '../../types/common/seasonContent';
import './SeasonContentBlocksEditor.scss';

export interface SeasonContentBlocksEditorProps {
  blocks: SeasonContentBlockDraft[];
  onChange: (blocks: SeasonContentBlockDraft[]) => void;
  disabled?: boolean;
}

export default function SeasonContentBlocksEditor({
  blocks,
  onChange,
  disabled = false,
}: SeasonContentBlocksEditorProps) {
  const { t } = useTranslation();

  const updateBlock = (clientId: string, patch: Partial<SeasonContentBlockDraft>): void => {
    onChange(blocks.map((block) => (block.clientId === clientId ? { ...block, ...patch } : block)));
  };

  const moveBlock = (index: number, direction: -1 | 1): void => {
    const nextIndex = index + direction;
    if (nextIndex < 0 || nextIndex >= blocks.length) {
      return;
    }
    const next = [...blocks];
    const [moved] = next.splice(index, 1);
    next.splice(nextIndex, 0, moved);
    onChange(next);
  };

  return (
    <div className="season-content-blocks-editor">
      <div className="season-content-blocks-editor__header">
        <div>
          <h3 className="form-section__title">
            <i className="fas fa-align-left"></i>
            {t('seasonContent.title')}
          </h3>
          <p className="season-content-blocks-editor__hint">{t('seasonContent.description')}</p>
        </div>
        <button
          type="button"
          className="btn btn-secondary"
          disabled={disabled}
          onClick={() => onChange([...blocks, createEmptyContentBlockDraft()])}
        >
          {t('seasonContent.add')}
        </button>
      </div>

      {blocks.length === 0 ? (
        <p className="season-content-blocks-editor__empty">{t('seasonContent.empty')}</p>
      ) : (
        <ol className="season-content-blocks-editor__list">
          {blocks.map((block, index) => (
            <li key={block.clientId} className="season-content-blocks-editor__item">
              <div className="season-content-blocks-editor__toolbar">
                <span className="season-content-blocks-editor__index">{index + 1}</span>
                <button
                  type="button"
                  className="btn btn-secondary btn-sm"
                  disabled={disabled || index === 0}
                  onClick={() => moveBlock(index, -1)}
                >
                  {t('seasonContent.moveUp')}
                </button>
                <button
                  type="button"
                  className="btn btn-secondary btn-sm"
                  disabled={disabled || index === blocks.length - 1}
                  onClick={() => moveBlock(index, 1)}
                >
                  {t('seasonContent.moveDown')}
                </button>
                <button
                  type="button"
                  className="btn btn-danger btn-sm"
                  disabled={disabled}
                  onClick={() => onChange(blocks.filter((item) => item.clientId !== block.clientId))}
                >
                  {t('seasonContent.remove')}
                </button>
              </div>
              <div className="form-group">
                <label htmlFor={`season-block-title-${block.clientId}`}>
                  {t('seasonContent.blockTitle')} *
                </label>
                <input
                  id={`season-block-title-${block.clientId}`}
                  type="text"
                  value={block.title}
                  maxLength={200}
                  disabled={disabled}
                  onChange={(event) => updateBlock(block.clientId, { title: event.target.value })}
                />
              </div>
              <div className="form-group">
                <label htmlFor={`season-block-content-${block.clientId}`}>
                  {t('seasonContent.blockContent')}
                </label>
                <RichTextEditor
                  id={`season-block-content-${block.clientId}`}
                  value={block.contentHtml}
                  onChange={(html) => updateBlock(block.clientId, { contentHtml: html })}
                  variant="compact"
                  readOnly={disabled}
                />
              </div>
            </li>
          ))}
        </ol>
      )}
    </div>
  );
}
