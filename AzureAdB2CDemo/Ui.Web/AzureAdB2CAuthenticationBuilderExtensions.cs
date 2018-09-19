namespace WebApp_OpenIDConnect_DotNet
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class AzureAdB2CAuthenticationBuilderExtensions
    {
        #region methods

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder)
        {
            return builder.AddAzureAdB2C(
                _ =>
                {
                });
        }

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder, Action<AzureAdB2COptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectOptionsSetup>();
            builder.AddOpenIdConnect();
            return builder;
        }

        #endregion
    }
}