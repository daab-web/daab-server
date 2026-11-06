using System.Web;
using Daab.Modules.Auth.Options;
using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Auth.Features.Login;

public class LoginPageEndpoint(IOptions<GoogleOptions> google) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var returnUrl = Query<string>("returnUrl", isRequired: false) ?? "/";

        var errorMessage = Query<string>("error", isRequired: false) switch
        {
            "google" => "Google sign-in did not complete. Please try again.",
            "google-existing" =>
                "An account with this e-mail already exists. Sign in with your password.",
            _ => null,
        };
        var errorBanner = errorMessage is null
            ? string.Empty
            : $"""<div class="error-banner" role="alert">{HttpUtility.HtmlEncode(errorMessage)}</div>""";

        var googleButton = google.Value.IsConfigured
            ? $"""
                <div class="divider">or</div>
                <a class="google-button" href="/auth/external/google?returnUrl={HttpUtility.UrlEncode(
                    returnUrl
                )}">
                  <svg width="16" height="16" viewBox="0 0 48 48" aria-hidden="true"><path fill="#FFC107" d="M43.6 20.5H42V20H24v8h11.3C33.7 32.7 29.2 36 24 36c-6.6 0-12-5.4-12-12s5.4-12 12-12c3.1 0 5.9 1.2 8 3.1l5.7-5.7C34 6.1 29.3 4 24 4 12.9 4 4 12.9 4 24s8.9 20 20 20 20-8.9 20-20c0-1.3-.1-2.4-.4-3.5z"/><path fill="#FF3D00" d="M6.3 14.7l6.6 4.8C14.7 15.1 19 12 24 12c3.1 0 5.9 1.2 8 3.1l5.7-5.7C34 6.1 29.3 4 24 4 16.3 4 9.7 8.3 6.3 14.7z"/><path fill="#4CAF50" d="M24 44c5.2 0 9.9-2 13.4-5.2l-6.2-5.2C29.2 35.1 26.7 36 24 36c-5.2 0-9.6-3.3-11.3-8l-6.5 5C9.5 39.6 16.2 44 24 44z"/><path fill="#1976D2" d="M43.6 20.5H42V20H24v8h11.3c-.8 2.2-2.2 4.2-4.1 5.6l6.2 5.2C37 39.2 44 34 44 24c0-1.3-.1-2.4-.4-3.5z"/></svg>
                  Continue with Google
                </a>
                """
            : string.Empty;

        var html = $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Sign In</title>
              <link rel="preconnect" href="https://fonts.googleapis.com" />
              <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
              <link href="https://fonts.googleapis.com/css2?family=Geist:wght@400;500;600&display=swap" rel="stylesheet" />
              <style>
                *, *::before, *::after {
                  box-sizing: border-box;
                  margin: 0;
                  padding: 0;
                }

                :root {
                  --bg:          #09090b;
                  --surface:     #18181b;
                  --border:      #27272a;
                  --border-focus:#52525b;
                  --text:        #fafafa;
                  --muted:       #71717a;
                  --accent:      #fafafa;
                  --accent-fg:   #09090b;
                  --ring:        rgba(250,250,250,.15);
                  --radius:      6px;
                  --font:        'Geist', ui-sans-serif, system-ui, sans-serif;
                }

                html, body {
                  height: 100%;
                  background: var(--bg);
                  color: var(--text);
                  font-family: var(--font);
                  font-size: 18px;
                  line-height: 1.5;
                  -webkit-font-smoothing: antialiased;
                }

                body {
                  display: flex;
                  align-items: center;
                  justify-content: center;
                  min-height: 100vh;
                  padding: 1rem;
                }

                .card {
                  width: 100%;
                  max-width: 360px;
                  background: var(--surface);
                  border: 1px solid var(--border);
                  border-radius: calc(var(--radius) + 2px);
                  padding: 1.75rem;
                  animation: card-in 0.2s ease both;
                }

                @keyframes card-in {
                  from { opacity: 0; transform: translateY(6px); }
                  to   { opacity: 1; transform: translateY(0); }
                }

                .card-header {
                  margin-bottom: 1.5rem;
                }

                .card-title {
                  font-size: 1.125rem;
                  font-weight: 600;
                  letter-spacing: -0.01em;
                  color: var(--text);
                }

                .card-description {
                  margin-top: 0.25rem;
                  font-size: 0.8125rem;
                  color: var(--muted);
                }

                .field {
                  display: flex;
                  flex-direction: column;
                  gap: 0.375rem;
                  margin-bottom: 1rem;
                }

                .field:last-of-type {
                  margin-bottom: 0;
                }

                label {
                  font-size: 0.8125rem;
                  font-weight: 500;
                  color: var(--text);
                }

                input[type="text"],
                input[type="password"] {
                  width: 100%;
                  height: 36px;
                  padding: 0 0.75rem;
                  background: transparent;
                  border: 1px solid var(--border);
                  border-radius: var(--radius);
                  color: var(--text);
                  font-family: var(--font);
                  font-size: 0.875rem;
                  outline: none;
                  transition: border-color 0.15s, box-shadow 0.15s;
                }

                input[type="text"]::placeholder,
                input[type="password"]::placeholder {
                  color: var(--muted);
                }

                input[type="text"]:focus,
                input[type="password"]:focus {
                  border-color: var(--border-focus);
                  box-shadow: 0 0 0 3px var(--ring);
                }

                .form-footer {
                  margin-top: 1.25rem;
                  display: flex;
                  flex-direction: column;
                  gap: 0.75rem;
                }

                button[type="submit"] {
                  width: 100%;
                  height: 36px;
                  background: var(--accent);
                  color: var(--accent-fg);
                  border: none;
                  border-radius: var(--radius);
                  font-family: var(--font);
                  font-size: 0.875rem;
                  font-weight: 500;
                  cursor: pointer;
                  transition: opacity 0.15s;
                  letter-spacing: -0.005em;
                }

                button[type="submit"]:hover  { opacity: 0.88; }
                button[type="submit"]:active { opacity: 0.78; }

                .divider {
                  display: flex;
                  align-items: center;
                  gap: 0.75rem;
                  color: var(--muted);
                  font-size: 0.75rem;
                }

                .divider::before,
                .divider::after {
                  content: '';
                  flex: 1;
                  height: 1px;
                  background: var(--border);
                }

                .error-banner {
                  display: flex;
                  align-items: center;
                  gap: 0.5rem;
                  padding: 0.625rem 0.75rem;
                  background: rgba(239,68,68,.08);
                  border: 1px solid rgba(239,68,68,.25);
                  border-radius: var(--radius);
                  color: #f87171;
                  font-size: 0.8125rem;
                  margin-bottom: 1.25rem;
                }

                .error-banner svg {
                  flex-shrink: 0;
                }

                .google-button {
                  display: flex;
                  align-items: center;
                  justify-content: center;
                  gap: 0.5rem;
                  width: 100%;
                  height: 36px;
                  background: transparent;
                  border: 1px solid var(--border);
                  border-radius: var(--radius);
                  color: var(--text);
                  font-size: 0.875rem;
                  font-weight: 500;
                  text-decoration: none;
                  transition: border-color 0.15s, background 0.15s;
                }

                .google-button:hover {
                  border-color: var(--border-focus);
                  background: rgba(250,250,250,.04);
                }
              </style>
            </head>
            <body>
              <div class="card">
                <div class="card-header">
                  <h1 class="card-title">Sign in</h1>
                  <p class="card-description">Enter your credentials to continue</p>
                </div>

                {{errorBanner}}

                <form method="post" action="/auth/login?returnUrl={{HttpUtility.UrlEncode(
                returnUrl
            )}}" novalidate>
                  <div class="field">
                    <label for="username">Username</label>
                    <input
                      id="username"
                      type="text"
                      name="username"
                      placeholder="you@example.com"
                      autocomplete="username"
                      required
                    />
                  </div>

                  <div class="field">
                    <label for="password">Password</label>
                    <input
                      id="password"
                      type="password"
                      name="password"
                      placeholder="••••••••"
                      autocomplete="current-password"
                      required
                    />
                  </div>

                  <div class="form-footer">
                    <button type="submit">Sign in</button>
                    {{googleButton}}
                  </div>
                </form>
              </div>
            </body>
            </html>
            """;

        await Send.StringAsync(html, contentType: "text/html", cancellation: ct);
    }
}
