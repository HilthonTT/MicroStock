namespace Application.Abstractions.Authentication;

public sealed record TokenRequest(Guid UserId, string IdentityId, string Email, IEnumerable<string> Roles);
