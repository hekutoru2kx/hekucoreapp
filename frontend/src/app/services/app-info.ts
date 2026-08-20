import { Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { inject } from '@angular/core';
import { environment } from '../../environments/environment';

export interface AppInfoDto {
  id: number;
  appName: string;
  description: string;
  version: string;
  createdAt: string;
}

@Service()
export class AppInfo {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/appinfo`;

  getAppInfo(): Observable<AppInfoDto> {
    return this.http.get<AppInfoDto>(this.apiUrl);
  }
}