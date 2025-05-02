using Ofgem_Web_LAF_ExternalPortal.Services;
using System.Diagnostics.CodeAnalysis;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Services;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions;
[ExcludeFromCodeCoverage]
public static class BuilderExtensions
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDeclarationManagementService, DeclarationManagementService>();

        string declarationServiceUrl = builder.Configuration["DeclarationServiceApiUrl"] ?? throw new InvalidOperationException();
        builder.Services.AddHttpClient<IDeclarationManagementService, DeclarationManagementService>(
            x => x.BaseAddress = new Uri(declarationServiceUrl!));

        builder.Services.AddScoped<ISendMessageService, SendMessageService>();
        builder.Services.AddScoped<IBasicValidationService, BasicValidationService>();
        builder.Services.AddScoped<IDocumentManagementService, DocumentManagementService>();

        string documentServiceUrl = builder.Configuration["DocumentServiceApiUrl"] ?? throw new InvalidOperationException();
        builder.Services.AddHttpClient<IDocumentManagementService, DocumentManagementService>(
            x => x.BaseAddress = new Uri(documentServiceUrl!));

        string localAuthorityServiceUrl = builder.Configuration["LocalAuthorityServiceApiUrl"] ?? throw new InvalidOperationException();
        builder.Services.AddHttpClient<Services.ILaManagementService, Services.LaManagementService>(
            x => x.BaseAddress = new Uri(localAuthorityServiceUrl!));

        string userManagementServiceUrl = builder.Configuration["UserServiceApiUrl"] ?? throw new InvalidOperationException();
        builder.Services.AddHttpClient<Services.IUserManagementServiceUI, Services.UserManagementServiceUI>(
            x => x.BaseAddress = new Uri(userManagementServiceUrl!));

    }

}