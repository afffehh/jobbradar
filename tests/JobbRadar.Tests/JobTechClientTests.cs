using System.Text.Json;
using JobbRadar.Models;
using JobbRadar.Services;

namespace JobbRadar.Tests;

public class JobTechClientTests
{
    [Fact]
    public void ParsesSampleResponse()
    {
        var json = File.ReadAllText("TestData/sample.json");
        var response = JsonSerializer.Deserialize<JobSearchResponse>(json, JobTechClient.JsonOptions)!;

        Assert.Equal(204, response.Total.Value);

        var ad = JobTechClient.ToJobAd(response.Hits[0]);
        Assert.Equal(".NET Developer", ad.Headline);
        Assert.Equal("Rasulson Consulting AB", ad.Employer);
        Assert.Equal("Stockholm", ad.Municipality);
    }
}