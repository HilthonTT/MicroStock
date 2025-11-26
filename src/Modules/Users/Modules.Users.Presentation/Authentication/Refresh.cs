using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using MicroStock.Common.Presentation.ApiResults;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Authentication.RefreshToken;

namespace Modules.Users.Presentation.Authentication;

internal sealed class Refresh : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            Request request,
            ICommandHandler<RefreshTokenCommand, AccessTokensDto> handler,
            CancellationToken cancellationToken) =>
        {
            Result<AccessTokensDto> result = await handler.Handle(
                new RefreshTokenCommand(request.RefreshToken), 
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .Produces<AccessTokensDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous()
        .WithTags(Tags.Authentication);
    }

    private sealed record Request
    {
        public required string RefreshToken { get; init; }
    }
}
