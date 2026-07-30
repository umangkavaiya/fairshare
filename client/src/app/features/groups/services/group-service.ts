import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  Group,
  GroupMember,
  PagedResult,
  UserSearchResult,
} from '../../../core/models/group.model';

@Injectable({ providedIn: 'root' })
export class GroupService {
  private readonly baseUrl = `${environment.apiUrl}/groups`;

  constructor(private http: HttpClient) {}

  getGroups(page = 1, pageSize = 20): Observable<PagedResult<Group>> {
    return this.http.get<PagedResult<Group>>(`${this.baseUrl}?page=${page}&pageSize=${pageSize}`);
  }

  getGroup(groupId: string): Observable<Group> {
    return this.http.get<Group>(`${this.baseUrl}/${groupId}`);
  }

  createGroup(payload: {
    name: string;
    description?: string;
    defaultCurrency: string;
  }): Observable<Group> {
    return this.http.post<Group>(this.baseUrl, payload);
  }

  patchGroup(
    groupId: string,
    payload: Partial<{ name: string; description: string; defaultCurrency: string }>,
  ): Observable<Group> {
    return this.http.patch<Group>(`${this.baseUrl}/${groupId}`, payload);
  }

  archiveGroup(groupId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${groupId}`);
  }

  getMembers(groupId: string): Observable<GroupMember[]> {
    return this.http.get<GroupMember[]>(`${this.baseUrl}/${groupId}/members`);
  }

  inviteMember(groupId: string, email: string): Observable<GroupMember> {
    return this.http.post<GroupMember>(`${this.baseUrl}/${groupId}/members`, { email });
  }

  removeMember(groupId: string, memberUserId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${groupId}/members/${memberUserId}`);
  }

  leaveGroup(groupId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${groupId}/leave`, {});
  }

  searchUsers(groupId: string, query: string): Observable<UserSearchResult[]> {
    return this.http.get<UserSearchResult[]>(
      `${this.baseUrl}/${groupId}/members/search?query=${encodeURIComponent(query)}`,
    );
  }
}
