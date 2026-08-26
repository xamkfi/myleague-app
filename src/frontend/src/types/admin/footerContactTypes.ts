export interface FooterContact {
  id: string;
  title: string;
  details: string | null;
  email: string | null;
  phone: string | null;
  url: string | null;
  sortOrder: number;
  lastModifiedBy: string | null;
  updatedAt: string;
}

export interface FooterContactRequest {
  title: string;
  details: string | null;
  email: string | null;
  phone: string | null;
  url: string | null;
  sortOrder: number;
}
