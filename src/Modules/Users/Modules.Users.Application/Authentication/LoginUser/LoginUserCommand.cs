using MicroStock.Common.Application.Messaging;
using Modules.Users.Application.Abstractions.Authentication;

namespace Modules.Users.Application.Authentication.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AccessTokensDto>;
