import { Component, inject, signal, computed, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Admin, UserListItem } from '../../../services/admin';
import { Roles } from '../../../services/role-management';
import { debounceTime, Subject } from 'rxjs';
import { PAGINATION } from '../../../constants/pagination';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PersonItem } from '../person-management/person-management';
import { ColumnReorder } from '../../../components/column-reorder/column-reorder';
import { exportToCsv, ExportColumn } from '../../../shared/csv-export';
import { loadColumnPreferences, saveColumnPreferences } from '../../../shared/column-preferences';
import { MatDialog } from '@angular/material/dialog';
import { RoleHistoryDialog } from '../../../components/role-history-dialog/role-history-dialog';

@Component({
  selector: 'app-user-management',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatDividerModule,
    TranslocoModule,
    ColumnReorder
  ],
  templateUrl: './user-management.html',
  styleUrl: './user-management.scss',
})
export class UserManagement implements OnInit {
  private adminService = inject(Admin);
  private rolesService = inject(Roles);
  private fb = inject(FormBuilder);
  private transloco = inject(TranslocoService);
  private http = inject(HttpClient);
  private dialog = inject(MatDialog);
  private router = inject(Router);

  users = signal<UserListItem[]>([]);
  totalCount = signal(0);

  private readonly tableKey = 'users';
  private readonly baseColumns = ['userName', 'email', 'roles', 'status', 'personLinked', 'createdAt'];
  columnOrder = signal<string[]>([...this.baseColumns]);
  hiddenColumns = signal<Set<string>>(new Set());
  displayedColumns = computed(() => [...this.columnOrder().filter(c => !this.hiddenColumns().has(c)), 'actions']);
  showColumnMenu = signal(false);

  availableRoles = signal<string[]>([]);

  pageIndex = 0;
  pageSize = PAGINATION.defaultPageSize;
  pageSizeOptions = PAGINATION.pageSizeOptions;
  sortActive = 'userName';
  sortDirection: 'asc' | 'desc' | '' = 'asc';

  searchControl = this.fb.control('');
  roleFilterControl = this.fb.control('');
  statusFilterControl = this.fb.control<boolean | null>(null);

  private searchSubject = new Subject<string>();

  showCreateForm = signal(false);
  createdUserPassword = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  showPersonSearch = signal(false);
  linkingUserId = signal<string | null>(null);
  persons = signal<PersonItem[]>([]);
  personSearch = this.fb.control('');
  private personSearchSubject = new Subject<string>();

  createForm = this.fb.group({
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    role: ['', Validators.required]
  });

