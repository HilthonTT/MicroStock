namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    string IdentityId { get; }

    Guid UserId { get; }
}
