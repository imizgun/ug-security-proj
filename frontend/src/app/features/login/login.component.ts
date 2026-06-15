import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h1>UgSocial</h1>
        <p class="subtitle">Sign in to continue</p>

        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="field">
            <label>Email</label>
            <input type="email" formControlName="email" placeholder="you@example.com" autocomplete="email">
          </div>
          <div class="field">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="••••••" autocomplete="current-password">
          </div>

          <div class="error" *ngIf="error">{{ error }}</div>

          <button type="submit" [disabled]="loading">
            {{ loading ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>

        <div class="hint">
          <strong>Demo accounts:</strong><br>
          moderator&#64;ugsocial.local / Mod123!<br>
          user&#64;ugsocial.local / User123!
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #0f172a;
    }
    .login-card {
      background: #1e293b;
      padding: 2.5rem;
      border-radius: 12px;
      width: 100%;
      max-width: 380px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.4);
    }
    h1 { color: #60a5fa; margin: 0 0 0.25rem; font-size: 1.8rem; }
    .subtitle { color: #94a3b8; margin: 0 0 2rem; }
    .field { margin-bottom: 1.2rem; }
    label { display: block; color: #94a3b8; font-size: 0.85rem; margin-bottom: 0.4rem; }
    input {
      width: 100%; padding: 0.7rem 0.9rem; border-radius: 8px;
      background: #0f172a; border: 1px solid #334155; color: #f1f5f9;
      font-size: 0.95rem; box-sizing: border-box;
    }
    input:focus { outline: none; border-color: #60a5fa; }
    .error { color: #f87171; margin-bottom: 1rem; font-size: 0.9rem; }
    button {
      width: 100%; padding: 0.8rem; background: #3b82f6; color: white;
      border: none; border-radius: 8px; font-size: 1rem; cursor: pointer;
    }
    button:disabled { opacity: 0.6; cursor: not-allowed; }
    button:hover:not(:disabled) { background: #2563eb; }
    .hint { margin-top: 1.5rem; color: #64748b; font-size: 0.8rem; line-height: 1.6; }
  `]
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private oauth = inject(OAuthService);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  error = '';
  loading = false;

  ngOnInit() {}

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';

    const { email, password } = this.form.value;

    this.http.post(`${environment.apiUrl}/api/auth/login`, { email, password }, { withCredentials: true })
      .subscribe({
        next: () => {
          // Cookie set — now start PKCE flow; backend will issue auth code
          this.oauth.initCodeFlow();
        },
        error: () => {
          this.error = 'Invalid email or password.';
          this.loading = false;
        }
      });
  }
}
