using JobbRadar.Models;
using JobbRadar.Services;

namespace JobbRadar.Tests;

public class EmailSenderTests
{
    [Fact]
    public void BuildHtmlContainsAdsAndEncodesSpecialCharacters()
    {
        var ads = new List<JobAd>
        {
            new("1", "Backend Developer <C#>", "Synsam & Co", "Stockholm",
                new DateTime(2026, 12, 6), DateTime.Now, "https://example.com/1", "")
        };

        var html = EmailSender.BuildHtml(ads);

        Assert.Contains("1 nya .NET-jobb", html);
        Assert.Contains("Backend Developer &lt;C#&gt;", html);
        Assert.Contains("Synsam &amp; Co", html);
        Assert.Contains("2026-12-06", html);
        Assert.Contains("https://example.com/1", html);
    }
}