import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { ApiHealthService } from '@qualiopi-tracker/shared/core';
import { Shell } from '@qualiopi-tracker/shared/ui';

@Component({
  selector: 'qt-root',
  imports: [RouterOutlet, Shell],
  template: `
    <qt-shell appName="Qualiopi Tracker" [apiStatus]="apiStatus()">
      <router-outlet />
    </qt-shell>
  `,
})
export class App {
  protected readonly apiStatus = toSignal(inject(ApiHealthService).check(), {
    initialValue: 'checking',
  });
}
