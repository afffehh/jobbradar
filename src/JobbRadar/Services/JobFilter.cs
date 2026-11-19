using System.Text.RegularExpressions;
using JobbRadar.Models;

namespace JobbRadar.Services;

public class FilterOptions
{
    public List<string> ExcludedHeadlineWords { get; init; } = [];
    public List<string> RequiredHeadlineWords { get; init; } = [];
    public int MaxYearsRequired { get; init; } = 3;
}

public partial class JobFilter(FilterOptions options)
{
    private static readonly string[] SafeWords = ["javascript"];

    private static readonly Dictionary<string, int> NumberWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ett"] = 1,
        ["en"] = 1,
        ["one"] = 1,
        ["två"] = 2,
        ["two"] = 2,
        ["tre"] = 3,
        ["three"] = 3,
        ["fyra"] = 4,
        ["four"] = 4,
        ["fem"] = 5,
        ["five"] = 5,
        ["sex"] = 6,
        ["six"] = 6,
        ["sju"] = 7,
        ["seven"] = 7,
        ["åtta"] = 8,
        ["eight"] = 8,
        ["nio"] = 9,
        ["nine"] = 9,
        ["tio"] = 10,
        ["ten"] = 10
    };

    // Matchar t.ex. "5 års erfarenhet", "fem års erfarenhet", "five years of", "3+ years of"
    [GeneratedRegex(@"\b(\d{1,2}|ett|en|one|två|two|tre|three|fyra|four|fem|five|sex|six|sju|seven|åtta|eight|nio|nine|tio|ten)\s*\+?\s*(?:års|år|years?)\s+(?:erfarenhet|experience|of|av|inom|med)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex YearsRegex();

    public IReadOnlyList<JobAd> Apply(IEnumerable<JobAd> ads) => Apply(ads, DateTime.Today);

    public IReadOnlyList<JobAd> Apply(IEnumerable<JobAd> ads, DateTime today) =>
        ads.Where(ad => IsRelevant(ad, today)).ToList();

    public bool IsRelevant(JobAd ad) => IsRelevant(ad, DateTime.Today);

    public bool IsRelevant(JobAd ad, DateTime today) =>
        HasRequiredHeadlineWord(ad.Headline) &&
        !HasExcludedHeadlineWord(ad.Headline) &&
        MaxYearsRequired(ad.Description) <= options.MaxYearsRequired &&
        (ad.ApplicationDeadline is null || ad.ApplicationDeadline.Value.Date >= today.Date);

    private bool HasRequiredHeadlineWord(string headline) =>
        options.RequiredHeadlineWords.Count == 0 ||
        options.RequiredHeadlineWords.Any(w => headline.Contains(w, StringComparison.OrdinalIgnoreCase));

    private bool HasExcludedHeadlineWord(string headline)
    {
        var text = headline.ToLowerInvariant();
        foreach (var safe in SafeWords)
            text = text.Replace(safe, "");

        return options.ExcludedHeadlineWords.Any(word => text.Contains(word.ToLowerInvariant()));
    }

    public static int MaxYearsRequired(string text) =>
        YearsRegex().Matches(text)
            .Select(m => m.Groups[1].Value)
            .Select(v => int.TryParse(v, out var n) ? n : NumberWords[v])
            .DefaultIfEmpty(0)
            .Max();
}