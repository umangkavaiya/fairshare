import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
  },
  {
    path: 'groups',
    canActivate: [authGuard],
    loadComponent: () => import('./features/groups/group-list/group-list').then((m) => m.GroupList),
  },
  {
    path: 'groups/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/groups/create-group/create-group').then((m) => m.CreateGroup),
  },
  {
    path: 'groups/:groupId',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/groups/group-detail/group-detail').then((m) => m.GroupDetail),
  },
  {
    path: 'groups/:groupId/expenses/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/expenses/add-expense/add-expense').then((m) => m.AddExpense),
  },
];
