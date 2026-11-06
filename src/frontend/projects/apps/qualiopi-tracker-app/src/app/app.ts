import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { ApiHealthService, AuthService, SessionStore } from '@qualiopi-tracker/shared/core';
import { Shell } from '@qualiopi-tracker/shared/ui';

@Component({
  selector: 'qt-root',
  imports: [RouterOutlet, RouterLink, Shell],
  template: `
    <qt-shell appName="Qualiopi Tracker" [apiStatus]="apiStatus()">
      <ng-container shellNav>
        @if (user(); as user) {
          <span class="text-slate-600" data-testid="user-name">{{ user.displayName }}</span>
          <a routerLink="/framework" class="link">Framework</a>
          <a routerLink="/provider" class="link">Provider profile</a>
          <a routerLink="/account/password" class="link">Change password</a>
          <button type="button" class="link" (click)="logOut()">Log out</button>
        } @else {
          <a routerLink="/login" class="link">Log in</a>
          <a routerLink="/sign-up" class="link">Create your provider</a>
        }
      </ng-container>
      <router-outlet />
    </qt-shell>
  `,
})
export class App {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly user = inject(SessionStore).user;
  protected readonly apiStatus = toSignal(inject(ApiHealthService).check(), {
    initialValue: 'checking',
  });

  protected logOut(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }
}
