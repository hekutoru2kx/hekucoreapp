import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from './components/nav-bar/nav-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoModule } from '@jsverse/transloco';
import { Auth } from './services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavBar, MatIconModule, MatButtonModule, TranslocoModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  auth = inject(Auth);
  router = inject(Router);

  goToChangePassword(): void {
    this.router.navigate(['/change-password']);
  }
}