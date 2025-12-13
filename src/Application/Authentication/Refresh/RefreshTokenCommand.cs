using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;

namespace Application.Authentication.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AccessTokensDto>;
