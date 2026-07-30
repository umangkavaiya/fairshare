import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { Group, GroupMember, UserSearchResult } from '../../../core/models/group.model';
import { GroupService } from '../services/group-service';

@Component({
  selector: 'app-group-detail',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './group-detail.html',
})
export class GroupDetail implements OnInit {
  groupId!: string;
  group = signal<Group | null>(null);
  members = signal<GroupMember[]>([]);
  searchControl = new FormControl('');
  searchResults = signal<UserSearchResult[]>([]);
  inviteError = '';

  constructor(
    private route: ActivatedRoute,
    private groupService: GroupService,
  ) {}

  ngOnInit() {
    this.groupId = this.route.snapshot.paramMap.get('groupId')!;
    this.loadGroup();
    this.loadMembers();

    // switchMap cancels any in-flight search the moment a newer keystroke
    // fires — a stale response can never overwrite a newer one.
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) =>
          !query || query.length < 2 ? of([]) : this.groupService.searchUsers(this.groupId, query),
        ),
      )
      .subscribe((results) => this.searchResults.set(results));
  }

  loadGroup() {
    this.groupService.getGroup(this.groupId).subscribe((g) => this.group.set(g));
  }
  loadMembers() {
    this.groupService.getMembers(this.groupId).subscribe((m) => this.members.set(m));
  }

  invite(user: UserSearchResult) {
    this.inviteError = '';
    this.groupService.inviteMember(this.groupId, user.email).subscribe({
      next: () => {
        this.searchControl.setValue('');
        this.searchResults.set([]);
        this.loadMembers();
        this.loadGroup();
      },
      error: (err) => (this.inviteError = err.error?.message ?? 'Failed to invite user.'),
    });
  }

  removeMember(userId: string) {
    this.groupService.removeMember(this.groupId, userId).subscribe(() => this.loadMembers());
  }
}
