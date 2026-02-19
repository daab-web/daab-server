namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public sealed record GetAllScientistsRequest(
    string? Search,
    string? Country,
    string? Area,
    int Page = 1,
    int PageSize = 20
);
