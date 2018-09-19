namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    public class SessionController : Controller
    {
        #region constructors and destructors

        public SessionController(IOptions<AzureAdB2COptions> b2COptions)
        {
            AzureAdB2COptions = b2COptions.Value;
        }

        #endregion

        #region methods

        [HttpGet]
        public IActionResult EditProfile()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = AzureAdB2COptions.EditProfilePolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = AzureAdB2COptions.ResetPasswordPolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            return View();
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action(nameof(SignedOut), "Session", null, Request.Scheme);
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = callbackUrl
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        #endregion

        #region properties

        public AzureAdB2COptions AzureAdB2COptions { get; set; }

        #endregion
    }
}