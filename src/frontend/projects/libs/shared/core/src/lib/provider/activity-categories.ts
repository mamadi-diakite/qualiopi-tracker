export interface ActivityCategory {
  /** Name used by the API. */
  readonly value: string;
  readonly label: string;
}

/** The four kinds of activity a provider can deliver, in the order the API uses. */
export const ACTIVITY_CATEGORIES: readonly ActivityCategory[] = [
  { value: 'TrainingCourses', label: 'Training courses' },
  { value: 'SkillsAssessments', label: 'Skills assessments' },
  { value: 'PriorExperienceAccreditation', label: 'Accreditation of prior experience (VAE)' },
  { value: 'Apprenticeship', label: 'Apprenticeship' },
];

/** The label of a category, or the API name itself when it is unknown. */
export function activityCategoryLabel(value: string): string {
  return ACTIVITY_CATEGORIES.find((category) => category.value === value)?.label ?? value;
}
