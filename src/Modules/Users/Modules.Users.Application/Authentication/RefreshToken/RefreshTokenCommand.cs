using MicroStock.Common.Application.Messaging;
using Modules.Users.Application.Abstractions.Authentication;

namespace Modules.Users.Application.Authentication.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AccessTokensDto>;
