using MicroStock.Common.Application.Messaging;
using Modules.Users.Application.Users.GetProfile;

namespace Modules.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserDto>;
