using System.Web;
using FastEndpoints;

namespace Daab.Modules.Auth.Features.Login;

public class LoginPageEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var returnUrl = Query<string>("returnUrl", isRequired: false) ?? "/";

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
              </style>
            </head>
            <body>
              <div class="card">
                <div class="card-header">
                  <h1 class="card-title">Sign in</h1>
                  <p class="card-description">Enter your credentials to continue</p>
                </div>

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
                  </div>
                </form>
              </div>
            </body>
            </html>
            """;

        await Send.StringAsync(html, contentType: "text/html", cancellation: ct);
    }
}
