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

Console.WriteLine($"{newAds.Count} nya av {relevant.Count} relevanta ({ads.Count} totalt).");

if (newAds.Count == 0)
{
    Console.WriteLine("Inga nya annonser – inget mejl skickas.");
    return;
}

var email = new EmailSettings
{
    Host = config["Email:Host"] ?? "smtp.gmail.com",
    Port = int.TryParse(config["Email:Port"], out var port) ? port : 587,
    User = config["SMTP_USER"] ?? "",
    Password = config["SMTP_PASSWORD"] ?? "",
    To = config["MAIL_TO"] ?? ""
};

if (!email.IsConfigured)
{
    Console.WriteLine("SMTP_USER, SMTP_PASSWORD eller MAIL_TO saknas – visar bara i terminalen:\n");
    foreach (var ad in newAds)
        Console.WriteLine($"{ad.Headline} – {ad.Employer}\n  {ad.Url}");
    return;
}

await new EmailSender(email).SendAsync(newAds);
Console.WriteLine($"Mejl skickat till {email.To}.");

// Spara först när mejlet har gått iväg, så att inga annonser tappas bort
seen.UnionWith(newAds.Select(a => a.Id));
store.Save(seen);