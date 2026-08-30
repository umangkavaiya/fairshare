import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { v4 as uuidv4 } from 'uuid'; // npm install uuid @types/uuid
import { environment } from '../../../../environments/environment';
import { CreateExpensePayload, Expense } from '../../../core/models/expense.model';
import { PagedResult } from '../../../core/models/group.model';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private readonly baseUrl = `${environment.apiUrl}/expenses`;

  constructor(private http: HttpClient) {}

  getExpenses(groupId: string, page = 1, pageSize = 20): Observable<PagedResult<Expense>> {
    return this.http.get<PagedResult<Expense>>(
      `${this.baseUrl}?groupId=${groupId}&page=${page}&pageSize=${pageSize}`,
    );
  }

  getExpense(expenseId: string): Observable<Expense> {
    return this.http.get<Expense>(`${this.baseUrl}/${expenseId}`);
  }

  createExpense(payload: CreateExpensePayload): Observable<Expense> {
    // A fresh idempotency key per logical submission — generated once per form
    // session, not per HTTP call, so a retry after a network failure reuses it.
    const idempotencyKey = uuidv4();
    return this.http.post<Expense>(this.baseUrl, payload, {
      headers: { 'Idempotency-Key': idempotencyKey },
    });
  }

  patchExpense(
    expenseId: string,
    rowVersion: string,
    changes: Partial<{ description: string; categoryId: number; expenseDate: string }>,
  ): Observable<Expense> {
    return this.http.patch<Expense>(`${this.baseUrl}/${expenseId}`, { ...changes, rowVersion });
  }

  deleteExpense(expenseId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${expenseId}`);
  }
}
