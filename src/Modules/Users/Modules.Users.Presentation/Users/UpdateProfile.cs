using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MicroStock.Common.Application.Authentication;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using MicroStock.Common.Presentation.ApiResults;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Users.GetProfile;
using Modules.Users.Application.Users.UpdateUser;

namespace Modules.Users.Presentation.Users;

internal sealed class UpdateProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("me", async (
            Request request,
            IUserContext userContext,
            ICommandHandler<UpdateUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Guid userId = await userContext.GetUserIdAsync(cancellationToken);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            Result result = await handler.Handle(
                new UpdateUserCommand(userId, request.FirstName, request.LastName),
                cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags(Tags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<UserDto>(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }

    private sealed record Request
    {
        public required string FirstName { get; init; }

        public required string LastName { get; init; }
    }
}
