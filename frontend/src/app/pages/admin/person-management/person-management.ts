import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Auth } from '../../../services/auth';
import { environment } from '../../../../environments/environment';
import { debounceTime, Subject } from 'rxjs';
import { PAGINATION } from '../../../constants/pagination';
import { PersonForm, PersonFormData } from '../../../components/person-form/person-form';

export interface PersonItem {
  id: number;
  firstName: string;
  lastName: string;
  birthday?: string;
  documentType?: string;
  documentId?: string;
  phone?: string;
  phoneExtension?: string;
  email?: string;
  address?: string;
  postalCode?: string;
  gender?: string;
  countryId?: number;
  stateId?: number;
  cityId?: number;
  countryName?: string;
  stateName?: string;
  cityName?: string;
  linkedUserName?: string | null;
}

export interface PagedPersonResult {
  items: PersonItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Component({
  selector: 'app-person-management',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatTooltipModule,
    MatProgressBarModule,
    TranslocoModule,
    PersonForm
  ],
  templateUrl: './person-management.html',
  styleUrl: './person-management.scss',
})
export class PersonManagement implements OnInit {
  private http = inject(HttpClient);
  protected auth = inject(Auth);
  private fb = inject(FormBuilder);
  private transloco = inject(TranslocoService);

  editingPersonFormData = signal<PersonFormData | null>(null);
  persons = signal<PersonItem[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  displayedColumns = ['lastName', 'firstName', 'email', 'document', 'phone', 'location', 'actions'];

  pageSize = PAGINATION.defaultPageSize;
  pageSizeOptions = PAGINATION.pageSizeOptions;
  pageIndex = 0;
  sortActive = 'lastName';
  sortDirection: 'asc' | 'desc' | '' = 'asc';

  searchControl = this.fb.control('');
  private searchSubject = new Subject<string>();

  showForm = signal(false);
  editingPerson = signal<PersonItem | null>(null);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.searchSubject.pipe(debounceTime(400)).subscribe(() => {
      this.pageIndex = 0;
      this.loadPersons();
    });

    this.searchControl.valueChanges.subscribe(val => this.searchSubject.next(val || ''));
    this.loadPersons();
  }

  loadPersons(): void {
    this.loading.set(true);
    let params = new HttpParams()
      .set('page', this.pageIndex + 1)
      .set('pageSize', this.pageSize)
      .set('sortBy', this.sortActive)
      .set('sortDirection', this.sortDirection || 'asc');

    if (this.searchControl.value)
      params = params.set('search', this.searchControl.value);

    this.http.get<PagedPersonResult>(`${environment.apiUrl}/person`, { params }).subscribe({
      next: (data) => {
        this.persons.set(data.items);
        this.totalCount.set(data.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error || this.transloco.translate('common.loadError'));
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadPersons();
  }

  onSortChange(sort: Sort): void {
    this.sortActive = sort.active;
    this.sortDirection = sort.direction;
    this.pageIndex = 0;
    this.loadPersons();
  }

  toggleForm(person?: PersonItem): void {
    this.editingPerson.set(person || null);
    this.editingPersonFormData.set(person ? this.toPersonFormData(person) : null);
    this.showForm.set(!this.showForm());
    this.errorMessage.set(null);
  }

  onPersonSaved(data: PersonFormData): void {
    const editing = this.editingPerson();
    const request = editing
      ? this.http.put(`${environment.apiUrl}/person/${editing.id}`, data)
      : this.http.post(`${environment.apiUrl}/person`, data);

    request.subscribe({
      next: () => {
        this.showForm.set(false);
        this.editingPerson.set(null);
        this.loadPersons();
      },
      error: (err) => this.errorMessage.set(err.error || this.transloco.translate('common.saveError'))
    });
  }

  onPersonCancelled(): void {
    this.showForm.set(false);
    this.editingPerson.set(null);
  }

  canCreatePerson(): boolean {
    return this.auth.hasClaim('PersonsPermission', 'Create');
  }

  canUpdatePerson(): boolean {
    return this.auth.hasClaim('PersonsPermission', 'Update');
  }

  toPersonFormData(person: PersonItem): PersonFormData {
    return {
      firstName: person.firstName,
      lastName: person.lastName,
      birthday: person.birthday ? new Date(person.birthday) : null,
      documentType: person.documentType,
      documentId: person.documentId,
      phone: person.phone,
      phoneExtension: person.phoneExtension,
      email: person.email,
      address: person.address,
      postalCode: person.postalCode,
      gender: person.gender,
      countryId: person.countryId,
      stateId: person.stateId,
      cityId: person.cityId
    };
  }
}