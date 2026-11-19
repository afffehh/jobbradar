using System.Text.Json;
using JobbRadar.Models;
using JobbRadar.Services;

namespace JobbRadar.Tests;

public class JobFilterTests
{
    private static readonly JobFilter Filter = new(new FilterOptions
    {
        ExcludedHeadlineWords = ["senior", "erfaren", "lead", "staff", "principal", "arkitekt", "architect", "java"],
        MaxYearsRequired = 3
    });

    private static readonly JobFilter StrictFilter = new(new FilterOptions
    {
        RequiredHeadlineWords = ["utvecklare", "developer", "engineer"],
        MaxYearsRequired = 3
    });

    private static JobAd Ad(string headline, string description = "") =>
        new("1", headline, "Test AB", "Stockholm", null, DateTime.Now, "https://example.com", description);

    [Theory]
    [InlineData("Senior fullstackutvecklare .NET")]
    [InlineData("Erfaren .NET systemutvecklare")]
    [InlineData("Java developer")]
    [InlineData("Javautvecklare till bank")]
    [InlineData("Lösningsarkitekt")]
    [InlineData("Staff .NET Developer")]
    public void ExcludesUnwantedHeadlines(string headline) =>
    Assert.False(Filter.IsRelevant(Ad(headline)));

    [Fact]
    public void DoesNotExcludeJavaScriptWhenJavaIsExcluded() =>
        Assert.True(Filter.IsRelevant(Ad("Fullstack .NET och JavaScript")));

    [Theory]
    [InlineData("Minst fem års erfarenhet .NET", 5)]
    [InlineData("At least five years of software development in C#", 5)]
    [InlineData("Minst 3 års erfarenhet av C# .NET utveckling", 3)]
    [InlineData("3+ years of experience with .NET", 3)]
    [InlineData("Företaget har funnits i över 100 år.", 0)]
    [InlineData("Junior roll, inga krav på erfarenhet", 0)]
    public void ReadsYearsRequired(string text, int expected) =>
        Assert.Equal(expected, JobFilter.MaxYearsRequired(text));

    [Fact]
    public void KeepsOnlyRelevantAdsFromSample()
    {
        var json = File.ReadAllText("TestData/sample.json");
        var response = JsonSerializer.Deserialize<JobSearchResponse>(json, JobTechClient.JsonOptions)!;
        var ads = response.Hits.Select(JobTechClient.ToJobAd);

        var kept = Filter.Apply(ads, new DateTime(2026, 11, 18)).Select(a => a.Headline).ToList();

        Assert.Equal(["Junior .NET utvecklare", "C# .NET Utvecklare"], kept);
    }

    [Theory]
    [InlineData("2nd Line Support - Onboarding & Partner Specialist", false)]
    [InlineData("AI Adoption Specialist", false)]
    [InlineData("Fullstackutvecklare till Hallon", true)]
    [InlineData("Backend Developer", true)]
    public void RequiresDeveloperHeadline(string headline, bool expected) =>
    Assert.Equal(expected, StrictFilter.IsRelevant(Ad(headline)));

    [Fact]
    public void ExcludesAdsPastDeadline()
    {
        var today = new DateTime(2026, 11, 18);
        var expired = Ad("Backend Developer") with { ApplicationDeadline = new DateTime(2026, 11, 17) };
        var lastDay = Ad("Backend Developer") with { ApplicationDeadline = new DateTime(2026, 11, 18, 23, 59, 59) };

        Assert.False(StrictFilter.IsRelevant(expired, today));
        Assert.True(StrictFilter.IsRelevant(lastDay, today));
    }
}