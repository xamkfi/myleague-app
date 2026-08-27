import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import RichTextEditor from '../../../../components/RichTextEditor/RichTextEditor';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { seasonContentBlockService } from '../../../../api/common/seasonContentBlockService';
import type { SportsCategory } from '../../../../types/common/sports';
import type { SeasonContentBlockDto } from '../../../../types/admin/seasonContentBlockTypes';
import './SeasonContentBlocksTab.scss';

interface SeasonContentBlocksTabProps {
  sport: SportsCategory;
  competitionId: string;
  seasonYear: string;
  onSuccess?: (message: string) => void;
}

type EditorMode = 'list' | 'create' | 'edit';

export default function SeasonContentBlocksTab({
  sport,
  competitionId,
  seasonYear,
  onSuccess,
}: Readonly<SeasonContentBlocksTabProps>) {
  const { t } = useTranslation();
  const [blocks, setBlocks] = useState<SeasonContentBlockDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [mode, setMode] = useState<EditorMode>('list');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [title, setTitle] = useState('');
  const [contentHtml, setContentHtml] = useState('');

  const loadBlocks = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      setError(null);
      const data = await seasonContentBlockService.getByCompetition(competitionId);
      setBlocks([...data].sort((a, b) => a.sortOrder - b.sortOrder));
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('admin.seasonContentBlocks.loadError', 'Sisältöblokkien lataus epäonnistui'),
      );
    } finally {
      setLoading(false);
    }
  }, [competitionId, t]);

  useEffect(() => {
    void loadBlocks();
  }, [loadBlocks]);

  const resetEditor = (): void => {
    setMode('list');
    setEditingId(null);
    setTitle('');
    setContentHtml('');
  };

  const openCreate = (): void => {
    setMode('create');
    setEditingId(null);
    setTitle('');
    setContentHtml('');
    setError(null);
  };

  const openEdit = (block: SeasonContentBlockDto): void => {
    setMode('edit');
    setEditingId(block.id);
    setTitle(block.title);
    setContentHtml(block.contentHtml);
    setError(null);
  };

  const handleSave = async (): Promise<void> => {
    if (!title.trim()) {
      setError(t('admin.seasonContentBlocks.titleRequired', 'Otsikko on pakollinen'));
      return;
    }

    try {
      setSaving(true);
      setError(null);

      if (mode === 'create') {
        await seasonContentBlockService.create({
          sport,
          competitionId,
          seasonYear,
          title: title.trim(),
          contentHtml,
          sortOrder: blocks.length,
        });
        onSuccess?.(t('admin.seasonContentBlocks.created', 'Sisältöblokki luotu'));
      } else if (editingId) {
        const current = blocks.find((block) => block.id === editingId);
        await seasonContentBlockService.update(editingId, {
          title: title.trim(),
          contentHtml,
          sortOrder: current?.sortOrder ?? 0,
        });
        onSuccess?.(t('admin.seasonContentBlocks.saved', 'Sisältöblokki tallennettu'));
      }

      resetEditor();
      await loadBlocks();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('admin.seasonContentBlocks.saveFailed', 'Tallennus epäonnistui'),
      );
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (block: SeasonContentBlockDto): Promise<void> => {
    const confirmed = window.confirm(
      t('admin.seasonContentBlocks.deleteConfirm', 'Poistetaanko tämä sisältöblokki?'),
    );
    if (!confirmed) {
      return;
    }

    try {
      setSaving(true);
      setError(null);
      await seasonContentBlockService.delete(block.id);
      onSuccess?.(t('admin.seasonContentBlocks.deleted', 'Sisältöblokki poistettu'));
      await loadBlocks();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('admin.seasonContentBlocks.deleteFailed', 'Poisto epäonnistui'),
      );
    } finally {
      setSaving(false);
    }
  };

  const moveBlock = async (index: number, direction: -1 | 1): Promise<void> => {
    const targetIndex = index + direction;
    if (targetIndex < 0 || targetIndex >= blocks.length) {
      return;
    }

    const next = [...blocks];
    const [moved] = next.splice(index, 1);
    next.splice(targetIndex, 0, moved);

    try {
      setSaving(true);
      setError(null);
      const updated = await seasonContentBlockService.reorder(next.map((block) => block.id));
      setBlocks([...updated].sort((a, b) => a.sortOrder - b.sortOrder));
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('admin.seasonContentBlocks.reorderFailed', 'Järjestyksen vaihto epäonnistui'),
      );
    } finally {
      setSaving(false);
    }
  };

  if (mode !== 'list') {
    return (
      <div className="season-content-blocks">
        <ErrorPopup message={error} />
        <div className="season-content-blocks__form">
          <div className="form-group">
            <label htmlFor="season-content-block-title">
              {t('admin.seasonContentBlocks.fields.title', 'Otsikko')} *
            </label>
            <input
              id="season-content-block-title"
              type="text"
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              disabled={saving}
              maxLength={200}
            />
          </div>
          <div className="form-group">
            <span className="season-content-blocks__label">
              {t('admin.seasonContentBlocks.fields.content', 'Sisältö')}
            </span>
            <RichTextEditor
              value={contentHtml}
              onChange={setContentHtml}
              showMatchInsert={false}
              variant="compact"
            />
          </div>
          <div className="season-content-blocks__actions">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={resetEditor}
              disabled={saving}
            >
              {t('common.cancel', 'Peruuta')}
            </button>
            <button
              type="button"
              className="btn btn-primary"
              onClick={() => void handleSave()}
              disabled={saving}
            >
              {saving
                ? t('common.saving', 'Tallennetaan...')
                : t('common.save', 'Tallenna')}
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="season-content-blocks">
      <ErrorPopup message={error} />
      <div className="season-content-blocks__header">
        <h3>{t('admin.seasonContentBlocks.title', 'Kauden sisältöblokit')}</h3>
        <button
          type="button"
          className="btn btn-primary"
          onClick={openCreate}
          disabled={saving || loading}
        >
          {t('admin.seasonContentBlocks.add', 'Lisää sisältöblokki')}
        </button>
      </div>

      {loading ? (
        <p>{t('common.loading', 'Ladataan...')}</p>
      ) : blocks.length === 0 ? (
        <p className="season-content-blocks__empty">
          {t('admin.seasonContentBlocks.empty', 'Tälle kaudelle ei ole vielä sisältöblokkeja.')}
        </p>
      ) : (
        <ul className="season-content-blocks__list">
          {blocks.map((block, index) => (
            <li key={block.id} className="season-content-blocks__item">
              <div>
                <strong>{block.title}</strong>
                <span>
                  {t('admin.seasonContentBlocks.sortOrder', 'Järjestys')} {block.sortOrder + 1}
                </span>
              </div>
              <div className="season-content-blocks__item-actions">
                <button
                  type="button"
                  onClick={() => void moveBlock(index, -1)}
                  disabled={saving || index === 0}
                  aria-label={t('admin.seasonContentBlocks.moveUp', 'Siirrä ylös')}
                >
                  ↑
                </button>
                <button
                  type="button"
                  onClick={() => void moveBlock(index, 1)}
                  disabled={saving || index === blocks.length - 1}
                  aria-label={t('admin.seasonContentBlocks.moveDown', 'Siirrä alas')}
                >
                  ↓
                </button>
                <button type="button" onClick={() => openEdit(block)} disabled={saving}>
                  {t('common.edit', 'Muokkaa')}
                </button>
                <button
                  type="button"
                  className="season-content-blocks__delete"
                  onClick={() => void handleDelete(block)}
                  disabled={saving}
                >
                  {t('common.delete', 'Poista')}
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
