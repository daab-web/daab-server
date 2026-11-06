using Daab.SharedKernel;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Auth.Features.Users.GetUsers;

public sealed record GetUsersQuery(string? Search, int Page, int PageSize)
    : IRequest<Fin<PagedResponse<UserSummaryResponse>>>;