  ngOnInit(): void {
    this.restoreColumnPreferences();

    this.searchSubject.pipe(debounceTime(400)).subscribe(() => {
      this.pageIndex = 0;
      this.loadUsers();
    });

    this.searchControl.valueChanges.subscribe(val => this.searchSubject.next(val || ''));
    this.roleFilterControl.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.loadUsers();
    });
    this.statusFilterControl.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.loadUsers();
    });

    this.loadUsers();
    this.loadAvailableRoles();

    this.personSearchSubject.pipe(debounceTime(400)).subscribe(() => {
      this.searchPersons();
    });
    this.personSearch.valueChanges.subscribe(val => 
      this.personSearchSubject.next(val || ''));
  }

  private restoreColumnPreferences(): void {
    const saved = loadColumnPreferences(this.tableKey);
    if (!saved) return;

    const validOrder = saved.order.filter(c => this.baseColumns.includes(c));
    const missing = this.baseColumns.filter(c => !validOrder.includes(c));
    this.columnOrder.set([...validOrder, ...missing]);
    this.hiddenColumns.set(new Set(saved.hidden.filter(c => this.baseColumns.includes(c))));
  }

  private persistColumnPreferences(): void {
    saveColumnPreferences(this.tableKey, {
      order: this.columnOrder(),
      hidden: Array.from(this.hiddenColumns())
    });
  }

  onColumnsReordered(newOrder: string[]): void {
    this.columnOrder.set(newOrder);
    this.persistColumnPreferences();
  }

  onColumnVisibilityToggled(key: string): void {
    const updated = new Set(this.hiddenColumns());
    if (updated.has(key)) updated.delete(key); else updated.add(key);
    this.hiddenColumns.set(updated);
    this.persistColumnPreferences();
  }

  reorderableColumns = computed(() => {
    const defs = this.columnDefs();
    const hidden = this.hiddenColumns();
    return this.columnOrder().map(col => ({
      key: col,
      label: defs[col]?.header ?? col,
      hidden: hidden.has(col)
    }));
  });

  exportUsers(): void {
    exportToCsv('users.csv', this.getExportColumns(), this.users());
  }

  private columnDefs(): Record<string, ExportColumn<UserListItem>> {
    return {
      userName: {
        key: 'userName',
        header: this.transloco.translate('admin.users.name'),
        getValue: (u) => u.userName
      },
      email: {
        key: 'email',
        header: this.transloco.translate('admin.users.email'),
        getValue: (u) => u.email
      },
      roles: {
        key: 'roles',
        header: this.transloco.translate('admin.users.roles'),
        getValue: (u) => u.roles.join('; ')
      },
      status: {
        key: 'status',
        header: this.transloco.translate('admin.users.status'),
        getValue: (u) => u.isActive
          ? this.transloco.translate('admin.users.activeOnly')
          : this.transloco.translate('admin.users.inactiveOnly')
      },
      personLinked: {
        key: 'personLinked',
        header: this.transloco.translate('admin.users.personLinked'),
        getValue: (u) => u.personId
          ? this.transloco.translate('admin.users.linked')
          : this.transloco.translate('admin.users.notLinked')
      },
      createdAt: {
        key: 'createdAt',
        header: this.transloco.translate('admin.users.createdAt'),
        getValue: (u) => u.createdAt
      }
    };
  }

  private getExportColumns(): ExportColumn<UserListItem>[] {
    const defs = this.columnDefs();
    return this.displayedColumns()
      .filter(col => col !== 'actions')
      .map(col => defs[col])
      .filter((col): col is ExportColumn<UserListItem> => !!col);
  }

  loadUsers(): void {
    this.adminService.getUsers({
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortActive,
      sortDirection: this.sortDirection || 'asc',
      search: this.searchControl.value || undefined,
      roleFilter: this.roleFilterControl.value || undefined,
      statusFilter: this.statusFilterControl.value ?? undefined
    }).subscribe({
      next: (data) => {
        this.users.set(data.items);
        this.totalCount.set(data.totalCount);
      },
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.loadError'))
    });
  }

  loadAvailableRoles(): void {
    this.rolesService.getRoles().subscribe({
      next: (data) => this.availableRoles.set(data.map(r => r.name)),
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.loadError'))
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  onSortChange(sort: Sort): void {
    this.sortActive = sort.active;
    this.sortDirection = sort.direction;
    this.pageIndex = 0;
    this.loadUsers();
  }

  toggleCreateForm(): void {
    this.showCreateForm.set(!this.showCreateForm());
    this.createdUserPassword.set(null);
    this.createForm.reset({ role: '' });
  }

  createUser(): void {
    if (this.createForm.invalid) return;

    const { userName, email, password, role } = this.createForm.value;

    this.adminService.createUser({
      userName: userName!,
      email: email!,
      role: role!,
      password: password || undefined
    }).subscribe({
      next: (res) => {
        this.createdUserPassword.set(res.temporaryPassword);
        this.loadUsers();
        this.createForm.reset({ role: '' });
      },
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.saveError'))
    });
  }

  // All role editing — add, remove, and setting a start/expiry window — happens on the dedicated
  // per-user page. This keeps one code path for the user_roles audit trail instead of a
  // dateless quick-add here plus a dated editor there.
  manageRoles(user: UserListItem): void {
    this.router.navigate(['/admin/users', user.id, 'roles']);
  }

  toggleActive(user: UserListItem): void {
    const action = user.isActive
      ? this.adminService.deactivateUser(user.id)
      : this.adminService.activateUser(user.id);

    action.subscribe({
      next: () => this.loadUsers(),
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.saveError'))
    });
  }

  resetPassword(userId: string): void {
    this.adminService.resetPassword(userId).subscribe({
      next: (res) => {
        this.createdUserPassword.set(res.temporaryPassword);
        this.loadUsers();
      },
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.saveError'))
    });
  }

  deleteUser(userId: string): void {
    if (!confirm(this.transloco.translate('admin.users.confirmDelete'))) return;
    this.adminService.deleteUser(userId).subscribe({
      next: () => this.loadUsers(),
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.deleteError'))
    });
  }

  openPersonSearch(userId: string): void {
    this.linkingUserId.set(userId);
    this.showPersonSearch.set(true);
    this.searchPersons();
  }

  searchPersons(): void {
    let params = new HttpParams()
      .set('page', 1)
      .set('pageSize', 10);
    if (this.personSearch.value)
      params = params.set('search', this.personSearch.value);

    this.http.get<any>(`${environment.apiUrl}/person`, { params }).subscribe({
      next: (data) => this.persons.set(data.items)
    });
  }

  linkPerson(personId: number): void {
    const userId = this.linkingUserId();
    if (!userId) return;

    this.adminService.linkPerson(userId, personId).subscribe({
      next: () => {
        this.showPersonSearch.set(false);
        this.linkingUserId.set(null);
        this.loadUsers();
      },
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.saveError'))
    });
  }

  unlinkPerson(userId: string): void {
    this.adminService.unlinkPerson(userId).subscribe({
      next: () => this.loadUsers(),
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.saveError'))
    });
  }

  openRoleHistory(user: UserListItem): void {
    this.adminService.getRoleHistory(user.id).subscribe({
      next: (history) => {
        this.dialog.open(RoleHistoryDialog, {
          data: { userName: user.userName, history },
          width: '600px'
        });
      },
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.loadError'))
    });
  }
}