import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Group, PagedResult } from '../../../core/models/group.model';
import { GroupService } from '../services/group-service';

@Component({
  selector: 'app-group-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './group-list.html',
})
export class GroupList implements OnInit {
  groups = signal<Group[]>([]);
  page = signal(1);
  totalPages = signal(1);
  loading = signal(true);

  constructor(private groupService: GroupService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.groupService.getGroups(this.page()).subscribe({
      next: (res: PagedResult<Group>) => {
        this.groups.set(res.items);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  nextPage() {
    if (this.page() < this.totalPages()) {
      this.page.set(this.page() + 1);
      this.load();
    }
  }
  prevPage() {
    if (this.page() > 1) {
      this.page.set(this.page() - 1);
      this.load();
    }
  }
}
