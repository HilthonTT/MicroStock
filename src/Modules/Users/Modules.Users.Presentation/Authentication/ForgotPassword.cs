using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using MicroStock.Common.Presentation.ApiResults;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Authentication.ForgotPassword;

namespace Modules.Users.Presentation.Authentication;

internal sealed class ForgotPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/forgot-password", async (
            Request request,
            ICommandHandler <ForgotPasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(
                new ForgotPasswordCommand(request.Email),
                cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .Produces(StatusCodes.Status204NoContent)
        .WithTags(Tags.Authentication);
    }

    private sealed record Request
    {
        public required string Email { get; init; }
    }
}
