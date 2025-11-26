using MicroStock.Common.Application.Messaging;

namespace Modules.Users.Application.Authentication.ChangePassword;

public sealed record ChangePasswordCommand(string Email, string CurrentPassword, string NewPassword) : ICommand;
