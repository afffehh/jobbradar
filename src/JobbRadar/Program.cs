using JobbRadar.Services;

using var http = new HttpClient { BaseAddress = new Uri("https://jobsearch.api.jobtechdev.se/") };
http.DefaultRequestHeaders.Add("Accept", "application/json");

var client = new JobTechClient(http);
var filter = new JobFilter(new FilterOptions(
     ["senior", "erfaren", "lead", "staff", "principal", "arkitekt", "architect", "java"], MaxYearsRequired: 3));

var ads = await client.SearchAsync(".NET", JobTechClient.StockholmRegion, limit: 100);
var relevant = filter.Apply(ads);

Console.WriteLine($"{relevant.Count} av {ads.Count} annonser passar:\n");
foreach (var ad in relevant)
{
    Console.WriteLine($"{ad.Headline} – {ad.Employer} ({ad.Municipality}), sista dag {ad.ApplicationDeadline:yyyy-MM-dd}");
    Console.WriteLine($"  {ad.Url}");
}