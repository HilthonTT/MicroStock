using Microsoft.AspNetCore.Identity;
using MicroStock.Common.Domain;
using MicroStock.Common.Domain.Auditing;

namespace Modules.Users.Domain.Entities;

[Auditable]
public sealed class RefreshToken : Entity
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public string Token { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public IdentityUser User { get; private set; } = default!;
    
    public static RefreshToken Create(
        string userId, 
        string token, 
        DateTime expiresAtUtc,
        IdentityUser user)
    {
        var rf = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            Token = token,
            UserId = userId,
            ExpiresAtUtc = expiresAtUtc,
            User = user,
        };

        return rf;
    }
}
