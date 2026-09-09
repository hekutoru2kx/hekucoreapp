import { Service, effect, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';
import { Theme } from './theme';

export interface AuthResponse {
  token: string;
  email: string;
  userName: string;
  mustChangePassword: boolean;
  preferredTheme: string;
  // Set by /register when the app requires email confirmation: the account exists but is
  // not logged in — token is empty and the user must confirm via the emailed link.
  requiresEmailConfirmation?: boolean;
}

export interface CurrentUser {
  email: string;
  userName: string;
  mustChangePassword: boolean;
  preferredTheme: string;
}

interface DecodedToken {
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  [key: string]: unknown;
}

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const TOKEN_KEY = 'hekucoreapp_token';
const USER_KEY = 'hekucoreapp_user';
const RESERVED_CLAIM_KEYS = new Set(['sub', 'email', 'userName', 'exp', 'iss', 'aud', 'nbf', 'iat', ROLE_CLAIM]);

export interface PermissionClaim {
  module: string;
  action: string;
}

@Service()
export class Auth {
  private http = inject(HttpClient);
  private theme = inject(Theme);
  private apiUrl = `${environment.apiUrl}/auth`;

  token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  currentUser = signal<CurrentUser | null>(this.loadUser());

  private applyStoredTheme = effect(() => {
    const user = this.currentUser();
    if (user?.preferredTheme) {
      this.theme.setColorTheme(user.preferredTheme as any);
    }
  });
  
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(tap(res => this.setSession(res)));
  }

  register(userName: string, email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { userName, email, password })
      .pipe(tap(res => {
        // When confirmation is required there is no session to start yet.
        if (!res.requiresEmailConfirmation) this.setSession(res);
      }));
  }

  loginWithGoogle(idToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/google`, { idToken })
      .pipe(tap(res => this.setSession(res)));
  }

  confirmEmail(userId: string, token: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/confirm-email`, { userId, token });
  }

  resendConfirmation(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/resend-confirmation`, { email });
  }

  logout(): void {
    this.token.set(null);
    this.currentUser.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  isLoggedIn(): boolean {
    return this.token() !== null;
  }

  getRoles(): string[] {
    const currentToken = this.token();
    if (!currentToken) return [];

    try {
      const decoded = jwtDecode<DecodedToken>(currentToken);
      const role = decoded[ROLE_CLAIM];
      if (!role) return [];
      return Array.isArray(role) ? role : [role];
    } catch {
      return [];
    }
  }

  hasRole(role: string): boolean {
    return this.getRoles().includes(role);
  }

  getClaims(): PermissionClaim[] {
    const currentToken = this.token();
    if (!currentToken) return [];

    try {
      const decoded = jwtDecode<DecodedToken>(currentToken);
      const claims: PermissionClaim[] = [];

      for (const [key, value] of Object.entries(decoded)) {
        if (RESERVED_CLAIM_KEYS.has(key)) continue;
        const actions = Array.isArray(value) ? value : [value];
        for (const action of actions) {
          if (typeof action === 'string') claims.push({ module: key, action });
        }
      }

      return claims;
    } catch {
      return [];
    }
  }

  hasClaim(module: string, action: string): boolean {
    return this.getClaims().some(c => c.module === module && c.action === action);
  }

  private setSession(res: AuthResponse): void {
    this.token.set(res.token);
    this.currentUser.set({
      email: res.email,
      userName: res.userName,
      mustChangePassword: res.mustChangePassword,
      preferredTheme: res.preferredTheme
    });
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USER_KEY, JSON.stringify({
      email: res.email,
      userName: res.userName,
      mustChangePassword: res.mustChangePassword
    }));
  }

  private loadUser(): CurrentUser | null {
    const stored = localStorage.getItem(USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}