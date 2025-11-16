using FluentValidation;

namespace Modules.Users.Application.Authentication.LoginUser;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
          .NotEmpty()
          .EmailAddress()
          .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
