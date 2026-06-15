import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { OAuthService, AuthConfig } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />'
})
export class App implements OnInit {
  private oauth = inject(OAuthService);
  private authConfig = inject(AuthConfig);

  ngOnInit() {
    this.oauth.configure(this.authConfig);
    this.oauth.loadDiscoveryDocumentAndTryLogin();
  }
}
