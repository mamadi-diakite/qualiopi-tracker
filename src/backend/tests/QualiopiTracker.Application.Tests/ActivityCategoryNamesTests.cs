namespace QualiopiTracker.Application.Tests;

public sealed class ActivityCategoryNamesTests
{
    [Fact]
    public void Lists_names_in_a_fixed_order()
    {
        Assert.Equal(
            ["TrainingCourses", "SkillsAssessments", "PriorExperienceAccreditation", "Apprenticeship"],
            ActivityCategoryNames.ToNames(ActivityCategories.All));
        Assert.Equal(
            ["TrainingCourses", "Apprenticeship"],
            ActivityCategoryNames.ToNames(ActivityCategories.Apprenticeship | ActivityCategories.TrainingCourses));
        Assert.Empty(ActivityCategoryNames.ToNames(ActivityCategories.None));
    }

    [Fact]
    public void Parses_known_names_and_ignores_repeats()
    {
        Assert.Equal(
            ActivityCategories.SkillsAssessments | ActivityCategories.PriorExperienceAccreditation,
            ActivityCategoryNames.Parse(["PriorExperienceAccreditation", "SkillsAssessments", "SkillsAssessments"]));
    }

    [Fact]
    public void No_names_give_no_category()
    {
        Assert.Equal(ActivityCategories.None, ActivityCategoryNames.Parse([]));
        Assert.Equal(ActivityCategories.None, ActivityCategoryNames.Parse(null));
    }

    [Theory]
    [InlineData("Webinars")]
    [InlineData("trainingcourses")]
    [InlineData("None")]
    [InlineData("All")]
    public void Unknown_names_are_rejected(string name)
    {
        Assert.Null(ActivityCategoryNames.Parse(["TrainingCourses", name]));
    }
}
