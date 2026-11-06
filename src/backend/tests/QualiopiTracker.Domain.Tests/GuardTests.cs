namespace QualiopiTracker.Domain.Tests;

public sealed class GuardTests
{
    [Fact]
    public void Required_trims_the_value()
    {
        Assert.Equal("value", Guard.Required("  value ", "name", 10));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Required_rejects_blank_values(string? value)
    {
        var error = Assert.Throws<ArgumentException>(() => Guard.Required(value, "name", 10));

        Assert.Equal("name", error.ParamName);
    }

    [Fact]
    public void Required_rejects_values_over_the_limit()
    {
        Assert.Throws<ArgumentException>(() => Guard.Required("too long", "name", 3));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void Optional_turns_blank_values_into_null(string? value)
    {
        Assert.Null(Guard.Optional(value, "name", 10));
    }

    [Fact]
    public void Optional_trims_and_checks_the_limit()
    {
        Assert.Equal("ok", Guard.Optional(" ok ", "name", 10));
        Assert.Throws<ArgumentException>(() => Guard.Optional("too long", "name", 3));
    }

    [Fact]
    public void Email_is_trimmed_and_lowercased()
    {
        Assert.Equal("claire.morel@example.com", Guard.Email("  Claire.Morel@Example.com ", "email", 100));
    }

    [Theory]
    [InlineData("no-at-sign.example.com")]
    [InlineData("@example.com")]
    [InlineData("claire@")]
    [InlineData("claire@example")]
    [InlineData("claire@.example.com")]
    [InlineData("claire@example.com.")]
    [InlineData("claire@@example.com")]
    [InlineData("cla ire@example.com")]
    public void Email_rejects_invalid_addresses(string value)
    {
        var error = Assert.Throws<ArgumentException>(() => Guard.Email(value, "email", 100));

        Assert.Equal("email", error.ParamName);
    }

    [Fact]
    public void NotEmpty_rejects_an_empty_id()
    {
        var id = Guid.NewGuid();

        Assert.Equal(id, Guard.NotEmpty(id, "id"));
        Assert.Throws<ArgumentException>(() => Guard.NotEmpty(Guid.Empty, "id"));
    }

    [Fact]
    public void Utc_rejects_local_and_unspecified_dates()
    {
        Assert.Equal(TestDates.Now, Guard.Utc(TestDates.Now, "date"));
        Assert.Throws<ArgumentException>(() => Guard.Utc(DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local), "date"));
        Assert.Throws<ArgumentException>(() => Guard.Utc(DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Unspecified), "date"));
    }

    [Fact]
    public void Defined_rejects_unknown_enum_values()
    {
        Assert.Equal(DayOfWeek.Monday, Guard.Defined(DayOfWeek.Monday, "day"));
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Defined((DayOfWeek)42, "day"));
    }
}
