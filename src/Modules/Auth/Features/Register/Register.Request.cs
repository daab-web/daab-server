namespace Daab.Modules.Auth.Features.Register;

public sealed record RegisterRequest(string FullName, string Email, string Password);
