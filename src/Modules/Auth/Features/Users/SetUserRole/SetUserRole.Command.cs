using LanguageExt;
using MediatR;

namespace Daab.Modules.Auth.Features.Users.SetUserRole;

/// <summary>Grants (<paramref name="Granted"/> = true) or removes one assignable role.</summary>
public sealed record SetUserRoleCommand(string UserId, string Role, bool Granted)
    : IRequest<Fin<SetUserRoleResponse>>;

public sealed record SetUserRoleResponse(string UserId, IReadOnlyList<string> Roles);
