import { Injectable, inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private oauth = inject(OAuthService);
  private http = inject(HttpClient);
  private router = inject(Router);

  get isLoggedIn(): boolean {
    return this.oauth.hasValidAccessToken();
  }

  get accessToken(): string {
    return this.oauth.getAccessToken();
  }

  get userRoles(): string[] {
    const claims = this.oauth.getIdentityClaims() as Record<string, unknown> | null;
    if (!claims) return [];
    const roles = claims['role'] ?? claims['roles'];
    if (Array.isArray(roles)) return roles as string[];
    if (typeof roles === 'string') return [roles];
    return [];
  }

  get isModerator(): boolean {
    return this.userRoles.includes('moderator');
  }

  get displayName(): string {
    const claims = this.oauth.getIdentityClaims() as Record<string, unknown> | null;
    return (claims?.['name'] as string) ?? (claims?.['email'] as string) ?? 'User';
  }

  login(email: string, password: string, returnUrl: string) {
    return this.http.post(`${environment.apiUrl}/api/auth/login`, { email, password }, {
      withCredentials: true
    }).subscribe({
      next: () => {
        window.location.href = returnUrl || '/connect/authorize';
      },
      error: () => {
        throw new Error('Invalid credentials');
      }
    });
  }

  startOAuthFlow() {
    this.oauth.initCodeFlow();
  }

  async handleCallback(): Promise<void> {
    await this.oauth.tryLoginCodeFlow();
  }

  logout() {
    this.oauth.logOut();
  }
}
