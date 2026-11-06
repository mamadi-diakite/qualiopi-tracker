import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  activityCategoryLabel,
  FormSubmission,
  FrameworkService,
  Indicator,
  SessionStore,
} from '@qualiopi-tracker/shared/core';
import { FieldErrors } from '@qualiopi-tracker/shared/ui';
import { IndicatorStatusBadge } from './indicator-status';

@Component({
  selector: 'qt-indicator-page',
  imports: [ReactiveFormsModule, RouterLink, FieldErrors, IndicatorStatusBadge],
  template: `
    <article class="mx-auto max-w-2xl space-y-6">
      <a routerLink="/framework" class="link text-sm">← All indicators</a>

      @if (loadError(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      } @else if (indicator(); as indicator) {
        <header class="space-y-2">
          <p class="text-sm text-slate-500">Criterion {{ indicator.criterionNumber }}</p>
          <h1 class="text-2xl font-bold tracking-tight">
            Indicator {{ indicator.number }} · {{ indicator.title }}
          </h1>
          <qt-indicator-status [status]="indicator.status" />
          <p class="text-sm text-slate-600" data-testid="applies-to">
            Applies to: {{ appliesTo(indicator) }}
          </p>
        </header>

        @if (indicator.status === 'OutOfScope') {
          <p class="alert-info" data-testid="out-of-scope">
            This indicator is outside your activity categories, so you do not need to prove it.
          </p>
        }

        @if (canEdit) {
          <form
            class="space-y-4 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
            [formGroup]="form"
            (ngSubmit)="save()"
            novalidate
          >
            @if (saved()) {
              <p class="alert-info" role="status">The indicator has been saved.</p>
            }
            @if (submission.errors().form; as message) {
              <p class="alert-error" role="alert">{{ message }}</p>
            }

            @if (indicator.status !== 'OutOfScope') {
              <label class="flex items-center gap-2 text-sm text-slate-700">
                <input
                  id="notApplicable"
                  type="checkbox"
                  class="size-4 accent-teal-600"
                  formControlName="notApplicable"
                />
                This indicator does not apply to us
              </label>

              @if (form.controls.notApplicable.value) {
                <div>
                  <label class="form-label" for="notApplicableReason">Reason</label>
                  <textarea
                    id="notApplicableReason"
                    class="form-input"
                    rows="3"
                    formControlName="notApplicableReason"
                  ></textarea>
                  <p class="form-hint">For example: we deliver every course ourselves.</p>
                  <qt-field-errors [messages]="submission.messagesFor('notApplicableReason')" />
                </div>
              }
            }

            <div>
              <label class="form-label" for="notes">Notes</label>
              <textarea id="notes" class="form-input" rows="5" formControlName="notes"></textarea>
              <p class="form-hint">How the indicator applies to you, who owns it.</p>
              <qt-field-errors [messages]="submission.messagesFor('notes')" />
            </div>

            <button class="button-primary" type="submit" [disabled]="submission.pending()">
              Save indicator
            </button>
          </form>
        } @else {
          <dl class="space-y-4 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            @if (indicator.notApplicableReason; as reason) {
              <div>
                <dt class="form-label">Why it does not apply</dt>
                <dd class="mt-1 text-sm whitespace-pre-line" data-testid="reason">{{ reason }}</dd>
              </div>
            }
            <div>
              <dt class="form-label">Notes</dt>
              <dd class="mt-1 text-sm whitespace-pre-line" data-testid="notes">
                {{ indicator.notes || 'No notes yet.' }}
              </dd>
            </div>
          </dl>
        }
      } @else {
        <p class="text-sm text-slate-500">Loading…</p>
      }
    </article>
  `,
})
export class IndicatorPage {
  private readonly framework = inject(FrameworkService);
  private readonly number = Number(inject(ActivatedRoute).snapshot.paramMap.get('number'));

  protected readonly canEdit = inject(SessionStore).isQualityLead();
  protected readonly indicator = signal<Indicator | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saved = signal(false);
  protected readonly form = inject(NonNullableFormBuilder).group({
    notApplicable: false,
    notApplicableReason: [{ value: '', disabled: true }, Validators.required],
    notes: '',
  });
  protected readonly submission = new FormSubmission(this.form, {
    notApplicableReason: 'Explain why this indicator does not apply.',
  });

  constructor() {
    this.form.controls.notApplicable.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((notApplicable) => {
        const reason = this.form.controls.notApplicableReason;
        if (notApplicable) {
          reason.enable();
        } else {
          reason.disable();
        }
      });

    this.framework.getIndicator(this.number).subscribe({
      next: (indicator) => this.show(indicator),
      error: (error: unknown) =>
        this.loadError.set(
          error instanceof HttpErrorResponse && error.status === HttpStatusCode.NotFound
            ? 'This indicator does not exist.'
            : 'We could not load this indicator. Please try again later.',
        ),
    });
  }

  protected appliesTo(indicator: Indicator): string {
    return indicator.appliesTo.map(activityCategoryLabel).join(', ');
  }

  protected save(): void {
    this.saved.set(false);
    const { notApplicable, notApplicableReason, notes } = this.form.getRawValue();
    this.submission.submit(
      () =>
        this.framework.updateIndicator(this.number, {
          notApplicableReason: notApplicable ? notApplicableReason : null,
          notes,
        }),
      (indicator) => {
        this.show(indicator);
        this.saved.set(true);
      },
    );
  }

  private show(indicator: Indicator): void {
    this.indicator.set(indicator);
    this.form.reset({
      notApplicable: indicator.status === 'NotApplicable',
      notApplicableReason: indicator.notApplicableReason ?? '',
      notes: indicator.notes ?? '',
    });
  }
}
