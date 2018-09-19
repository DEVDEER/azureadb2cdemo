namespace WebApp_OpenIDConnect_DotNet
{
    using System;
    using System.Linq;

    public class AzureAdB2COptions
    {
        #region constants

        public const string AzureAdB2CInstance = "https://login.microsoftonline.com/tfp";

        public const string PolicyAuthenticationProperty = "Policy";

        #endregion

        #region properties

        public string ApiScopes { get; set; }

        public string ApiUrl { get; set; }

        public string Authority => $"{AzureAdB2CInstance}/{Tenant}/{DefaultPolicy}/v2.0";

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string DefaultPolicy => SignUpSignInPolicyId;

        public string EditProfilePolicyId { get; set; }

        public string RedirectUri { get; set; }

        public string ResetPasswordPolicyId { get; set; }

        public string SignInPolicyId { get; set; }

        public string SignUpPolicyId { get; set; }

        public string SignUpSignInPolicyId { get; set; }

        public string Tenant { get; set; }

        #endregion
    }
}