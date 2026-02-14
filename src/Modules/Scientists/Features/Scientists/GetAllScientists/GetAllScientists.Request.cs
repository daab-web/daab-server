namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public sealed record GetAllScientistsRequest(
    string? Country,
    int Page = 1,
    int PageSize = 20
);