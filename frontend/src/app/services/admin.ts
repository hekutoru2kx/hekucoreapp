import { Service, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserListItem {
  id: string;
  userName: string;
  email: string;
  roles: string[];
  isActive: boolean;
  mustChangePassword: boolean;
  createdAt: string;
  personId?: number | null;
}

export interface PagedUserResult {
  items: UserListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UserListQueryParams {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: string;
  search?: string;
  roleFilter?: string;
  statusFilter?: boolean;
}

export interface CreateUserRequest {
  userName: string;
  email: string;
  password?: string;
  role: string;
}

export interface CreateUserResponse {
  userId: string;
  email: string;
  userName: string;
  temporaryPassword: string;
}

export interface UserRoleHistoryItem {
  roleName: string;
  createdAt: string;
  createdBy: string;
  revokedAt: string | null;
  revokedBy: string | null;
}

// A currently-open role assignment as returned by GET users/:id/roles — for editing on the
// role-assignment page. `isPending`/`isExpired` are derived server-side from the window.
export interface RoleAssignment {
  roleName: string;
  startsAt?: string | null;
  expiresAt?: string | null;
  isPending: boolean;
  isExpired: boolean;
  createdAt: string;
  createdByName: string;
  updatedAt: string;
  updatedByName: string;
}

// One desired assignment sent to PUT users/:id/roles. Null dates mean "effective now" /
// "never expires".
export interface RoleAssignmentInput {
  roleName: string;
  startsAt?: string | null;
  expiresAt?: string | null;
}

@Service()
export class Admin {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/admin`;

  getUsers(query: UserListQueryParams): Observable<PagedUserResult> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);
    if (query.search) params = params.set('search', query.search);
    if (query.roleFilter) params = params.set('roleFilter', query.roleFilter);
    if (query.statusFilter !== undefined && query.statusFilter !== null) {
      params = params.set('statusFilter', query.statusFilter);
    }

    return this.http.get<PagedUserResult>(`${this.apiUrl}/users`, { params });
  }

  createUser(data: CreateUserRequest): Observable<CreateUserResponse> {
    return this.http.post<CreateUserResponse>(`${this.apiUrl}/users`, data);
  }

  getUser(userId: string): Observable<UserListItem> {
    return this.http.get<UserListItem>(`${this.apiUrl}/users/${userId}`);
  }

  getRoleAssignments(userId: string): Observable<RoleAssignment[]> {
    return this.http.get<RoleAssignment[]>(`${this.apiUrl}/users/${userId}/roles`);
  }

  assignRoles(userId: string, roles: RoleAssignmentInput[]): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${userId}/roles`, { roles });
  }

  getRoleHistory(userId: string): Observable<UserRoleHistoryItem[]> {
    return this.http.get<UserRoleHistoryItem[]>(`${this.apiUrl}/users/${userId}/roles/history`);
  }

  deactivateUser(userId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${userId}/deactivate`, {});
  }

  activateUser(userId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${userId}/activate`, {});
  }

  resetPassword(userId: string, newPassword?: string): Observable<{ temporaryPassword: string }> {
    return this.http.put<{ temporaryPassword: string }>(`${this.apiUrl}/users/${userId}/reset-password`, { newPassword });
  }

  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${userId}`);
  }

  // Add to Admin service:
  linkPerson(userId: string, personId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${userId}/link-person/${personId}`, {});
  }

  unlinkPerson(userId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${userId}/unlink-person`, {});
  }
}