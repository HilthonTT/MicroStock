using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Authentication.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Name, string Password, string ConfirmPassword) : ICommand;
