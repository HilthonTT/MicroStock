using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class EmailVerificationToken : Entity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public bool IsUsed { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    /// <summary>
    /// Navigation Properties
    /// </summary>
    public User User { get; private set; } = default!;

    public static EmailVerificationToken Create(Guid userId, string token, TimeSpan validityPeriod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(validityPeriod, TimeSpan.Zero);

        DateTime now = DateTime.UtcNow;

        return new EmailVerificationToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Token = token,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(validityPeriod),
            IsUsed = false
        };
    }

    public static EmailVerificationToken Create(Guid userId, string token, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAtUtc));
        }

        return new EmailVerificationToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Token = token,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiresAtUtc,
            IsUsed = false
        };
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Token has already been used");
        }

        if (IsExpired())
        {
            throw new InvalidOperationException("Token has expired");
        }

        IsUsed = true;
        UsedAtUtc = DateTime.UtcNow;
    }

    public bool IsValid() => !IsUsed && !IsExpired();

    public bool IsExpired() => DateTime.UtcNow > ExpiresAtUtc;
}
