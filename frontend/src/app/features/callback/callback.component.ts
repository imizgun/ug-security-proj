import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-callback',
  standalone: true,
  template: `<div style="min-height:100vh;display:flex;align-items:center;justify-content:center;background:#0f172a;color:#94a3b8">Authenticating…</div>`
})
export class CallbackComponent implements OnInit {
  private oauth = inject(OAuthService);
  private router = inject(Router);

  async ngOnInit() {
    try {
      await this.oauth.tryLoginCodeFlow({ preventClearHashAfterLogin: false });
      await this.router.navigate(['/feed']);
    } catch {
      await this.router.navigate(['/login']);
    }
  }
}
