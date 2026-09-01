import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import { environment } from '../../../../environments/environment';
import { Balance, Settlement, SettlementSuggestion } from '../../../core/models/settlement.model';
import { PagedResult } from '../../../core/models/group.model';

@Injectable({ providedIn: 'root' })
export class SettlementService {
  private base(groupId: string) {
    return `${environment.apiUrl}/groups/${groupId}`;
  }

  constructor(private http: HttpClient) {}

  getBalances(groupId: string): Observable<Balance[]> {
    return this.http.get<Balance[]>(`${this.base(groupId)}/balances`);
  }

  getSuggestions(groupId: string): Observable<SettlementSuggestion[]> {
    return this.http.get<SettlementSuggestion[]>(`${this.base(groupId)}/settlements/suggestions`);
  }

  getHistory(groupId: string, page = 1): Observable<PagedResult<Settlement>> {
    return this.http.get<PagedResult<Settlement>>(`${this.base(groupId)}/settlements?page=${page}`);
  }

  recordSettlement(
    groupId: string,
    payload: {
      payerUserId: string;
      payeeUserId: string;
      amount: number;
      currencyCode: string;
      settledAt: string;
      note?: string;
    },
  ): Observable<Settlement> {
    return this.http.post<Settlement>(`${this.base(groupId)}/settlements`, payload, {
      headers: { 'Idempotency-Key': uuidv4() },
    });
  }

  confirmSettlement(groupId: string, settlementId: string): Observable<Settlement> {
    return this.http.post<Settlement>(
      `${this.base(groupId)}/settlements/${settlementId}/confirm`,
      {},
    );
  }
}
