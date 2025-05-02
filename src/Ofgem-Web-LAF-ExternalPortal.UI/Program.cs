using Microsoft.AspNetCore.Authorization;
using Ofgem.OneLogin.SharedLibrary;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Services;
using Ofgem_Web_LAF_ExternalPortal.SignOut;
using Serilog;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Add services to the container
builder.Services.AddHttpClient();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddLogsConfiguration(builder.Configuration);
}

builder.Services.AddHttpContextAccessor();

// One Login
// =========
builder.Services.AddOneLoginServices(builder.Configuration);

builder.Services.AddRazorPages();

// Add named services to the container.
builder.Services.AddScoped<IRedactionService, RedactionService>();
FeatureService.ConfigureFeatureService(builder);

builder.Services.AddSession();
builder.Services.AddMemoryCache();

builder.Services.AddApplicationInsightsTelemetry();
builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.RegisterServices();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MustHaveClaimLocalAuthorities", policy =>
        policy.Requirements.Add(new MustHaveClaimLocalAuthoritiesRequirement("UserLocalAuthorities")));

builder.Services.AddSingleton<IAuthorizationHandler, MustHaveClaimHandler>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // peter app.UseHsts(); please LEAVE the commented out code
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseRouting();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapRazorPages();
app.UseSession();
app.UseStatusCodePagesWithReExecute("/Errors/{0}");
app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(context =>
{
    if (context.HttpContext.Response.StatusCode == 403)
    {
        context.HttpContext.Response.Redirect("/Errors/403");
    }

    return Task.CompletedTask;
});

app.MapSignout();
await app.RunAsync();

[ExcludeFromCodeCoverage]
public static partial class Program { }