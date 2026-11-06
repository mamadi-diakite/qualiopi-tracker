import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Criterion, FrameworkService } from '@qualiopi-tracker/shared/core';
import { IndicatorStatusBadge } from './indicator-status';

@Component({
  selector: 'qt-framework-page',
  imports: [RouterLink, IndicatorStatusBadge],
  template: `
    <section class="space-y-6">
      <header class="flex flex-wrap items-end justify-between gap-4">
        <div class="space-y-1">
          <h1 class="text-2xl font-bold tracking-tight">Quality framework</h1>
          @if (summary(); as summary) {
            <p class="text-sm text-slate-600" data-testid="summary">{{ summary }}</p>
          }
        </div>
        <label class="flex items-center gap-2 text-sm text-slate-700">
          <input
            #box
            id="showOutOfScope"
            type="checkbox"
            class="size-4 accent-teal-600"
            [checked]="showOutOfScope()"
            (change)="showOutOfScope.set(box.checked)"
          />
          Show indicators outside my activities
        </label>
      </header>

      @if (loadFailed()) {
        <p class="alert-error" role="alert">
          We could not load the framework. Please try again later.
        </p>
      } @else if (visibleCriteria(); as criteria) {
        @for (criterion of criteria; track criterion.number) {
          <section
            class="rounded-xl border border-slate-200 bg-white shadow-sm"
            [attr.aria-labelledby]="'criterion-' + criterion.number"
          >
            <h2
              class="border-b border-slate-200 px-4 py-3 font-semibold"
              [id]="'criterion-' + criterion.number"
            >
              Criterion {{ criterion.number }} · {{ criterion.title }}
            </h2>
            <ul class="divide-y divide-slate-100">
              @for (indicator of criterion.indicators; track indicator.number) {
                <li class="flex items-start justify-between gap-4 px-4 py-3">
                  <div>
                    <a [routerLink]="['/framework/indicators', indicator.number]" class="link">
                      {{ indicator.number }}. {{ indicator.title }}
                    </a>
                    @if (indicator.notApplicableReason; as reason) {
                      <p class="mt-1 text-sm text-slate-500">{{ reason }}</p>
                    }
                  </div>
                  <qt-indicator-status [status]="indicator.status" />
                </li>
              }
            </ul>
          </section>
        }
      } @else {
        <p class="text-sm text-slate-500">Loading…</p>
      }
    </section>
  `,
})
export class FrameworkPage {
  private readonly criteria = signal<readonly Criterion[] | null>(null);

  protected readonly loadFailed = signal(false);
  protected readonly showOutOfScope = signal(false);

  protected readonly visibleCriteria = computed(() =>
    this.criteria()
      ?.map((criterion) => ({
        ...criterion,
        indicators: criterion.indicators.filter(
          (indicator) => this.showOutOfScope() || indicator.status !== 'OutOfScope',
        ),
      }))
      .filter((criterion) => criterion.indicators.length > 0),
  );

  protected readonly summary = computed(() => {
    const indicators = this.criteria()?.flatMap((criterion) => criterion.indicators);
    if (!indicators) {
      return null;
    }

    const count = (status: string) => indicators.filter((i) => i.status === status).length;
    return (
      `${count('Applicable')} apply to you · ${count('NotApplicable')} marked not applicable · ` +
      `${count('OutOfScope')} outside your activities`
    );
  });

  constructor() {
    inject(FrameworkService)
      .getCriteria()
      .subscribe({
        next: (criteria) => this.criteria.set(criteria),
        error: () => this.loadFailed.set(true),
      });
  }
}
