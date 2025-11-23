using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using MicroStock.Common.Presentation.ApiResults;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Authentication.LoginUser;

namespace Modules.Users.Presentation.Authentication;

internal sealed class LoginUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            Request request,
            ICommandHandler<LoginUserCommand, AccessTokensDto> handler,
            CancellationToken cancellationToken) =>
        {
            Result<AccessTokensDto> result = await handler.Handle(
                new LoginUserCommand(request.Email, request.Password),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Authentication);
    }

    private sealed record Request
    {
        public required string Email { get; init; }

        public required string Password { get; init; }
    }
}
