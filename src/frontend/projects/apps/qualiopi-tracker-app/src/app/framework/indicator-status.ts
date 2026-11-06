import { Component, computed, input } from '@angular/core';
import { IndicatorStatus } from '@qualiopi-tracker/shared/core';

const labels: Record<IndicatorStatus, string> = {
  Applicable: 'Applies',
  NotApplicable: 'Not applicable',
  OutOfScope: 'Outside your activities',
};

const styles: Record<IndicatorStatus, string> = {
  Applicable: 'bg-teal-50 text-teal-800',
  NotApplicable: 'bg-amber-50 text-amber-800',
  OutOfScope: 'bg-slate-100 text-slate-600',
};

@Component({
  selector: 'qt-indicator-status',
  template: `
    <span
      class="inline-block rounded-full px-2 py-0.5 text-xs font-medium whitespace-nowrap"
      [class]="style()"
      data-testid="indicator-status"
      >{{ label() }}</span
    >
  `,
})
export class IndicatorStatusBadge {
  readonly status = input.required<IndicatorStatus>();

  protected readonly label = computed(() => labels[this.status()]);
  protected readonly style = computed(() => styles[this.status()]);
}
