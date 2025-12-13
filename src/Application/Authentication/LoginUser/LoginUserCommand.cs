using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;

namespace Application.Authentication.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AccessTokensDto>;
