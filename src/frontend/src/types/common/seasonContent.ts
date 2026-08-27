export interface SeasonContentBlockDto {
  id: string;
  title: string;
  contentHtml: string;
  sortOrder: number;
}

export interface SeasonContentBlocksDto {
  seasonId: string | null;
  blocks: SeasonContentBlockDto[];
}

export interface SeasonContentBlockItem {
  id?: string;
  title: string;
  contentHtml: string;
}

export interface SeasonContentBlockDraft {
  clientId: string;
  id?: string;
  title: string;
  contentHtml: string;
}

export function toContentBlockDrafts(blocks: SeasonContentBlockDto[]): SeasonContentBlockDraft[] {
  return [...blocks]
    .sort((left, right) => left.sortOrder - right.sortOrder)
    .map((block) => ({
      clientId: block.id,
      id: block.id,
      title: block.title,
      contentHtml: block.contentHtml,
    }));
}

export function toContentBlockItems(drafts: SeasonContentBlockDraft[]): SeasonContentBlockItem[] {
  return drafts.map((draft) => ({
    id: draft.id,
    title: draft.title.trim(),
    contentHtml: draft.contentHtml,
  }));
}

export function createEmptyContentBlockDraft(): SeasonContentBlockDraft {
  return {
    clientId: crypto.randomUUID(),
    title: '',
    contentHtml: '',
  };
}
