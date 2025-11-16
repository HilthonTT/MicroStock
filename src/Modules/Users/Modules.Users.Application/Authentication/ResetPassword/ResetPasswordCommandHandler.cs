using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Authentication;
using System.Text;

namespace Modules.Users.Application.Authentication.ResetPassword;

internal sealed class ResetPasswordCommandHandler(UserManager<IdentityUser> userManager) 
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        IdentityUser? identityUser = await userManager.FindByEmailAsync(command.Email);

        if (identityUser is null)
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.Unauthorized);
        }

        string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));
        IdentityResult identityResult = await userManager.ResetPasswordAsync(identityUser, command.Token, command.NewPassword);

        if (!identityResult.Succeeded)
        {
            return Result.Failure(AuthErrors.Unauthorized);
        }

        return Result.Success();
    }
}
