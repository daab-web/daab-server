using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Features.Invitations.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler(AuthDbContext context)
    : IRequestHandler<AcceptInvitationCommand, Fin<User>>,
        IRequestHandler<GetInvitationQuery, Fin<User>>
{
    public static Error InvalidInvitation { get; } =
        Error.New(StatusCodes.Status404NotFound, "Invitation is invalid or has expired");

    public async Task<Fin<User>> Handle(
        GetInvitationQuery request,
        CancellationToken cancellationToken
    )
    {
        var invitation = await FindUsableAsync(request.Token, cancellationToken);
        return invitation is null ? InvalidInvitation : invitation.User;
    }

    public async Task<Fin<User>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken
    )
    {
        var invitation = await FindUsableAsync(request.Token, cancellationToken);
        if (invitation is null)
        {
            return InvalidInvitation;
        }

        if (ValidatePassword(request.Password, request.ConfirmPassword) is { } problem)
        {
            return Error.New(StatusCodes.Status400BadRequest, problem);
        }

        invitation.User.SetPassword(request.Password);
        invitation.Accept();
        await context.SaveChangesAsync(cancellationToken);

        return invitation.User;
    }

    public static string? ValidatePassword(string? password, string? confirmPassword)
    {
        if (
            string.IsNullOrEmpty(password)
            || password.Length < AcceptInvitationPage.MinPasswordLength
        )
        {
            return $"Password must be at least {AcceptInvitationPage.MinPasswordLength} characters long";
        }

        if (password.Length > 128)
        {
            return "Password must be at most 128 characters long";
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            return "Password must contain a letter and a digit";
        }

        return password == confirmPassword ? null : "Passwords do not match";
    }

    private async Task<Invitation?> FindUsableAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var hash = Invitation.Hash(token);
        var invitation = await context
            .Invitations.Include(i => i.User)
                .ThenInclude(u => u.Roles)
            .SingleOrDefaultAsync(i => i.TokenHash == hash, ct);

        return invitation is not null && invitation.IsUsable(DateTime.UtcNow) ? invitation : null;
    }
}
