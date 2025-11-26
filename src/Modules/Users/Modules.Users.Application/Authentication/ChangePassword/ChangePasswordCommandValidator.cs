using FluentValidation;

namespace Modules.Users.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.CurrentPassword).NotEmpty();

        RuleFor(x => x.NewPassword).NotEmpty()
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);
    }
}
