using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MicroStock.Common.Application.Authentication;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using MicroStock.Common.Presentation.ApiResults;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Users.GetProfile;
using Modules.Users.Application.Users.GetUser;

namespace Modules.Users.Presentation.Users;

internal sealed class GetProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me", async (
            IUserContext userContext,
            IQueryHandler<GetUserQuery, UserDto> handler,
            CancellationToken cancellationToken) =>
        {
            Guid userId = await userContext.GetUserIdAsync(cancellationToken);
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            Result<UserDto> result = await handler.Handle(new GetUserQuery(userId), cancellationToken);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization()
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces<UserDto>(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
