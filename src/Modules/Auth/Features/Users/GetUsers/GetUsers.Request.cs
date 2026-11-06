using FastEndpoints;

namespace Daab.Modules.Auth.Features.Users.GetUsers;

public sealed record GetUsersRequest(
    [property: QueryParam] string? Search = null,
    [property: QueryParam] int Page = 1,
    [property: QueryParam] int PageSize = 20
);
