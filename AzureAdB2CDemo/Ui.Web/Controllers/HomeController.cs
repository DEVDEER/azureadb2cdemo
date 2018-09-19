namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;

    using Models;

    public class HomeController : Controller
    {
        #region member vars

        private readonly AzureAdB2COptions _azureAdB2COptions;

        #endregion

        #region constructors and destructors

        public HomeController(IOptions<AzureAdB2COptions> azureAdB2COptions)
        {
            _azureAdB2COptions = azureAdB2COptions.Value;
        }

        #endregion

        #region methods

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = string.Format("Claims available for the user {0}", User.FindFirst("name")?.Value);
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Api()
        {
            var responseString = "";
            try
            {
                // Retrieve the token with the specified scopes
                var scope = _azureAdB2COptions.ApiScopes.Split(' ');
                var signedInUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var userTokenCache = new MsalSessionCache(signedInUserId, HttpContext).GetMsalCacheInstance();
                var clientApplication = new ConfidentialClientApplication(
                    _azureAdB2COptions.ClientId,
                    _azureAdB2COptions.Authority,
                    _azureAdB2COptions.RedirectUri,
                    new ClientCredential(_azureAdB2COptions.ClientSecret),
                    userTokenCache,
                    null);
                var result = await clientApplication.AcquireTokenSilentAsync(scope, clientApplication.Users.FirstOrDefault(), _azureAdB2COptions.Authority, false);
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, _azureAdB2COptions.ApiUrl);
                    // Add token to the Authorization header and make the request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    var response = await client.SendAsync(request);
                    // Handle the response
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            responseString = await response.Content.ReadAsStringAsync();
                            break;
                        case HttpStatusCode.Unauthorized:
                            responseString = $"Please sign in again. {response.ReasonPhrase}";
                            break;
                        default:
                            responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                            break;
                    }
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }
            ViewData["Payload"] = $"{responseString}";
            return View();
        }

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        #endregion
    }
}