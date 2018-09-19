namespace WebApp_OpenIDConnect_DotNet
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;

    using Models;

    /// <summary>
    /// 
    /// </summary>
    public class OpenIdConnectOptionsSetup : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        #region constructors and destructors

        public OpenIdConnectOptionsSetup(IOptions<AzureAdB2COptions> options)
        {
            AzureAdB2COptions = options.Value;
        }

        #endregion

        #region explicit interfaces

        /// <inheritdoc />
        public void Configure(string name, OpenIdConnectOptions options)
        {
            options.ClientId = AzureAdB2COptions.ClientId;
            options.Authority = AzureAdB2COptions.Authority;
            options.UseTokenLifetime = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name"
            };
            // hook to OpenId events
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                OnRemoteFailure = OnRemoteFailure,
                OnAuthorizationCodeReceived = OnAuthorizationCodeReceived
            };
        }

        /// <inheritdoc />
        public void Configure(OpenIdConnectOptions options)
        {
            Configure(Options.DefaultName, options);
        }

        #endregion

        #region methods

        /// <summary>
        /// Is called whenever B2C retrieves a new auth token.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            // Use MSAL to swap the code for an access token
            // Extract the authorization code from the response notification
            var code = context.ProtocolMessage.Code;
            var signedInUserId = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userTokenCache = new MsalSessionCache(signedInUserId, context.HttpContext).GetMsalCacheInstance();
            var clientApplication = new ConfidentialClientApplication(
                AzureAdB2COptions.ClientId,
                AzureAdB2COptions.Authority,
                AzureAdB2COptions.RedirectUri,
                new ClientCredential(AzureAdB2COptions.ClientSecret),
                userTokenCache,
                null);
            // try to retrieve the baerer token
            try
            {
                var result = await clientApplication.AcquireTokenByAuthorizationCodeAsync(code, AzureAdB2COptions.ApiScopes.Split(' '));
                context.HandleCodeRedemption(result.AccessToken, result.IdToken);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Is called when OpenId is redirecting to an identity provider (e.g. B2C)
        /// </summary>
        /// <param name="context">The redirection context.</param>
        public Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            var defaultPolicy = AzureAdB2COptions.DefaultPolicy;
            if (context.Properties.Items.TryGetValue(AzureAdB2COptions.PolicyAuthenticationProperty, out var policy) && !policy.Equals(defaultPolicy))
            {
                context.ProtocolMessage.Scope = OpenIdConnectScope.OpenIdProfile;
                context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
                context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.ToLower().Replace(defaultPolicy.ToLower(), policy.ToLower());
                context.Properties.Items.Remove(AzureAdB2COptions.PolicyAuthenticationProperty);
            }
            else if (!string.IsNullOrEmpty(AzureAdB2COptions.ApiUrl))
            {
                context.ProtocolMessage.Scope += $" offline_access {AzureAdB2COptions.ApiScopes}";
                context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Is called when an identity provider throws an error.
        /// </summary>
        /// <param name="context">The context containing failure details.</param>
        public Task OnRemoteFailure(RemoteFailureContext context)
        {
            context.HandleResponse();
            // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
            // because password reset is not supported by a "sign-up or sign-in policy"
            if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90118"))
            {
                // If the user clicked the reset password link, redirect to the reset password route
                context.Response.Redirect("/Session/ResetPassword");
            }
            else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
            {
                context.Response.Redirect("/");
            }
            else
            {
                context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
            }
            return Task.FromResult(0);
        }

        #endregion

        #region properties

        /// <summary>
        /// Retrieves the options configured for AAD B2C.
        /// </summary>
        public AzureAdB2COptions AzureAdB2COptions { get; }

        #endregion
    }
}