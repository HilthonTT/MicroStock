using MicroStock.Common.Domain;

namespace Modules.Users.Application.Abstractions.Authentication;

public static class AuthErrors
{
    public static readonly Error UnableToRegister = Error.Problem(
        $"Auth.UnableToRegister",
        "Unable to register user, please try again");

    public static readonly Error Unauthorized = Error.Authorization(
        "Auth.Unauthorized",
        "You are unauthorized");

    public static readonly Error AlreadyRegistered = Error.Conflict(
        "Auth.AlreadyRegistered",
        "You are already registered");
}
