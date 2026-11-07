using Daab.Modules.Auth.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Features.Users.GetUsers;

public sealed class GetUsersQueryHandler(AuthDbContext context)
    : IRequestHandler<GetUsersQuery, Fin<PagedResponse<UserSummaryResponse>>>
{
    public async Task<Fin<PagedResponse<UserSummaryResponse>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term)
                || (u.Email != null && u.Email.ToLower().Contains(term))
                || (u.FullName != null && u.FullName.ToLower().Contains(term))
            );
        }

        var total = await query.CountAsync(cancellationToken);

        var users = await query
            .Include(u => u.Roles)
            .Include(u => u.ExternalLogins)
            .OrderByDescending(u => u.CreatedAt)
            .ThenBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = users.ConvertAll(u => new UserSummaryResponse(
            u.Id,
            u.Username,
            u.Email,
            u.DisplayName,
            u.PictureUrl,
            [.. u.Roles.Select(r => r.Name).Order(StringComparer.Ordinal)],
            u.HasPassword,
            [.. u.ExternalLogins.Select(l => l.Provider).Distinct()],
            u.CreatedAt
        ));

        return PagedResponse<UserSummaryResponse>.Create(items, page, pageSize, total);
    }
}
