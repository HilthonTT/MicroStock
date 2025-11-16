using MicroStock.Common.Application.Messaging;
using Modules.Users.Application.Abstractions.Authentication;

namespace Modules.Users.Application.Authentication.RegisterUser;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password, string ConfirmPassword)
    : ICommand<AccessTokensDto>;
