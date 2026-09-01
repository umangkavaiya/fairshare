export interface Balance {
  userId: string;
  displayName: string;
  netBalance: number;
}

export interface SettlementSuggestion {
  fromUserId: string;
  fromDisplayName: string;
  toUserId: string;
  toDisplayName: string;
  amount: number;
}

export interface Settlement {
  id: string;
  groupId: string;
  payerUserId: string;
  payerDisplayName: string;
  payeeUserId: string;
  payeeDisplayName: string;
  amount: number;
  currencyCode: string;
  note: string | null;
  status: 'Pending' | 'Confirmed';
  createdByUserId: string;
  settledAt: string;
  createdAt: string;
  confirmedAt: string | null;
}
