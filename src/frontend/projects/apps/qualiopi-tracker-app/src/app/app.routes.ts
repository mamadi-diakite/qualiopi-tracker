import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@qualiopi-tracker/shared/core';
import { ChangePasswordPage } from './auth/change-password-page';
import { LoginPage } from './auth/login-page';
import { SignUpPage } from './auth/sign-up-page';
import { HomePage } from './home/home-page';
import { ProviderProfilePage } from './provider/provider-profile-page';

export const routes: Routes = [
  { path: '', component: HomePage, title: 'Qualiopi Tracker' },
  {
    path: 'login',
    component: LoginPage,
    canActivate: [guestGuard],
    title: 'Log in · Qualiopi Tracker',
  },
  {
    path: 'sign-up',
    component: SignUpPage,
    canActivate: [guestGuard],
    title: 'Create your provider · Qualiopi Tracker',
  },
  {
    path: 'account/password',
    component: ChangePasswordPage,
    canActivate: [authGuard],
    title: 'Change password · Qualiopi Tracker',
  },
  {
    path: 'provider',
    component: ProviderProfilePage,
    canActivate: [authGuard],
    title: 'Provider profile · Qualiopi Tracker',
  },
  { path: '**', redirectTo: '' },
];
