using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Auth.Features.RefreshToken;

public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken);

public sealed class RefreshTokenEndpoint(IMediator mediator, IOptions<JwtOptions> options)
    : EndpointWithoutRequest
{
    private readonly JwtOptions _options = options.Value;

    public override void Configure()
    {
        Post("/auth/refresh-token");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var refreshToken = HttpContext.Request.Cookies["daab.refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var res = await mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken }, ct);

        await res.Match(
            async success =>
            {
                var (accessToken, refreshToken) = success;
                HttpContext.Response.Cookies.Append(
                    "daab.accessToken",
                    accessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddSeconds(_options.ExpiresMinutes),
                    }
                );
                HttpContext.Response.Cookies.Append(
                    "daab.refreshToken",
                    refreshToken.Token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = refreshToken.ExpiresAt,
                    }
                );

                await Send.OkAsync(new { AccessToken = accessToken }, ct);
            },
            async error =>
                await Send.ResultAsync(TypedResults.Problem(error.ToProblemDetails(HttpContext)))
        );
    }
}

public sealed class RefreshTokenCommand
    : IRequest<Fin<(string AccessToken, Models.RefreshToken RefreshToken)>>
{
    public required string RefreshToken { get; init; }
}

public sealed class RefreshTokenCommandHandler(AuthDbContext context, ITokenService tokenService)
    : IRequestHandler<
        RefreshTokenCommand,
        Fin<(string AccessToken, Models.RefreshToken RefreshToken)>
    >
{
    public async Task<Fin<(string AccessToken, Models.RefreshToken RefreshToken)>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        var refreshToken = await context
            .RefreshTokens.Include(rt => rt.User)
                .ThenInclude(u => u.Roles)
            .SingleOrDefaultAsync(
                rt => rt.Token == request.RefreshToken,
                cancellationToken: cancellationToken
            );

        var validationError = IsValidRefreshToken(refreshToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var newRefreshToken = tokenService.GenerateRefreshToken(refreshToken!.UserId);
        var newAccessToken = tokenService.GenerateAccessToken(refreshToken.User);

        refreshToken.Revoke();
        refreshToken.Replace(newRefreshToken.Token);

        await context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        context.RefreshTokens.Update(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return (newAccessToken, newRefreshToken);
    }

    private static Error? IsValidRefreshToken(Models.RefreshToken? refreshToken)
    {
        return refreshToken switch
        {
            null => Error.New("Invalid refresh token"),

            // TODO: Maybe I need to revoke entire token family instead of just the current token
            { IsRevoked: true } => Error.New("Refresh token has been revoked"),

            { ExpiresAt: var expiresAt } when expiresAt < DateTimeOffset.UtcNow => Error.New(
                "Refresh token has expired"
            ),
            _ => null,
        };
    }
}
