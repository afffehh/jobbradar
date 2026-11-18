using System.Net.Http.Json;
using System.Text.Json;
using JobbRadar.Models;

namespace JobbRadar.Services;

public class JobTechClient(HttpClient http)
{
    public const string StockholmRegion = "CifL_Rzy_Mku";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<IReadOnlyList<JobAd>> SearchAsync(
        string query, string region, int limit = 100, CancellationToken ct = default)
    {
        var url = $"search?q={Uri.EscapeDataString(query)}&region={region}&limit={limit}";
        var response = await http.GetFromJsonAsync<JobSearchResponse>(url, JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty response from JobTech API");

        return response.Hits.Select(ToJobAd).ToList();
    }

    public static JobAd ToJobAd(HitDto hit) => new(
        hit.Id,
        hit.Headline,
        hit.Employer?.Name ?? "Okänd arbetsgivare",
        hit.WorkplaceAddress?.Municipality,
        hit.ApplicationDeadline,
        hit.PublicationDate,
        hit.WebpageUrl,
        hit.Description?.Text ?? "");
}