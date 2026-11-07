using Daab.Modules.Auth.Models;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Auth.Features.Invitations.AcceptInvitation;

/// <summary>Sets the invited user's password and consumes the invitation.</summary>
public sealed record AcceptInvitationCommand(string Token, string Password, string ConfirmPassword)
    : IRequest<Fin<User>>;

/// <summary>Looks up the account an invitation link is for, without consuming it.</summary>
public sealed record GetInvitationQuery(string Token) : IRequest<Fin<User>>;
