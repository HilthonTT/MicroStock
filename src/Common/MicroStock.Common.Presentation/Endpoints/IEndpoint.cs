using Microsoft.AspNetCore.Routing;

namespace MicroStock.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
