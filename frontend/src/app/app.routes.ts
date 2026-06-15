import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'feed', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'callback',
    loadComponent: () =>
      import('./features/callback/callback.component').then(m => m.CallbackComponent)
  },
  {
    path: 'feed',
    loadComponent: () =>
      import('./features/feed/feed.component').then(m => m.FeedComponent),
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: 'feed' }
];
