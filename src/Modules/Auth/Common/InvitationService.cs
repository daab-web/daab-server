using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Persistence;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Common;

/// <param name="UserId">An account already associated with the invitee, if any.</param>
public sealed record InviteRequest(string Email, string FullName, string? UserId);

/// <param name="Token">
/// The raw one-time token to put in the link; <c>null</c> when the account already has a
/// password (nothing to accept — the user can sign in as they are).
/// </param>
public sealed record InviteResult(string UserId, string Email, string? Token, DateTime? ExpiresAt)
{
    public bool AlreadyActive => Token is null;
}

/// <summary>
/// Creates accounts for people invited by an administrator. Used by the host to invite
/// scientists; the Auth module does not know what the account is for.
/// </summary>
public interface IInvitationService
{
    Task<Fin<InviteResult>> InviteAsync(InviteRequest request, CancellationToken cancellationToken);
}

internal sealed class InvitationService(AuthDbContext context) : IInvitationService
{
    public async Task<Fin<InviteResult>> InviteAsync(
        InviteRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = User.NormalizeEmail(request.Email);

        User? user = null;
        if (request.UserId is not null)
        {
            user = await context.Users.SingleOrDefaultAsync(
                u => u.Id == request.UserId,
                cancellationToken
            );
        }

        user ??= await context.Users.SingleOrDefaultAsync(
            u => u.Username == email || u.Email == email,
            cancellationToken
        );

        if (user is { HasPassword: true })
        {
            return new InviteResult(user.Id, user.Email ?? user.Username, null, null);
        }

        if (user is null)
        {
            user = User.Invite(email, request.FullName);
            var userRole = await context.Roles.SingleOrDefaultAsync(
                r => r.Name == AuthRoles.User,
                cancellationToken
            );
            if (userRole is not null)
            {
                user.Roles.Add(userRole);
            }

            context.Users.Add(user);
        }

        // Only the newest link works: re-inviting replaces earlier ones.
        var previous = await context
            .Invitations.Where(i =>
                i.UserId == user.Id && i.AcceptedAt == null && i.RevokedAt == null
            )
            .ToListAsync(cancellationToken);
        previous.ForEach(i => i.Revoke());

        var (invitation, token) = Invitation.Issue(user.Id);
        context.Invitations.Add(invitation);

        var written = await context.SaveChangesAsync(cancellationToken);

        return written <= 0
            ? Error.New(
                StatusCodes.Status500InternalServerError,
                "Unable to create the invitation. Please try again"
            )
            : new InviteResult(user.Id, user.Email ?? email, token, invitation.ExpiresAt);
    }
}
