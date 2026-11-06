import { ACTIVITY_CATEGORIES, activityCategoryLabel } from './activity-categories';

describe('activity categories', () => {
  it('lists the four categories in the API order', () => {
    expect(ACTIVITY_CATEGORIES.map((category) => category.value)).toEqual([
      'TrainingCourses',
      'SkillsAssessments',
      'PriorExperienceAccreditation',
      'Apprenticeship',
    ]);
  });

  it.each([
    ['TrainingCourses', 'Training courses'],
    ['PriorExperienceAccreditation', 'Accreditation of prior experience (VAE)'],
    ['Unknown', 'Unknown'],
  ])('labels %s', (value, label) => {
    expect(activityCategoryLabel(value)).toBe(label);
  });
});
