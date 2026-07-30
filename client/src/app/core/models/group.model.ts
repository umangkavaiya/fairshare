export interface Group {
  id: string;
  name: string;
  description: string | null;
  defaultCurrency: string;
  createdByUserId: string;
  createdAt: string;
  memberCount: number;
  currentUserRole: 'Member' | 'Admin';
}

export interface GroupMember {
  userId: string;
  displayName: string;
  email: string;
  role: 'Member' | 'Admin';
  joinedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserSearchResult {
  id: string;
  displayName: string;
  email: string;
}
