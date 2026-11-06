# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
make run                     # dotnet run --project ./src/Daab.Web
make watch                   # dotnet watch (no hot reload)

dotnet build                 # build whole solution (Daab.slnx)
dotnet test                  # run all tests
dotnet test tests/Daab.Modules.Translation.Tests   # single test project
dotnet test --filter "FullyQualifiedName~TagValidationTests"   # single test class/method

dotnet csharpier check .     # verify formatting (CI gate)
dotnet csharpier format .    # apply formatting
```

- **Format is enforced.** CI fails on `csharpier check`; a Husky pre-commit hook (`dotnet husky run`) runs it locally. `Migrations/` is exempt (`.csharpierignore`).
- **Target: `net10.0`**, nullable enabled. Uses C# `extension` member blocks (`extension(IServiceCollection services) { ... }`) for DI registration — not classic `this`-parameter extension methods.

### Migrations
EF Core migrations, one `DbContext` per module. Add via Make targets:
```bash
make add-translation-migration name=AddFooTable   # -> src/Modules/Translation/Persistence/Migrations
```
Targets exist for `activities`, `scientists`, `auth`, `reference-data`, `translation`, `blog`. Each module calls `context.Database.Migrate()` at startup in its `UseXModule()` — migrations apply automatically on boot.

### Secrets
Gemini API key is not in config files — set via user-secrets in `Daab.Web`:
```bash
dotnet user-secrets set "Gemini:ApiKey" <key>
```
Module DI throws at startup if it is missing.

## Architecture

**Modular monolith.** Single ASP.NET Core host (`src/Daab.Web`) composes independent feature modules. Stack: FastEndpoints + MediatR (CQRS), EF Core / **SQLite (one DB file per module)**, LanguageExt for functional error handling, MinIO/S3 blob storage, Serilog, Scalar+Swagger OpenAPI.

### Projects
- `src/Daab.Web` — host. `Program.cs` wires every module in order: `AddXModule(config)` (services) then `UseXModule()` (middleware + auto-migrate).
- `src/Modules/*` — the modules: `Auth`, `Scientists`, `Activities`, `ReferenceData`, `Translation`, `Blog`. Each is a class library with a public `DependencyInjection.cs` exposing `AddXModule` / `UseXModule`.
- `src/Daab.SharedKernel` — cross-cutting contracts only: `IBlobStorage`, locale options/resolvers, `ChannelKeys`, `PagedResponse`, FastEndpoints pre-processors. Modules depend on it; **modules do not reference each other.**
- `src/Daab.Infrastructure` — concrete infra (`MinioStorageService`, `IAmazonS3`).
- `tests/*` — xUnit per module.

### Module internals — vertical slices
Each module is organized by feature under `Features/<Area>/<UseCase>/`. A slice is a set of co-located files sharing a dotted prefix, e.g.:
```
Features/Applications/Apply/
  Apply.Request.cs        # FastEndpoints request DTO (HTTP binding)
  Apply.Endpoint.cs       # FastEndpoints Endpoint<Req,Res>; thin, delegates to mediator
  Apply.Command.cs        # MediatR IRequest<Fin<Response>>
  Apply.CommandHandler.cs # business logic + DbContext
  Apply.Response.cs
  Apply.Extensions.cs     # DTO <-> entity mapping
```
Endpoints are thin: bind request → `mediator.Send(command)` → `result.Match(...)`. Handlers return **`LanguageExt.Fin<T>`** (`Error.New(statusCode, msg)` for failures); endpoints translate errors via `err.ToProblemDetails(HttpContext)`. Queries vs Commands are separated (see `GetAll*` / `Apply`).

### Cross-module & background work
No message bus. Async/background work uses **`System.Threading.Channels`**: a keyed unbounded `Channel<TMessage>` (keys in `SharedKernel/Constants/ChannelKeys.cs`) registered in module DI, drained by an `IHostedService` worker (e.g. `ProfilePictureUploadWorker`, Activities `BackgroundWorkers`).

When two modules must interact, the **host** bridges them, not the modules. Example: `Translation` defines `IGlossaryProvider` but never references `ReferenceData`; `Program.cs` registers `ReferenceDataGlossaryProvider` (in `Daab.Web/Translation/`) to connect them.

### Accounts & roles
Roles are stored in Auth (`Roles` table, seeded: `admin`, `user`, `scientist`) and travel in the access token as a JSON array (`RoleClaimType = "roles"`). Endpoints that need the bearer token must set `AuthSchemes("Bearer")` — the default scheme is the `cookie` one used by `/authorize`.
- `scientist` may write blog posts and, in the client's admin panel, sees only the Blogs tab. Admins grant/remove it with `PUT`/`DELETE /users/{id}/roles/{role}` (only roles in `AuthRoles.Assignable`); `GET /users` lists accounts. Role changes reach the user at their next sign-in.
- At token issuance Auth calls `IAccountLinker` (host: `Daab.Web/Accounts/ScientistAccountLinker`), which only adds the `scientist_id` claim when a Scientist profile's `UserId` points to the user (never by e-mail matching).
- Invitations: `POST /scientists/{id}/invitation` (admin, host endpoint `InviteScientistEndpoint`) uses `IInvitationService` to create a password-less account for the profile's e-mail, links the profile, grants `scientist`, and returns a one-time link to `GET/POST /auth/invitations/{token}` (server-rendered set-password page, 7-day expiry, only the token's SHA-256 is stored, re-inviting revokes older links). An existing account with a password that is not already the profile's is refused with 409.
- Google sign-in (enabled only when `Google:ClientId` + `Google:ClientSecret` are set; redirect URI `{backend}/signin-google`): "Continue with Google" on the sign-in page → Google → `external` cookie → `GET /auth/external/callback`, which resolves the account by `ExternalLogins` (provider + Google `sub`), else attaches to the account with the same e-mail only if Google marks it verified, else creates a `user` account from the Google name, e-mail and picture.
- `POST /auth/register` (no UI) creates a `user` account; `GET /auth/logout` ends the sign-in cookie session. `Auth:PublicUrl` overrides the backend address in invitation links; `Auth:AppUrl` is where "Continue to the site" goes after activation.

### Blog module
Scientists write posts (Lexical editor state, single language); any signed-in user may comment on published posts. A post is published only via `POST /blog/posts/{id}/submit`, which runs `ContentRules` and then `IContentVerifier`; failing posts become `Rejected` with feedback, and any edit sends a post back to `Draft`. Comments store a detected language; `POST /blog/comments/{id}/translation` translates into a supported locale and caches the result per locale. Both AI seams (`IContentVerifier`, `ICommentLanguageService`) are implemented in the host (`Daab.Web/Blog/`) on top of the translation module's Gemini `ITranslator`.

### Localization
`LocaleOptions.SupportedLocales` (`en`, `az`) drives validation. Requests implementing `ILocalized` / `ILocalizedCollection` are validated by FastEndpoints pre-processors in SharedKernel (`LocaleCollectionPreProcessor`); unsupported locales become validation failures. `LocalePreProcessor` is `[Obsolete]` — use the collection variant.

### Translation module (in active development)
Pipeline: lexical extraction (`Lexical/` — tag codec, tree walker over markup), tiered lookup via translation memory (`Memory/`, embeddings stored as blobs, cosine `Similarity`) + glossary (`Glossary/`), then Gemini generation (`Gemini/GeminiClient` implements both `ITranslator` and `IEmbedder`). `Service/TranslationService` orchestrates; retries tag-invalid blocks by feeding the error back to the model. HTTP surface: `POST /translations/draft`, `POST /translations/approve` (endpoints in `Daab.Web/Translation/`). See `TRANSLATION_PLAN.md` and `TRANSLATION_EXAMPLES.md`.

## Deploy
Push to `master` → GitHub Actions builds `src/Daab.Web/Dockerfile`, pushes `tng1/daab-server` to Docker Hub, SSH-restarts the container on the VPS (env from `ASPNETCORE_ENVIRONMENT=Production` + secret env-file). PRs to `master` run the CI gate (csharpier → build → test).
