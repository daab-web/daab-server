using FastEndpoints;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public sealed record GetAllScientistsRequest(
    string? Country,
    int PageNumber = 1,
    int PageSize = 20
);
