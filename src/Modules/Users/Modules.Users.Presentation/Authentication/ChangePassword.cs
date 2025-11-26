using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using MicroStock.Common.Presentation.ApiResults;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Authentication.ChangePassword;

namespace Modules.Users.Presentation.Authentication;

internal sealed class ChangePassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/change-password", async (
            Request request,
            ICommandHandler<ChangePasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(
                new ChangePasswordCommand(request.Email, request.CurrentPassword, request.NewPassword),
                cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .WithTags(Tags.Authentication);
    }

    private sealed record Request
    {
        public required string Email { get; init; }

        public required string CurrentPassword { get; init; }

        public required string NewPassword { get; init; }
    }
}
