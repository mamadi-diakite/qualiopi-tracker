import { Criterion, Indicator } from '@qualiopi-tracker/shared/core';
import { AppHarness, memberSession } from '../../testing/app-harness';

function indicator(overrides: Partial<Indicator> & Pick<Indicator, 'number'>): Indicator {
  return {
    criterionNumber: 1,
    title: `Indicator ${overrides.number}`,
    appliesTo: [
      'TrainingCourses',
      'SkillsAssessments',
      'PriorExperienceAccreditation',
      'Apprenticeship',
    ],
    status: 'Applicable',
    notApplicableReason: null,
    notes: null,
    ...overrides,
  };
}

const criteria: Criterion[] = [
  {
    number: 1,
    title: 'Information on the offer',
    indicators: [indicator({ number: 1, title: 'Offer details' })],
  },
  {
    number: 3,
    title: 'Learner support',
    indicators: [
      indicator({ number: 12, criterionNumber: 3, title: 'Engagement and dropout' }),
      indicator({
        number: 15,
        criterionNumber: 3,
        title: 'Apprentice rights',
        appliesTo: ['Apprenticeship'],
        status: 'OutOfScope',
      }),
    ],
  },
  {
    number: 6,
    title: 'Professional environment',
    indicators: [
      indicator({
        number: 27,
        criterionNumber: 6,
        title: 'Subcontracting',
        status: 'NotApplicable',
        notApplicableReason: 'We never subcontract.',
      }),
      indicator({
        number: 29,
        criterionNumber: 6,
        title: 'Further study and jobs',
        appliesTo: ['Apprenticeship'],
        status: 'OutOfScope',
      }),
    ],
  },
  {
    number: 7,
    title: 'Feedback and improvement',
    indicators: [
      indicator({
        number: 31,
        criterionNumber: 7,
        title: 'Complaints and incidents',
        status: 'OutOfScope',
      }),
    ],
  },
];

