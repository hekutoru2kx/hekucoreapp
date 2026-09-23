import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Theme } from '../../services/theme';
import { Auth } from '../../services/auth';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-nav-bar',
  imports: [MatToolbarModule, MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule, TranslocoModule],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.scss',
})
export class NavBar {
  theme = inject(Theme);
  auth = inject(Auth);
  private transloco = inject(TranslocoService);
  private router = inject(Router);

  get currentLang(): string {
    return this.transloco.getActiveLang().toUpperCase();
  }

  toggleLanguage(): void {
    const next = this.transloco.getActiveLang() === 'es' ? 'en' : 'es';
    this.transloco.setActiveLang(next);
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  goToAdminUsers(): void {
    this.router.navigate(['/admin/users']);
  }

  goToProfile(): void {
    this.router.navigate(['/profile']);
  }

  goToHome(): void {
    this.router.navigate(['/']);
  }

  goToPersons(): void {
    this.router.navigate(['/admin/persons']);
  }

  goToRoles(): void {
    this.router.navigate(['/admin/roles']);
  }

  goToSettings(): void {
    this.router.navigate(['/admin/settings']);
  }

  goToLoggingSettings(): void {
    this.router.navigate(['/admin/logging-settings']);
  }

  goToLogs(): void {
    this.router.navigate(['/admin/logs']);
  }
}