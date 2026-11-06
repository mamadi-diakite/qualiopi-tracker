namespace QualiopiTracker.Domain.Tests;

public sealed class QualityFrameworkTests
{
    [Fact]
    public void Has_seven_criteria_numbered_in_order()
    {
        Assert.Equal(Enumerable.Range(1, 7), QualityFramework.Criteria.Select(c => c.Number));
    }

    [Fact]
    public void Has_thirty_two_indicators_numbered_in_order()
    {
        Assert.Equal(Enumerable.Range(1, 32), QualityFramework.Indicators.Select(i => i.Number));
    }

    [Fact]
    public void Indicators_are_spread_over_the_criteria()
    {
        var counts = QualityFramework.Criteria
            .Select(c => QualityFramework.Indicators.Count(i => i.CriterionNumber == c.Number));

        Assert.Equal([3, 5, 8, 4, 2, 7, 3], counts);
    }

    [Fact]
    public void Criterion_numbers_never_go_back()
    {
        var criterionNumbers = QualityFramework.Indicators.Select(i => i.CriterionNumber).ToList();

        Assert.Equal(criterionNumbers.Order(), criterionNumbers);
    }

    [Fact]
    public void Titles_are_short_and_unique()
    {
        var titles = QualityFramework.Criteria.Select(c => c.Title)
            .Concat(QualityFramework.Indicators.Select(i => i.Title)).ToList();

        Assert.All(titles, t => Assert.InRange(t.Length, 1, 40));
        Assert.Equal(QualityFramework.Indicators.Count, QualityFramework.Indicators.Select(i => i.Title).Distinct().Count());
        Assert.Equal(QualityFramework.Criteria.Count, QualityFramework.Criteria.Select(c => c.Title).Distinct().Count());
    }

    [Fact]
    public void Every_indicator_applies_to_known_categories()
    {
        Assert.All(QualityFramework.Indicators, i =>
        {
            Assert.NotEqual(ActivityCategories.None, i.AppliesTo);
            Assert.Equal(ActivityCategories.None, i.AppliesTo & ~ActivityCategories.All);
        });
    }

    [Fact]
    public void Some_indicators_are_for_apprenticeship_only()
    {
        Assert.Equal(
            [14, 15, 20, 29],
            QualityFramework.Indicators.Where(i => i.AppliesTo == ActivityCategories.Apprenticeship).Select(i => i.Number));
    }

    [Fact]
    public void Finds_an_indicator_by_number()
    {
        Assert.Equal("Subcontracting", QualityFramework.FindIndicator(27)?.Title);
        Assert.Equal(6, QualityFramework.FindIndicator(27)?.CriterionNumber);
        Assert.Null(QualityFramework.FindIndicator(0));
        Assert.Null(QualityFramework.FindIndicator(33));
    }
}
