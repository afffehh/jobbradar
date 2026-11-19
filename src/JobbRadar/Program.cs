using JobbRadar.Services;
using JobbRadar.Settings;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var settings = config.Get<JobbRadarSettings>() ?? new JobbRadarSettings();

using var http = new HttpClient { BaseAddress = new Uri("https://jobsearch.api.jobtechdev.se/") };
http.DefaultRequestHeaders.Add("Accept", "application/json");

var client = new JobTechClient(http);
var filter = new JobFilter(settings.Filter);

var ads = await client.SearchAsync(settings.Search.Query, settings.Search.Region, settings.Search.Limit);
var relevant = filter.Apply(ads, DateTime.Today);

Console.WriteLine($"{relevant.Count} av {ads.Count} annonser passar:\n");
foreach (var ad in relevant)
{
    Console.WriteLine($"{ad.Headline} – {ad.Employer} ({ad.Municipality}), sista dag {ad.ApplicationDeadline:yyyy-MM-dd}");
    Console.WriteLine($"  {ad.Url}");
}