import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  FormSubmission,
  ProviderProfile,
  ProviderService,
  SessionStore,
} from '@qualiopi-tracker/shared/core';
import { CategoryCheckboxes, FieldErrors } from '@qualiopi-tracker/shared/ui';
import { registrationNumberPattern } from './registration-number';

@Component({
  selector: 'qt-provider-profile-page',
  imports: [ReactiveFormsModule, CategoryCheckboxes, FieldErrors],
  template: `
    <form class="form-card" [formGroup]="form" (ngSubmit)="save()" novalidate>
      <h1 class="text-2xl font-bold tracking-tight">Provider profile</h1>

      @if (loadFailed()) {
        <p class="alert-error" role="alert">
          We could not load the provider profile. Please try again later.
        </p>
      } @else if (loaded()) {
        @if (!canEdit) {
          <p class="alert-info" role="status">
            Only the quality lead can edit the provider profile.
          </p>
        }
        @if (saved()) {
          <p class="alert-info" role="status">The provider profile has been saved.</p>
        }
        @if (submission.errors().form; as message) {
          <p class="alert-error" role="alert">{{ message }}</p>
        }

        <div>
          <label class="form-label" for="name">Provider name</label>
          <input id="name" class="form-input" formControlName="name" autocomplete="organization" />
          <qt-field-errors [messages]="submission.messagesFor('name')" />
        </div>

        <div>
          <label class="form-label" for="registrationNumber">Registration number (optional)</label>
          <input
            id="registrationNumber"
            class="form-input"
            formControlName="registrationNumber"
            inputmode="numeric"
          />
          <p class="form-hint">The 11-digit number of your training provider declaration.</p>
          <qt-field-errors [messages]="submission.messagesFor('registrationNumber')" />
        </div>

        <fieldset>
          <legend class="form-label">Activity categories</legend>
          <p class="form-hint">They decide which indicators apply to you.</p>
          <qt-category-checkboxes formControlName="activityCategories" />
          <qt-field-errors [messages]="submission.messagesFor('activityCategories')" />
        </fieldset>

        @if (canEdit) {
          <button class="button-primary" type="submit" [disabled]="submission.pending()">
            Save profile
          </button>
        }
      } @else {
        <p class="text-sm text-slate-500">Loading…</p>
      }
    </form>
  `,
})
export class ProviderProfilePage {
  private readonly provider = inject(ProviderService);

  protected readonly canEdit = inject(SessionStore).isQualityLead();
  protected readonly loaded = signal(false);
  protected readonly loadFailed = signal(false);
  protected readonly saved = signal(false);
  protected readonly form = inject(NonNullableFormBuilder).group({
    name: ['', Validators.required],
    registrationNumber: ['', Validators.pattern(registrationNumberPattern)],
    activityCategories: [[] as readonly string[], Validators.required],
  });
  protected readonly submission = new FormSubmission(this.form, {
    name: 'Enter the provider name.',
    registrationNumber: 'Registration number must be 11 digits.',
    activityCategories: 'Choose at least one activity category.',
  });

  constructor() {
    if (!this.canEdit) {
      this.form.disable();
    }

    this.provider.get().subscribe({
      next: (profile) => {
        this.show(profile);
        this.loaded.set(true);
      },
      error: () => this.loadFailed.set(true),
    });
  }

  protected save(): void {
    this.saved.set(false);
    this.submission.submit(
      () => this.provider.update(this.form.getRawValue()),
      (profile) => {
        this.show(profile);
        this.saved.set(true);
      },
    );
  }

  private show(profile: ProviderProfile): void {
    this.form.reset({
      name: profile.name,
      registrationNumber: profile.registrationNumber ?? '',
      activityCategories: profile.activityCategories,
    });
  }
}
