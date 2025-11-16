using MicroStock.Common.Application.Messaging;

namespace Modules.Users.Application.Authentication.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email, 
    string Token, 
    string NewPassword, 
    string ConfirmPassword) : ICommand;
