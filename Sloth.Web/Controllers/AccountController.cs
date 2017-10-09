using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sloth.Core.Models;
using Sloth.Core.Services;

namespace Sloth.Web.Controllers
{
    public class AccountController : SuperController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IDirectorySearchService _directorySearchService;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IDirectorySearchService directorySearchService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _directorySearchService = directorySearchService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;

            // Request a redirect to the external login provider.
            var provider = "UCDavis";
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                //ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (info.LoginProvider.Equals("UCDavis"))
            {
                await ProcessUCDavisInfo(info);
            }

            // Find user
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            // Sign in the user if the user already exists
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }

            // If the user does not have an account, create an account
            user = new User { Email = email };
            await _userManager.CreateAsync(user);

            // Sign in new user
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }

        private async Task ProcessUCDavisInfo(ExternalLoginInfo info)
        {
            // email comes across in both name claim and upn
            var email = info.Principal.FindFirstValue(ClaimTypes.Upn);

            var ucdUser = await _directorySearchService.GetByEmailAsync(email);
            if (ucdUser == null) return;

            // TODO: see if we need to modify claims like this
            var identity = (ClaimsIdentity) info.Principal.Identity;

            // Should we bother replacing via directory service?
            identity.AddClaim(new Claim(ClaimTypes.Email, ucdUser.Mail));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, ucdUser.GivenName));
            identity.AddClaim(new Claim(ClaimTypes.Surname, ucdUser.Surname));

            // name from Azure comes back w/ email, so replace w/ display name
            identity.RemoveClaim(identity.FindFirst(ClaimTypes.NameIdentifier));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ucdUser.Kerberos));
            info.ProviderKey = ucdUser.Kerberos;

            // name from Azure comes back w/ email, so replace w/ display name
            identity.RemoveClaim(identity.FindFirst(ClaimTypes.Name));
            identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            var provider = "UCDavis";
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, Url.Action("Index", "Home"));
            return SignOut(properties, provider);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
