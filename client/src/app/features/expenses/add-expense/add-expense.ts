import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GroupMember } from '../../../core/models/group.model';
import { GroupService } from '../../groups/services/group-service';
import { ExpenseService } from '../services/expense-service';
import { AuthService } from '../../../core/services/auth-service';

@Component({
  selector: 'app-add-expense',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-expense.html',
})
export class AddExpense implements OnInit {
  groupId!: string;
  members = signal<GroupMember[]>([]);
  description = '';
  amount: number | null = null;
  splitType: 'Equal' | 'Exact' | 'Percentage' = 'Equal';
  expenseDate = new Date().toISOString().slice(0, 10);
  selectedUserIds = new Set<string>();
  exactAmounts: Record<string, number> = {};
  percentages: Record<string, number> = {};
  errorMessage = '';

  // Live reconciliation feedback so the user sees the mismatch before submitting
  exactTotal = computed(() => Object.values(this.exactAmounts).reduce((a, b) => a + (b || 0), 0));
  percentageTotal = computed(() =>
    Object.values(this.percentages).reduce((a, b) => a + (b || 0), 0),
  );

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private groupService: GroupService,
    private expenseService: ExpenseService,
    private authService: AuthService,
  ) {}

  ngOnInit() {
    this.groupId = this.route.snapshot.paramMap.get('groupId')!;
    this.groupService.getMembers(this.groupId).subscribe((members) => {
      this.members.set(members);
      members.forEach((m) => this.selectedUserIds.add(m.userId)); // default: everyone included
    });
  }

  toggleParticipant(userId: string) {
    this.selectedUserIds.has(userId)
      ? this.selectedUserIds.delete(userId)
      : this.selectedUserIds.add(userId);
  }

  onSubmit() {
    this.errorMessage = '';
    if (!this.amount || this.amount <= 0) {
      this.errorMessage = 'Enter a valid amount.';
      return;
    }

    const currentUser = this.authService.currentUser();
    if (!currentUser) return;

    const participants = Array.from(this.selectedUserIds).map((userId) => ({
      userId,
      amount: this.splitType === 'Exact' ? this.exactAmounts[userId] : undefined,
      percentage: this.splitType === 'Percentage' ? this.percentages[userId] : undefined,
    }));

    this.expenseService
      .createExpense({
        groupId: this.groupId,
        paidByUserId: currentUser.id,
        description: this.description,
        amount: this.amount,
        currencyCode: 'USD',
        splitType: this.splitType,
        expenseDate: this.expenseDate,
        participants,
      })
      .subscribe({
        next: () => this.router.navigate(['/groups', this.groupId]),
        error: (err) => (this.errorMessage = err.error?.message ?? 'Failed to create expense.'),
      });
  }
}
