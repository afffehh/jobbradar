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

var store = new SeenStore(settings.SeenFile);
var seen = store.Load();
var newAds = relevant.Where(ad => !seen.Contains(ad.Id)).ToList();

Console.WriteLine($"{newAds.Count} nya av {relevant.Count} relevanta ({ads.Count} totalt):\n");
foreach (var ad in newAds)
{
    Console.WriteLine($"{ad.Headline} – {ad.Employer} ({ad.Municipality}), sista dag {ad.ApplicationDeadline:yyyy-MM-dd}");
    Console.WriteLine($"  {ad.Url}");
}

// När mejlet finns: spara först efter att mejlet har skickats, så att inga annonser tappas bort
seen.UnionWith(newAds.Select(a => a.Id));
store.Save(seen);