using FluentValidation;

namespace Modules.Users.Application.Authentication.ResetPassword;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Token).NotEmpty();

        RuleFor(x => x.NewPassword)
           .NotEmpty()
           .MinimumLength(6)
           .MaximumLength(100);

        RuleFor(x => x.ConfirmPassword)
           .NotEmpty()
           .Equal(x => x.NewPassword)
           .WithMessage("Passwords do not match");
    }
}
