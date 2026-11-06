using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Features.Users.SetUserRole;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace Daab.Web.Accounts;

/// <param name="Status">
/// <c>invited</c>: send <see cref="InviteUrl"/> to the scientist so they can set a password.
/// <c>active</c>: the scientist's account already has a password; they can sign in as they are.
/// </param>
public sealed record InviteScientistResponse(
    string Status,
    string Email,
    string? InviteUrl,
    DateTime? ExpiresAt
);

/// <summary>
/// Gives a scientist a sign-in account: creates (or reuses) the Auth account for the profile's
/// e-mail, links it to this exact profile, grants it the <c>scientist</c> role and returns a
/// one-time link for setting the password.
/// Lives in the host because it spans the Auth and Scientists modules.
/// </summary>
public sealed class InviteScientistEndpoint(
    ScientistsDbContext scientists,
    IInvitationService invitations,
    IMediator mediator,
    IConfiguration configuration
) : EndpointWithoutRequest<InviteScientistResponse>
{
    public override void Configure()
    {
        Post("/scientists/{id}/invitation");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Roles(AuthRoles.Admin);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var scientist = await scientists
            .Scientists.Include(s => s.Translations)
            .SingleOrDefaultAsync(s => s.Id == id, ct);

        if (scientist is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(scientist.Email))
        {
            ThrowError(
                "Add an e-mail address to the scientist's profile before inviting them",
                StatusCodes.Status400BadRequest
            );
        }

        var name =
            scientist
                .Translations.OrderBy(t => t.Locale == "en" ? 0 : 1)
                .Select(t => $"{t.FirstName} {t.LastName}".Trim())
                .FirstOrDefault(n => n.Length > 0)
            ?? scientist.Email!;

        var result = await invitations.InviteAsync(
            new InviteRequest(scientist.Email!, name, scientist.UserId),
            ct
        );

        await result.Match(
            async invite =>
            {
                if (invite.AlreadyActive && scientist.UserId != invite.UserId)
                {
                    // Someone already holds an account with this unverified e-mail. Linking it
                    // would hand them the scientist's author rights, so refuse instead.
                    await Send.ResultAsync(
                        TypedResults.Problem(
                            detail: "An account with this e-mail already exists and is not linked to this profile. It was not linked automatically.",
                            statusCode: StatusCodes.Status409Conflict
                        )
                    );
                    return;
                }

                if (scientist.UserId != invite.UserId)
                {
                    scientist.LinkUser(invite.UserId);
                    await scientists.SaveChangesAsync(ct);
                }

                var granted = await mediator.Send(
                    new SetUserRoleCommand(invite.UserId, AuthRoles.Scientist, Granted: true),
                    ct
                );
                if (granted.IsFail)
                {
                    await granted.Match(
                        _ => Task.CompletedTask,
                        e => e.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
                    );
                    return;
                }

                await Send.OkAsync(
                    invite.AlreadyActive
                        ? new InviteScientistResponse("active", invite.Email, null, null)
                        : new InviteScientistResponse(
                            "invited",
                            invite.Email,
                            $"{PublicBaseUrl()}/auth/invitations/{Uri.EscapeDataString(invite.Token!)}",
                            invite.ExpiresAt
                        ),
                    ct
                );
            },
            error => error.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }

    /// <summary>
    /// The link is opened by the scientist's browser, so it needs this server's public address.
    /// Calls arrive through the client's proxy, so the request host may be an internal one;
    /// set <c>Auth:PublicUrl</c> wherever that differs.
    /// </summary>
    private string PublicBaseUrl() =>
        configuration["Auth:PublicUrl"]?.TrimEnd('/')
        ?? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
}
