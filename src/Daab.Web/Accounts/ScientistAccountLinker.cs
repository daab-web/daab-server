using Daab.Modules.Auth.Common;
using Daab.Modules.Scientists.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daab.Web.Accounts;

/// <summary>
/// Host-side bridge between Auth and Scientists. The <c>scientist</c> role itself is stored on
/// the account (granted by an administrator, or by inviting the scientist); this bridge only adds
/// the <c>scientist_id</c> claim when a Scientist profile is linked to the account, so posts can
/// link back to the author's public profile. Profiles are linked only by an administrator's
/// invitation (<see cref="InviteScientistEndpoint"/>) — never by matching e-mails.
/// </summary>
internal sealed class ScientistAccountLinker(ScientistsDbContext context) : IAccountLinker
{
    public async Task<AccountLink> LinkAsync(
        string userId,
        string? email,
        CancellationToken cancellationToken
    )
    {
        var scientistId = await context
            .Scientists.Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return scientistId is null
            ? AccountLink.None
            : new AccountLink(
                [],
                new Dictionary<string, string> { [AuthClaims.ScientistId] = scientistId }
            );
    }
}
