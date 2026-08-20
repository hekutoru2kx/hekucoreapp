import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Auth } from '../../../services/auth';
import { GoogleSignInButton } from '../../../components/google-sign-in-button/google-sign-in-button';

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    TranslocoModule,
    GoogleSignInButton
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private fb = inject(FormBuilder);
  private auth = inject(Auth);
  private router = inject(Router);
  private transloco = inject(TranslocoService);

  errorMessage = signal('');

  form = this.fb.group({
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordsMatchValidator });

  onSubmit(): void {
    if (this.form.invalid) return;

    const { userName, email, password } = this.form.value;

    this.auth.register(userName!, email!, password!).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        if (err.status === 0) {
          this.errorMessage.set(this.transloco.translate('common.networkError'));
        } else {
          this.errorMessage.set(err.error || this.transloco.translate('auth.registrationFailed'));
        }
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  onGoogleCredential(idToken: string): void {
    this.auth.loginWithGoogle(idToken).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        if (err.status === 0) {
          this.errorMessage.set(this.transloco.translate('common.networkError'));
        } else {
          this.errorMessage.set(err.error || this.transloco.translate('auth.registrationFailed'));
        }
      }
    });
  }
}