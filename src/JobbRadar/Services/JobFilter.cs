using System.Text.RegularExpressions;
using JobbRadar.Models;

namespace JobbRadar.Services;

public record FilterOptions(IReadOnlyList<string> ExcludedHeadlineWords, int MaxYearsRequired);

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

    public IReadOnlyList<JobAd> Apply(IEnumerable<JobAd> ads) => ads.Where(IsRelevant).ToList();

    public bool IsRelevant(JobAd ad) =>
        !HasExcludedHeadlineWord(ad.Headline) &&
        MaxYearsRequired(ad.Description) <= options.MaxYearsRequired;

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