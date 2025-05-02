using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.KeyVaultExtensions;
using Microsoft.IdentityModel.Tokens;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.Configuration;
using Ofgem.OneLogin.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.Services;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Services;
using System.Diagnostics.CodeAnalysis;

namespace Ofgem.OneLogin.SharedLibrary;

[ExcludeFromCodeCoverage]
public static class ConfigureOneLoginServices
{

    public static IServiceCollection AddOneLoginServices(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<ILocalAuthorityManagementService, LocalAuthorityManagementService>();

        services.AddHttpClient<IUserManagementService, UserManagementService>(
            x => x.BaseAddress = new Uri(configuration["UserServiceApiUrl"]!));

        services.AddHttpClient<ILocalAuthorityManagementService, LocalAuthorityManagementService>(
            x => x.BaseAddress = new Uri(configuration["LocalAuthorityServiceApiUrl"]!));

        services.AddAndConfigureGovUkAuthentication(configuration, typeof(CustomClaims), OneLoginConstants.SignedOutUrl);

        services.AddAuthorizationBuilder()
            .AddPolicy(
                PolicyNames.IsAuthenticated, policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

        return services;
    }

    public static void AddAndConfigureGovUkAuthentication(this IServiceCollection services,
        IConfiguration configuration, Type customClaims, string signedOutRedirectUrl = "")
    {

        services.AddServiceRegistration(configuration, customClaims);
        services.ConfigureGovUkAuthentication(configuration, signedOutRedirectUrl);
    }

    public static void AddServiceRegistration(this IServiceCollection services, IConfiguration configuration,
        Type customClaims)
    {
        if (!configuration.GetSection(nameof(GovUkOidcConfiguration)).GetChildren().Any())
        {
            throw new ArgumentException(
                "Cannot find GovUkOidcConfiguration in configuration. Please add a section called GovUkOidcConfiguration with BaseUrl, ClientId and KeyVaultIdentifier properties.");
        }

        services.AddOptions();
        services.AddHttpContextAccessor();
        services.AddTransient(typeof(ICustomClaims), customClaims);
#if NETSTANDARD2_0
            services.Configure<GovUkOidcConfiguration>(_=>configuration.GetSection(nameof(GovUkOidcConfiguration)));
#else 
        services.Configure<GovUkOidcConfiguration>(configuration.GetSection(nameof(GovUkOidcConfiguration)));
#endif
        services.AddSingleton(c => c.GetService<IOptions<GovUkOidcConfiguration>>()!.Value);
        services.AddHttpClient<IOidcService, OidcService>();
        services.AddTransient<IRedirectService, RedirectService>();
        services.AddTransient<IAzureIdentityService, AzureIdentityService>();
        services.AddTransient<IJwtSecurityTokenService, JwtSecurityTokenService>();
    }

    internal static void ConfigureGovUkAuthentication(this IServiceCollection services, 
        IConfiguration configuration, string redirectUrl)
    {
        services
            .AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(options =>
            {
                var govUkConfiguration = configuration.GetSection(nameof(GovUkOidcConfiguration));

                options.ClientId = govUkConfiguration["ClientId"];
                options.MetadataAddress = $"{govUkConfiguration["BaseUrl"]}/.well-known/openid-configuration";
                options.ResponseType = "code";
                options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
                options.SignedOutRedirectUri = $"/{OneLoginConstants.SignedOutUrl}";
                options.CallbackPath = "/signin-oidc";
                options.ResponseMode = string.Empty; 
                options.UsePkce = true;
                options.SaveTokens = true;

                var scopes = "openid email".Split(' ');
                options.Scope.Clear();
                foreach (var scope in scopes)
                {
                    options.Scope.Add(scope);
                }

                options.Events.OnRemoteFailure = c =>
                {
                    if (c.Failure != null && c.Failure.Message.Contains("Correlation failed"))
                    {
                        c.Response.Redirect("/");
                        c.HandleResponse();
                    }

                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToIdentityProvider = c =>
                {
                    if (bool.TryParse(govUkConfiguration[OneLoginConstants.EnableMfa], out var enableMfa))
                    {
                        // One login - where "Cl" is low level or no mfa and "Cl.Cm" is medium level or enabled mfa.
                        var vtr = enableMfa ? OneLoginConstants.MediumLevel : OneLoginConstants.LowLevel;
                        c.ProtocolMessage.SetParameter("vtr", $"[\"{vtr}\"]");
                    }
                    return Task.CompletedTask;
                };

                options.Events.OnUserInformationReceived = _ => Task.CompletedTask;

                options.Events.OnSignedOutCallbackRedirect = c =>
                {
                    c.Response.Cookies.Delete(GovUkConstants.AuthCookieName);
                    c.Response.Redirect($"/{OneLoginConstants.SignedOutUrl}");
                    c.HandleResponse();
                    return Task.CompletedTask;
                };
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/errors/403");
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.Cookie.Name = GovUkConstants.AuthCookieName;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.CookieManager = new ChunkingCookieManager { ChunkSize = 3000 };
                options.LogoutPath = $"/{OneLoginConstants.SignedOutUrl}";

            });

        // Add policy that the user cannot use the External Portal without a linked Ofgem account
        services.AddAuthorizationBuilder()
            .AddPolicy(ClaimsConstants.LinkedAccount, policy => policy.RequireClaim(ClaimsConstants.LinkedAccount));
        

        services
            .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<IOidcService, IUserManagementService,  IAzureIdentityService, IRedirectService, GovUkOidcConfiguration>(
                (options, oidcService, userManagementService, azureIdentityService, redirectService, config) =>
                {
                    var govUkConfiguration = config;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        AuthenticationType = "private_key_jwt",
                        IssuerSigningKey = new KeyVaultSecurityKey(
                            govUkConfiguration.KeyVaultIdentifier,
                            azureIdentityService.AuthenticationCallback),
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        SaveSigninToken = true
                    };

                    options.Events.OnAuthorizationCodeReceived = async (ctx) =>
                    {
                        var token = await oidcService.GetToken(ctx.TokenEndpointRequest,ctx.Options.UsePkce);
                        if (token?.AccessToken != null && token.IdToken != null)
                        {
                            ctx.HandleCodeRedemption(token.AccessToken, token.IdToken);
                        }
                    };
                    options.Events.OnTokenValidated = async ctx =>
                    {
                        await oidcService.PopulateAccountClaims(ctx);
                        var externalUser = await userManagementService.SyncExternalUserAsync(ctx);
                 

                        await oidcService.PopulateCustomClaims(ctx, externalUser);
                    };
                    options.Events.OnRedirectToIdentityProviderForSignOut = async ctx =>
                    {
                        // Redirects to the External Portal SignOut page
                        ctx.ProtocolMessage.PostLogoutRedirectUri = await redirectService.GetPostLogoutRedirectUriAsync();
                    };
                });
    }
}