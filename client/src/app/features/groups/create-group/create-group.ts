import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GroupService } from '../services/group-service';

@Component({
  selector: 'app-create-group',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './create-group.html',
})
export class CreateGroup {
  name = '';
  description = '';
  defaultCurrency = 'USD';
  errorMessage = '';

  constructor(
    private groupService: GroupService,
    private router: Router,
  ) {}

  onSubmit() {
    this.errorMessage = '';
    this.groupService
      .createGroup({
        name: this.name,
        description: this.description || undefined,
        defaultCurrency: this.defaultCurrency,
      })
      .subscribe({
        next: (group) => this.router.navigate(['/groups', group.id]),
        error: (err) => (this.errorMessage = err.error?.message ?? 'Failed to create group.'),
      });
  }
}
