using JobbRadar.Services;

using var http = new HttpClient { BaseAddress = new Uri("https://jobsearch.api.jobtechdev.se/") };
http.DefaultRequestHeaders.Add("Accept", "application/json");

var client = new JobTechClient(http);
var ads = await client.SearchAsync(".NET", JobTechClient.StockholmRegion, limit: 10);

foreach (var ad in ads)
{
    Console.WriteLine($"{ad.Headline} – {ad.Employer} ({ad.Municipality}), sista dag {ad.ApplicationDeadline:yyyy-MM-dd}");
    Console.WriteLine($"  {ad.Url}");
}