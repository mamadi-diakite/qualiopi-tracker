import { Component, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, FormSubmission } from '@qualiopi-tracker/shared/core';
import { CategoryCheckboxes, FieldErrors } from '@qualiopi-tracker/shared/ui';
import { registrationNumberPattern } from '../provider/registration-number';

@Component({
  selector: 'qt-sign-up-page',
  imports: [ReactiveFormsModule, RouterLink, CategoryCheckboxes, FieldErrors],
  template: `
    <form class="form-card" [formGroup]="form" (ngSubmit)="signUp()" novalidate>
      <h1 class="text-2xl font-bold tracking-tight">Create your provider</h1>

      @if (submission.errors().form; as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      <p class="text-sm text-slate-600">
        You become the quality lead of your provider and can invite your team later.
      </p>

      <div>
        <label class="form-label" for="providerName">Provider name</label>
        <input
          id="providerName"
          class="form-input"
          formControlName="providerName"
          autocomplete="organization"
        />
        <qt-field-errors [messages]="submission.messagesFor('providerName')" />
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
        <qt-category-checkboxes formControlName="activityCategories" />
        <qt-field-errors [messages]="submission.messagesFor('activityCategories')" />
      </fieldset>

      <div>
        <label class="form-label" for="displayName">Your name</label>
        <input
          id="displayName"
          class="form-input"
          formControlName="displayName"
          autocomplete="name"
        />
        <qt-field-errors [messages]="submission.messagesFor('displayName')" />
      </div>

      <div>
        <label class="form-label" for="email">Email</label>
        <input
          id="email"
          class="form-input"
          type="email"
          formControlName="email"
          autocomplete="email"
        />
        <qt-field-errors [messages]="submission.messagesFor('email')" />
      </div>

      <div>
        <label class="form-label" for="password">Password</label>
        <input
          id="password"
          class="form-input"
          type="password"
          formControlName="password"
          autocomplete="new-password"
        />
        <p class="form-hint">At least 8 characters, with a letter and a digit.</p>
        <qt-field-errors [messages]="submission.messagesFor('password')" />
      </div>

      <div>
        <label class="form-label" for="confirmPassword">Confirm password</label>
        <input
          id="confirmPassword"
          class="form-input"
          type="password"
          formControlName="confirmPassword"
          autocomplete="new-password"
        />
        <qt-field-errors [messages]="submission.messagesFor('confirmPassword')" />
      </div>

      <button class="button-primary" type="submit" [disabled]="submission.pending()">
        Create my provider
      </button>

      <p class="text-center text-sm text-slate-600">
        Already have an account?
        <a routerLink="/login" class="link">Log in</a>
      </p>
    </form>
  `,
})
export class SignUpPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly form = inject(NonNullableFormBuilder).group({
    providerName: ['', Validators.required],
    registrationNumber: ['', Validators.pattern(registrationNumberPattern)],
    activityCategories: [[] as readonly string[], Validators.required],
    displayName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    confirmPassword: ['', Validators.required],
  });
  protected readonly submission = new FormSubmission(this.form, {
    providerName: 'Enter the provider name.',
    registrationNumber: 'Registration number must be 11 digits.',
    activityCategories: 'Choose at least one activity category.',
    displayName: 'Enter your name.',
    email: 'Enter a valid email address.',
    password: 'Choose a password.',
    confirmPassword: 'Confirm your password.',
  });

  protected signUp(): void {
    this.submission.submit(
      () => this.auth.signUp(this.form.getRawValue()),
      () => void this.router.navigateByUrl('/'),
    );
  }
}
