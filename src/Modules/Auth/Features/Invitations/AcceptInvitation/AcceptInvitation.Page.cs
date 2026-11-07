using Daab.Modules.Auth.Common;

namespace Daab.Modules.Auth.Features.Invitations.AcceptInvitation;

/// <summary>HTML for the invitation pages, in the same style as the sign-in page.</summary>
internal static class AcceptInvitationPage
{
    public const int MinPasswordLength = 8;

    public static string Form(string token, string email, string? error = null) =>
        AuthPages.Layout(
            "Set your password",
            $"""
            {AuthPages.Header(
                "Set your password",
                "You have been invited to sign in. Choose a password to activate your account."
            )}
            {AuthPages.ErrorBanner(error)}
            <form method="post" action="/auth/invitations/{Uri.EscapeDataString(token)}" novalidate>
              <div class="field">
                <label for="email">Email</label>
                <input id="email" type="text" value="{AuthPages.Encode(email)}" autocomplete="username" readonly />
              </div>
              <div class="field">
                <label for="password">Password</label>
                <input id="password" type="password" name="password" autocomplete="new-password" required />
                <p class="hint">At least {MinPasswordLength} characters, including a letter and a digit.</p>
              </div>
              <div class="field">
                <label for="confirmPassword">Confirm password</label>
                <input id="confirmPassword" type="password" name="confirmPassword" autocomplete="new-password" required />
              </div>
              <div class="form-footer">
                <button type="submit">Activate account</button>
              </div>
            </form>
            """
        );

    public static string Invalid() =>
        AuthPages.Layout(
            "Invitation unavailable",
            $"""
            {AuthPages.Header(
                "Invitation unavailable",
                "This invitation link is invalid, has expired or has already been used."
            )}
            <p class="notice">Ask an administrator to send you a new invitation.</p>
            """
        );

    public static string Done(string email, string? continueUrl) =>
        AuthPages.Layout(
            "Account activated",
            $"""
            {AuthPages.Header("Account activated", "Your password has been set.")}
            <p class="notice">From now on, sign in as <strong>{AuthPages.Encode(email)}</strong>.</p>
            {(
                string.IsNullOrWhiteSpace(continueUrl)
                    ? string.Empty
                    : $"""<a class="button" href="{AuthPages.Encode(continueUrl)}">Continue to the site</a>"""
            )}
            """
        );
}
