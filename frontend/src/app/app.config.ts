import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideOAuthClient, AuthConfig } from 'angular-oauth2-oidc';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth.interceptor';
import { environment } from '../environments/environment';

const authConfig: AuthConfig = environment.oauth as AuthConfig;

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideOAuthClient({ resourceServer: { sendAccessToken: false } }),
    {
      provide: AuthConfig,
      useValue: authConfig
    }
  ]
};
