export type AdSlot = 'LeftTop' | 'LeftBottom' | 'RightTop' | 'RightBottom' | 'BottomCenter';

export const AD_SLOTS: AdSlot[] = ['LeftTop', 'LeftBottom', 'RightTop', 'RightBottom', 'BottomCenter'];

/** Side-rail slots shown beside the page content on wide screens. */
export const SIDE_AD_SLOTS: AdSlot[] = ['LeftTop', 'LeftBottom', 'RightTop', 'RightBottom'];

export interface PublicAdvertisement {
  id: string;
  name: string;
  imageUrl: string;
  linkUrl: string | null;
  altText: string | null;
  slot: AdSlot;
  sortOrder: number;
}

export interface Advertisement extends PublicAdvertisement {
  isActive: boolean;
  startsAt: string | null;
  endsAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface AdvertisementRequest {
  name: string;
  imageUrl: string;
  linkUrl: string | null;
  altText: string | null;
  slot: AdSlot;
  isActive: boolean;
  sortOrder: number;
  startsAt: string | null;
  endsAt: string | null;
}
