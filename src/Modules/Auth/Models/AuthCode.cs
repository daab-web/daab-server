namespace Daab.Modules.Auth.Models;

public record AuthCode
{
    public required string ClientId { get; set; }
    public required string UserId { get; set; }
    public required string CodeChallenge { get; set; }
    public required string CodeChallengeMethod { get; set; }
    public required string RedirectUri { get; set; }
    public DateTime Expiry { get; set; }
}
