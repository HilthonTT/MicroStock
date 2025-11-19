namespace Modules.Users.Application.Users.GetProfile;

public sealed record UserDto
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