describe('Framework pages', () => {
  let app: AppHarness;

  beforeEach(() => (app = new AppHarness()));

  afterEach(() => app.http.verify());

  describe('framework', () => {
    beforeEach(() => app.signIn());

    async function open(): Promise<void> {
      await app.open('/framework');
      expect(app.text('main section p')).toBe('Loading…');
      app.http.expectOne('/api/framework').flush(criteria);
      await app.settle();
    }

    it('groups the indicators that apply by criterion', async () => {
      await open();

      expect(app.text('[data-testid="summary"]')).toBe(
        '2 apply to you · 1 marked not applicable · 3 outside your activities',
      );
      expect(app.texts('h2')).toEqual([
        'Criterion 1 · Information on the offer',
        'Criterion 3 · Learner support',
        'Criterion 6 · Professional environment',
      ]);
      expect(app.texts('li a')).toEqual([
        '1. Offer details',
        '12. Engagement and dropout',
        '27. Subcontracting',
      ]);
      expect(app.texts('[data-testid="indicator-status"]')).toEqual([
        'Applies',
        'Applies',
        'Not applicable',
      ]);
      expect(app.texts('li p')).toEqual(['We never subcontract.']);
      expect(app.element.querySelector('li a')?.getAttribute('href')).toBe(
        '/framework/indicators/1',
      );
    });

    it('can also show the indicators outside the activities', async () => {
      await open();

      await app.click('#showOutOfScope');

      expect(app.texts('h2')).toHaveLength(4);
      expect(app.texts('li a')).toContain('15. Apprentice rights');
      expect(app.texts('[data-testid="indicator-status"]')).toContain('Outside your activities');

      await app.click('#showOutOfScope');

      expect(app.texts('h2')).toHaveLength(3);
    });

    it('says when the framework cannot be loaded', async () => {
      await app.open('/framework');
      app.http.expectOne('/api/framework').flush(null, { status: 500, statusText: 'Error' });
      await app.settle();

      expect(app.text('[role="alert"]')).toBe(
        'We could not load the framework. Please try again later.',
      );
      expect(app.element.querySelector('[data-testid="summary"]')).toBeNull();
    });
  });

  describe('indicator', () => {
    async function open(loaded: Indicator, number = loaded.number): Promise<void> {
      await app.open(`/framework/indicators/${number}`);
      expect(app.text('article p')).toBe('Loading…');
      app.http.expectOne(`/api/framework/indicators/${number}`).flush(loaded);
      await app.settle();
    }

    describe('as the quality lead', () => {
      beforeEach(() => app.signIn());

      it('shows the indicator and its current settings', async () => {
        await open(indicator({ number: 2, title: 'Results figures', notes: 'Owner: Claire.' }));

        expect(app.text('h1')).toBe('Indicator 2 · Results figures');
        expect(app.text('header p')).toBe('Criterion 1');
        expect(app.text('[data-testid="indicator-status"]')).toBe('Applies');
        expect(app.text('[data-testid="applies-to"]')).toBe(
          'Applies to: Training courses, Skills assessments, ' +
            'Accreditation of prior experience (VAE), Apprenticeship',
        );
        expect(app.input('notApplicable').checked).toBe(false);
        expect(app.element.querySelector('#notApplicableReason')).toBeNull();
        expect(app.input('notes').value).toBe('Owner: Claire.');
      });

      it('marks the indicator as not applicable with a reason', async () => {
        await open(indicator({ number: 27, criterionNumber: 6, title: 'Subcontracting' }));

        await app.click('#notApplicable');
        await app.type('notApplicableReason', 'We never subcontract.');
        await app.type('notes', 'Checked each year.');
        await app.submit();

        const request = app.http.expectOne('/api/framework/indicators/27');
        expect(request.request.method).toBe('PUT');
        expect(request.request.body).toEqual({
          notApplicableReason: 'We never subcontract.',
          notes: 'Checked each year.',
        });
        request.flush(
          indicator({
            number: 27,
            criterionNumber: 6,
            title: 'Subcontracting',
            status: 'NotApplicable',
            notApplicableReason: 'We never subcontract.',
            notes: 'Checked each year.',
          }),
        );
        await app.settle();

        expect(app.text('[role="status"]')).toBe('The indicator has been saved.');
        expect(app.text('[data-testid="indicator-status"]')).toBe('Not applicable');
        expect(app.input('notApplicable').checked).toBe(true);
        expect(app.input('notApplicableReason').value).toBe('We never subcontract.');
      });

      it('asks for the reason before marking it not applicable', async () => {
        await open(indicator({ number: 27 }));

        await app.click('#notApplicable');
        await app.submit();

        expect(app.texts('qt-field-errors p')).toEqual([
          'Explain why this indicator does not apply.',
        ]);
      });

      it('makes the indicator applicable again', async () => {
        await open(
          indicator({
            number: 27,
            status: 'NotApplicable',
            notApplicableReason: 'We never subcontract.',
          }),
        );
        expect(app.input('notApplicableReason').value).toBe('We never subcontract.');

        await app.click('#notApplicable');
        await app.submit();

        expect(app.http.expectOne('/api/framework/indicators/27').request.body).toEqual({
          notApplicableReason: null,
          notes: '',
        });
      });

      it('only keeps notes for an indicator outside the activities', async () => {
        await open(indicator({ number: 15, appliesTo: ['Apprenticeship'], status: 'OutOfScope' }));

        expect(app.text('[data-testid="out-of-scope"]')).toBe(
          'This indicator is outside your activity categories, so you do not need to prove it.',
        );
        expect(app.text('[data-testid="applies-to"]')).toBe('Applies to: Apprenticeship');
        expect(app.element.querySelector('#notApplicable')).toBeNull();

        await app.type('notes', 'We may start apprenticeships next year.');
        await app.submit();

        expect(app.http.expectOne('/api/framework/indicators/15').request.body).toEqual({
          notApplicableReason: null,
          notes: 'We may start apprenticeships next year.',
        });
      });

      it('shows the API messages next to each field', async () => {
        await open(indicator({ number: 4 }));

        await app.submit();
        app.http
          .expectOne('/api/framework/indicators/4')
          .flush(
            { errors: { notes: ['Use at most 2000 characters.'] } },
            { status: 400, statusText: 'Bad Request' },
          );
        await app.settle();

        expect(app.texts('qt-field-errors p')).toEqual(['Use at most 2000 characters.']);
        expect(app.element.querySelector('[role="status"]')).toBeNull();
      });

      it('explains when the API refuses the change', async () => {
        await open(indicator({ number: 4 }));

        await app.submit();
        app.http
          .expectOne('/api/framework/indicators/4')
          .flush(
            { title: 'Only the quality lead can change indicators.' },
            { status: 403, statusText: 'Forbidden' },
          );
        await app.settle();

        expect(app.text('[role="alert"]')).toBe('Only the quality lead can change indicators.');
      });

      it.each([
        [404, 'This indicator does not exist.'],
        [500, 'We could not load this indicator. Please try again later.'],
      ])('explains a %s when loading', async (status, message) => {
        await app.open('/framework/indicators/40');
        app.http
          .expectOne('/api/framework/indicators/40')
          .flush(null, { status, statusText: 'Error' });
        await app.settle();

        expect(app.text('[role="alert"]')).toBe(message);
        expect(app.element.querySelector('h1')).toBeNull();
      });
    });

    describe('as a member', () => {
      beforeEach(() => app.signIn(memberSession));

      it('shows the reason and notes read-only', async () => {
        await open(
          indicator({
            number: 27,
            status: 'NotApplicable',
            notApplicableReason: 'We never subcontract.',
            notes: 'Owner: Claire.',
          }),
        );

        expect(app.element.querySelector('form')).toBeNull();
        expect(app.text('[data-testid="reason"]')).toBe('We never subcontract.');
        expect(app.text('[data-testid="notes"]')).toBe('Owner: Claire.');
      });

      it('says when there are no notes yet', async () => {
        await open(indicator({ number: 5 }));

        expect(app.element.querySelector('[data-testid="reason"]')).toBeNull();
        expect(app.text('[data-testid="notes"]')).toBe('No notes yet.');
      });
    });

    it('asks visitors to log in first', async () => {
      await app.open('/framework/indicators/1');

      expect(app.router.url).toBe('/login?returnUrl=%2Fframework%2Findicators%2F1');
    });
  });
});
