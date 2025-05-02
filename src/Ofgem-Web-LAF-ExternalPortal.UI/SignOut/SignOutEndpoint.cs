using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.Services;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;

namespace Ofgem_Web_LAF_ExternalPortal.SignOut
{
    public static class ApplicationBuilderExtensions
    {
        public static void MapSignout(this WebApplication application)
        {
            application.MapGet(OneLoginConstants.SignOutUrl, [Authorize] async (HttpContext httpContext, IConfiguration configuration, ILoggerFactory loggerFactory, IRedirectService redirectService) =>
            {
                var logger = loggerFactory.CreateLogger(OneLoginConstants.SignOutUrl);
                try
                {
                    logger.LogInformation("User Signing Out");
                    const string tokenName = "id_token";
                    var token = await httpContext.GetTokenAsync(tokenName);

                    await SignOutFromOneLogInAsync(configuration, token, logger, redirectService);

                    await SignOutFromExternalPortalAsync(httpContext, tokenName, token, logger);
                }
                catch (Exception ex)
                {
                    logger.LogLafError(LogEvents.SignOut, "{Message}", ex.Message);
                    throw;
                }
            });
        }

        private static async Task SignOutFromOneLogInAsync(IConfiguration configuration, string? token, ILogger logger, IRedirectService redirectService)
        {
            try
            {
                var govUkConfiguration = configuration.GetSection("GovUkOidcConfiguration");

                var baseUrl = govUkConfiguration["BaseUrl"];

                ArgumentNullException.ThrowIfNull(baseUrl);

                var logoutUrl = $"logout?id_token_hint={token}&post_logout_redirect_uri={redirectService.GetPostLogoutRedirectUriAsync()}";

                var request = new HttpClient() { BaseAddress = new Uri(baseUrl) };

                await request.GetAsync(logoutUrl);
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.SignOut, "Signing out from OneLogIn threw an exception: {Message}.", ex.Message);
                throw;
            }
        }

        private static async Task SignOutFromExternalPortalAsync(HttpContext httpContext, string tokenName, string? token, ILogger logger)
        {
            try
            {
                var authenticationProperties = new AuthenticationProperties();
                authenticationProperties.Parameters.Clear();
                authenticationProperties.Parameters.Add(tokenName, token);
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, authenticationProperties);
                await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authenticationProperties);
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.SignOut, "Signing out from the External Portal threw an exception {Message}.", ex.Message);
                throw;
            }
        }
    }
}
