using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using MicroStock.Common.Presentation.ApiResults;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Authentication.RegisterUser;

namespace Modules.Users.Presentation.Authentication;

internal sealed class RegisterUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
            Request request,
            ICommandHandler <RegisterUserCommand, AccessTokensDto> handler,
            CancellationToken cancellationToken) =>
        {
            Result<AccessTokensDto> result = await handler.Handle(
                new RegisterUserCommand(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Password,
                    request.ConfirmPassword),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Authentication);
    }

    private sealed record Request
    {
        public required string Email { get; init; }

        public required string FirstName { get; init; }

        public required string LastName { get; init; }

        public required string Password { get; init; }

        public required string ConfirmPassword { get; init; }
    }
}
