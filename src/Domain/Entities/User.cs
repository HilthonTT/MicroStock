using Domain.DomainEvents;
using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class User : Entity
{
    public Guid Id { get; private set; }

    public string IdentityId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public bool EmailVerified { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Navigation properties
    /// </summary>
    public ICollection<InvestmentStrategy> InvestmentStrategies { get; private set; } = [];

    private User()
    {
    }

    public static User Create(string identityId, string name, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            IdentityId = identityId,
            Name = name,
            Email = email,
            CreatedAtUtc = DateTime.UtcNow,
        };

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, user.IdentityId, user.Email));

        return user;
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        UpdatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new UserUpdatedDomainEvent(Id, Name));
    }

    public void UpdateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Email = email;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
