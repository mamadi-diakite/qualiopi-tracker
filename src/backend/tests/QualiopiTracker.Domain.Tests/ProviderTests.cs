namespace QualiopiTracker.Domain.Tests;

public sealed class ProviderTests
{
    [Fact]
    public void Creates_a_provider()
    {
        var provider = new Provider(
            " Northwind Training ", " 12345678901 ",
            ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship, TestDates.Now);

        Assert.NotEqual(Guid.Empty, provider.Id);
        Assert.Equal("Northwind Training", provider.Name);
        Assert.Equal("12345678901", provider.RegistrationNumber);
        Assert.Equal(ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship, provider.ActivityCategories);
        Assert.Equal(TestDates.Now, provider.CreatedAtUtc);
    }

    [Fact]
    public void Registration_number_is_optional()
    {
        Assert.Null(new Provider("Northwind Training", " ", ActivityCategories.All, TestDates.Now).RegistrationNumber);
    }

    [Fact]
    public void Updates_the_profile()
    {
        var provider = new Provider("Northwind Training", null, ActivityCategories.TrainingCourses, TestDates.Now);

        provider.UpdateProfile(" Northwind Skills ", "10987654321", ActivityCategories.SkillsAssessments);

        Assert.Equal("Northwind Skills", provider.Name);
        Assert.Equal("10987654321", provider.RegistrationNumber);
        Assert.Equal(ActivityCategories.SkillsAssessments, provider.ActivityCategories);
    }

    [Fact]
    public void Rejected_update_keeps_the_profile()
    {
        var provider = new Provider("Northwind Training", "12345678901", ActivityCategories.TrainingCourses, TestDates.Now);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => provider.UpdateProfile("Other name", null, ActivityCategories.None));

        Assert.Equal("Northwind Training", provider.Name);
        Assert.Equal("12345678901", provider.RegistrationNumber);
        Assert.Equal(ActivityCategories.TrainingCourses, provider.ActivityCategories);
    }

    [Fact]
    public void Rejects_invalid_values()
    {
        Assert.Equal("name", Assert.Throws<ArgumentException>(
            () => new Provider(" ", null, ActivityCategories.All, TestDates.Now)).ParamName);
        Assert.Equal("registrationNumber", Assert.Throws<ArgumentException>(
            () => new Provider("Northwind", "1234", ActivityCategories.All, TestDates.Now)).ParamName);
        Assert.Equal("activityCategories", Assert.Throws<ArgumentOutOfRangeException>(
            () => new Provider("Northwind", null, ActivityCategories.None, TestDates.Now)).ParamName);
        Assert.Equal("createdAtUtc", Assert.Throws<ArgumentException>(
            () => new Provider("Northwind", null, ActivityCategories.All,
                DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(" ", true)]
    [InlineData("12345678901", true)]
    [InlineData(" 12345678901 ", true)]
    [InlineData("1234567890", false)]
    [InlineData("123456789012", false)]
    [InlineData("1234567890A", false)]
    public void Registration_number_has_eleven_digits(string? value, bool expected)
    {
        Assert.Equal(expected, Provider.IsValidRegistrationNumber(value));
    }

    [Theory]
    [InlineData(ActivityCategories.TrainingCourses, true)]
    [InlineData(ActivityCategories.All, true)]
    [InlineData(ActivityCategories.None, false)]
    [InlineData((ActivityCategories)16, false)]
    [InlineData((ActivityCategories)17, false)]
    public void Activity_categories_need_one_known_value(ActivityCategories value, bool expected)
    {
        Assert.Equal(expected, Provider.IsValidActivityCategories(value));
    }
}
