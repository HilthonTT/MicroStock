using SharedKernel;

namespace Application.Abstractions.Authentication;

public static class AuthErrors
{
    public static readonly Error UnableToRegister = Error.Problem(
        "Auth.UnableToRegister",
        "Unable to register user, please try again.");

    public static readonly Error UnableToLogin = Error.Problem(
        "Auth.UnableToLogin",
        "Unable to login, please try again.");

    public static readonly Error EmailAlreadyInUse = Error.Conflict(
        "Auth.EmailAlreadyInUse",
        "The email is already in use.");

    public static readonly Error EmailMustBeVerified = Error.Problem(
        "Auth.EmailMustBeVerified",
        "Please verify your email first.");
}
