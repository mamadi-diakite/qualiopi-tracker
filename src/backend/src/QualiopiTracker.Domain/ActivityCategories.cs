namespace QualiopiTracker.Domain;

/// <summary>The kinds of activity a provider runs. Some indicators apply only to some of them.</summary>
[Flags]
public enum ActivityCategories
{
    None = 0,
    TrainingCourses = 1,
    SkillsAssessments = 2,
    PriorExperienceAccreditation = 4,
    Apprenticeship = 8,
    All = TrainingCourses | SkillsAssessments | PriorExperienceAccreditation | Apprenticeship,
}
