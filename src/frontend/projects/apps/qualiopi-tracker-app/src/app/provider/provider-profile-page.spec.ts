import { ProviderProfile } from '@qualiopi-tracker/shared/core';
import { AppHarness, memberSession } from '../../testing/app-harness';

const profile: ProviderProfile = {
  id: 'provider-1',
  name: 'Horizon Skills Academy',
  registrationNumber: '12345678901',
  activityCategories: ['TrainingCourses', 'Apprenticeship'],
};

describe('Provider profile page', () => {
  let app: AppHarness;

  beforeEach(() => (app = new AppHarness()));

  afterEach(() => app.http.verify());

  async function open(loaded: ProviderProfile = profile): Promise<void> {
    await app.open('/provider');
    expect(app.text('form p')).toBe('Loading…');
    app.http.expectOne('/api/provider').flush(loaded);
    await app.settle();
  }

  describe('as the quality lead', () => {
    beforeEach(() => app.signIn());

    it('shows the current profile', async () => {
      await open();

      expect(app.input('name').value).toBe('Horizon Skills Academy');
      expect(app.input('registrationNumber').value).toBe('12345678901');
      expect(app.input('category-TrainingCourses').checked).toBe(true);
      expect(app.input('category-SkillsAssessments').checked).toBe(false);
      expect(app.input('category-Apprenticeship').checked).toBe(true);
      expect(app.button().textContent?.trim()).toBe('Save profile');
    });

    it('shows an empty registration number when there is none', async () => {
      await open({ ...profile, registrationNumber: null });

      expect(app.input('registrationNumber').value).toBe('');
    });

    it('saves the changes and confirms', async () => {
      await open();
      await app.type('name', 'Horizon Academy');
      await app.type('registrationNumber', '');
      await app.click('#category-Apprenticeship');
      await app.click('#category-SkillsAssessments');

      await app.submit();
      const request = app.http.expectOne('/api/provider');
      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual({
        name: 'Horizon Academy',
        registrationNumber: '',
        activityCategories: ['TrainingCourses', 'SkillsAssessments'],
      });
      request.flush({
        ...profile,
        name: 'Horizon Academy',
        registrationNumber: null,
        activityCategories: ['TrainingCourses', 'SkillsAssessments'],
      });
      await app.settle();

      expect(app.text('[role="status"]')).toBe('The provider profile has been saved.');
      expect(app.input('name').value).toBe('Horizon Academy');
      expect(app.input('category-Apprenticeship').checked).toBe(false);
    });

    it('asks for a name and at least one category', async () => {
      await open();
      await app.type('name', '');
      await app.click('#category-TrainingCourses');
      await app.click('#category-Apprenticeship');

      await app.submit();

      expect(app.texts('qt-field-errors p')).toEqual([
        'Enter the provider name.',
        'Choose at least one activity category.',
      ]);
    });

    it('shows the API messages next to each field', async () => {
      await open();

      await app.submit();
      app.http
        .expectOne('/api/provider')
        .flush(
          { errors: { registrationNumber: ['Registration number must be 11 digits.'] } },
          { status: 400, statusText: 'Bad Request' },
        );
      await app.settle();

      expect(app.texts('qt-field-errors p')).toEqual(['Registration number must be 11 digits.']);
      expect(app.element.querySelector('[role="status"]')).toBeNull();
    });

    it('explains when the API refuses the change', async () => {
      await open();

      await app.submit();
      app.http
        .expectOne('/api/provider')
        .flush(
          { title: 'Only the quality lead can edit the provider profile.' },
          { status: 403, statusText: 'Forbidden' },
        );
      await app.settle();

      expect(app.text('[role="alert"]')).toBe(
        'Only the quality lead can edit the provider profile.',
      );
    });

    it('says when the profile cannot be loaded', async () => {
      await app.open('/provider');
      app.http.expectOne('/api/provider').flush(null, { status: 500, statusText: 'Server Error' });
      await app.settle();

      expect(app.text('[role="alert"]')).toBe(
        'We could not load the provider profile. Please try again later.',
      );
      expect(app.element.querySelector('#name')).toBeNull();
    });
  });

  describe('as a member', () => {
    beforeEach(() => app.signIn(memberSession));

    it('shows the profile read-only', async () => {
      await open();

      expect(app.text('[role="status"]')).toBe(
        'Only the quality lead can edit the provider profile.',
      );
      expect(app.input('name').disabled).toBe(true);
      expect(app.input('registrationNumber').disabled).toBe(true);
      expect(app.input('category-TrainingCourses').disabled).toBe(true);
      expect(app.input('category-TrainingCourses').checked).toBe(true);
      expect(app.element.querySelector('form button[type="submit"]')).toBeNull();
    });
  });

  it('asks visitors to log in first', async () => {
    await app.open('/provider');

    expect(app.router.url).toBe('/login?returnUrl=%2Fprovider');
  });
});
