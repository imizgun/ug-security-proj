import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';

export const authGuard: CanActivateFn = (_, state) => {
  const oauth = inject(OAuthService);
  const router = inject(Router);

  if (oauth.hasValidAccessToken()) {
    return true;
  }

  // Redirect to Angular login page (renders immediately, no white flash)
  return router.createUrlTree(['/login']);
};