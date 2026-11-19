namespace JobbRadar.Models;

public record JobAd(
    string Id,
    string Headline,
    string Employer,
    string? Municipality,
    DateTime? ApplicationDeadline,
    DateTime PublicationDate,
    string Url,
    string Description);