using Daab.Modules.Auth.Models;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Auth.Features.ExternalLogin;

/// <summary>
/// Signs in with an identity confirmed by an external provider: finds the account it belongs to,
/// or creates one from the provider's profile.
/// </summary>
/// <param name="ProviderKey">The provider's stable account id (Google's <c>sub</c>).</param>
/// <param name="EmailVerified">
/// Whether the provider vouches for the e-mail. Only a verified e-mail may be attached to an
/// existing account.
/// </param>
public sealed record ExternalSignInCommand(
    string Provider,
    string ProviderKey,
    string? Email,
    bool EmailVerified,
    string? FullName,
    string? PictureUrl
) : IRequest<Fin<User>>;
