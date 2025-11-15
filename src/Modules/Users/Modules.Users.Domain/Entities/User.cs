using MicroStock.Common.Domain;
using MicroStock.Common.Domain.Auditing;
using Modules.Users.Domain.DomainEvents;

namespace Modules.Users.Domain.Entities;

[Auditable]
public sealed class User : Entity
{
    private User()
    {
    }

    public Guid Id { get; private set; }

    public string IdentityId { get; private set; }

    public string Email { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static User Create(
        string identityId, 
        string email, 
        string firstName, 
        string lastName,
        DateTime createdAtUtc)
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            IdentityId = identityId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAtUtc = createdAtUtc
        };

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, user.Email));

        return user;
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}
