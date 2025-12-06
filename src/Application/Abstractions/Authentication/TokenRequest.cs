namespace Application.Abstractions.Authentication;

public sealed record TokenRequest(string UserId, string Email, IEnumerable<string> Roles);
