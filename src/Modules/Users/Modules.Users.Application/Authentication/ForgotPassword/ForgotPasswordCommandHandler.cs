using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Mail;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Application.Origin;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Authentication;
using System.Text;

namespace Modules.Users.Application.Authentication.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler(
    UserManager<IdentityUser> userManager,
    IOptions<OriginOptions> options,
    IMailService mailService) : ICommandHandler<ForgotPasswordCommand>
{
    private readonly OriginOptions _originOptions = options.Value;

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        IdentityUser? identityUser = await userManager.FindByEmailAsync(command.Email);

        if (identityUser is null)
        {
            return Result.Failure(AuthErrors.Unauthorized);
        }

        string token = await userManager.GeneratePasswordResetTokenAsync(identityUser);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        string resetPasswordUri = $"{_originOptions.OriginUrl}/reset-password?token={token}&email={command.Email}";
        var mailRequest = new MailRequest(
            [identityUser.Email!],
            "Reset Password",
            $"Please reset your password using the following link: {resetPasswordUri}");

        await mailService.SendAsync(mailRequest, cancellationToken);

        return Result.Success();
    }
}
