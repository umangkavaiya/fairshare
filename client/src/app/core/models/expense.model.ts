export interface ExpenseSplit {
  userId: string;
  displayName: string;
  amountOwed: number;
  percentage: number | null;
}

export interface Expense {
  id: string;
  groupId: string | null;
  description: string;
  amount: number;
  currencyCode: string;
  categoryId: number | null;
  categoryName: string | null;
  splitType: 'Equal' | 'Exact' | 'Percentage';
  expenseDate: string;
  paidByUserId: string;
  paidByDisplayName: string;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string | null;
  rowVersion: string;
  splits: ExpenseSplit[];
}

export interface ExpenseParticipantInput {
  userId: string;
  amount?: number;
  percentage?: number;
}

export interface CreateExpensePayload {
  groupId?: string;
  paidByUserId: string;
  description: string;
  amount: number;
  currencyCode: string;
  categoryId?: number;
  splitType: string;
  expenseDate: string;
  participants: ExpenseParticipantInput[];
}
