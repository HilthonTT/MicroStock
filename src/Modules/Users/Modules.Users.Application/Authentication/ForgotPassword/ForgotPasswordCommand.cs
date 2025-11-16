using MicroStock.Common.Application.Messaging;

namespace Modules.Users.Application.Authentication.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand;