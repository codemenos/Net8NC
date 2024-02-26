namespace Solucao.Service.API.Core.Handlers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class SignInManagerHandler<TUser> : SignInManager<TUser> where TUser : class
{
    private readonly ILogger<SignInManagerHandler<TUser>> _logger;

    public SignInManagerHandler(UserManager<TUser> userManager,
                               IHttpContextAccessor contextAccessor,
                               IUserClaimsPrincipalFactory<TUser> claimsFactory,
                               IOptions<IdentityOptions> optionsAccessor,
                               ILogger<SignInManagerHandler<TUser>> logger,
                               IAuthenticationSchemeProvider schemes,
                               IUserConfirmation<TUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _logger = logger;
    }

    public override async Task SignInAsync(TUser user, AuthenticationProperties authenticationProperties, string authenticationMethod = null)
    {
        _logger.LogInformation($"User {user} is signing in.");
        await base.SignInAsync(user, authenticationProperties, authenticationMethod);
    }

    public override async Task SignInAsync(TUser user, bool isPersistent, string authenticationMethod = null)
    {
        _logger.LogInformation($"User {user} is signing in with isPersistent = {isPersistent}.");
        await base.SignInAsync(user, isPersistent, authenticationMethod);
    }

    public override async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
    {
        _logger.LogInformation($"User {user} is signing in with additional claims.");
        await base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
    }
}
