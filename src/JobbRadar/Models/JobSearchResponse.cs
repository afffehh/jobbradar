namespace JobbRadar.Models;

public record JobSearchResponse(TotalDto Total, List<HitDto> Hits);
public record TotalDto(int Value);

public record HitDto(string Id,
    string Headline,
    string WebpageUrl,
    DateTime? ApplicationDeadline,
    DateTime PublicationDate,
    DescriptionDto? Description,
    EmployerDto? Employer,
    WorkplaceAddressDto? WorkplaceAddress);

public record DescriptionDto(string? Text);
public record EmployerDto(string? Name);
public record WorkplaceAddressDto(string? Municipality, string? Region);