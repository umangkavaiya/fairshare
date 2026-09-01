import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Balance, Settlement, SettlementSuggestion } from '../../../core/models/settlement.model';
import { SettlementService } from '../services/settlement-service';
import { AuthService } from '../../../core/services/auth-service';

@Component({
  selector: 'app-balances-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './balances-view.html',
})
export class BalancesView implements OnInit {
  groupId!: string;
  balances = signal<Balance[]>([]);
  suggestions = signal<SettlementSuggestion[]>([]);
  history = signal<Settlement[]>([]);
  currentUserId = '';

  constructor(
    private route: ActivatedRoute,
    private settlementService: SettlementService,
    private authService: AuthService,
  ) {}

  ngOnInit() {
    this.groupId = this.route.snapshot.paramMap.get('groupId')!;
    this.currentUserId = this.authService.currentUser()?.id ?? '';
    this.load();
  }

  load() {
    this.settlementService.getBalances(this.groupId).subscribe((b) => this.balances.set(b));
    this.settlementService.getSuggestions(this.groupId).subscribe((s) => this.suggestions.set(s));
    this.settlementService.getHistory(this.groupId).subscribe((res) => this.history.set(res.items));
  }

  recordSuggested(s: SettlementSuggestion) {
    this.settlementService
      .recordSettlement(this.groupId, {
        payerUserId: s.fromUserId,
        payeeUserId: s.toUserId,
        amount: s.amount,
        currencyCode: 'USD',
        settledAt: new Date().toISOString(),
      })
      .subscribe(() => this.load());
  }

  confirm(settlementId: string) {
    this.settlementService
      .confirmSettlement(this.groupId, settlementId)
      .subscribe(() => this.load());
  }

  // A settlement needs confirmation from the current user specifically when
  // they're the counterparty to whoever created it, and it's still Pending.
  needsMyConfirmation(s: Settlement): boolean {
    const counterparty = s.createdByUserId === s.payerUserId ? s.payeeUserId : s.payerUserId;
    return s.status === 'Pending' && counterparty === this.currentUserId;
  }
}
