using JobbRadar.Services;

namespace JobbRadar.Settings;

public class JobbRadarSettings
{
    public SearchSettings Search { get; init; } = new();
    public FilterOptions Filter { get; init; } = new();
}

public class SearchSettings
{
    public string Query { get; init; } = ".NET";
    public string Region { get; init; } = JobTechClient.StockholmRegion;
    public int Limit { get; init; } = 100;
}