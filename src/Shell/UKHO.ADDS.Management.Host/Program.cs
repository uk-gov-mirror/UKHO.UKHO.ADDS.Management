using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Radzen;
using UKHO.ADDS.Management.Host.Extensions;
using UKHO.ADDS.Management.Host.Shell; // App component
using UKHO.ADDS.Management.Modules.FileShare.Registration;
using UKHO.ADDS.Management.Modules.Developer.Registration;
using UKHO.ADDS.Management.Modules.Permit.Registration;
using UKHO.ADDS.Management.Shell.Services;
using UKHO.ADDS.Management.Shell.Configuration;
using UKHO.ADDS.Management.Shell.Services.Storage;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace UKHO.ADDS.Management.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureConfigurationJson(builder);

        builder.AddServiceDefaults();

        // Registers the developer module
        builder.Services.AddDeveloperModule();
        builder.Services.AddFileShareModule();
        builder.Services.AddPermitModule();

        builder.Services.AddScoped(sp =>
        {
            var nav = sp.GetRequiredService<NavigationManager>();
            return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
        });

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.WebHost.UseStaticWebAssets();

        builder.Services.AddRazorPages();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddRadzenComponents();
        builder.Services.AddRadzenQueryStringThemeService();

        builder.Services.AddSingleton<ModuleHealthService>();
        builder.Services.AddScoped<DeploymentContext>();
        builder.Services.AddScoped<ModuleLifecycleOrchestrator>();
        builder.Services.AddScoped<ConfigurationReloadNotifier>();
        builder.Services.AddScoped<ModulePageService>();
        builder.Services.AddOutputCache();

        builder.Services.AddHttpContextAccessor().AddTransient<AuthorizationHandler>();

        builder.Services.AddTransient<IClaimsTransformation, KeycloakRealmRoleClaimsTransformation>();

        // Register the module configuration provider
        builder.Services.AddSingleton<IModuleConfigurationProvider, ModuleConfigurationProvider>();

        // Register deployments loader
        builder.Services.AddScoped<DeploymentsJsonLoader>();

        // Register deployment selection storage interop
        builder.Services.AddScoped<DeploymentSelectionStorage>();

        var oidcScheme = OpenIdConnectDefaults.AuthenticationScheme;

        builder.Services.AddAuthentication(oidcScheme)
            .AddKeycloakOpenIdConnect("keycloak", "ADDSManagement", oidcScheme, options =>
            {
                options.ClientId = "ADDSManagementShell";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.Scope.Add("addsmanagement:all");
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                options.SaveTokens = true;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddAuthorization();

        builder.Services.Configure<CircuitOptions>(options => { options.DetailedErrors = true; });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.MapLoginAndLogout();

        app.MapStaticAssets();

        // Map interactive server components.
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        app.MapRazorPages();

        // Redirect unknown deep links (for example, a user pasting `/permit`) back to the shell home page.
        // This avoids returning a raw server 404 without requiring an `_Host` Razor Page.
        app.MapFallback(() => Results.Redirect("/", permanent: false));

        app.MapDefaultEndpoints();

        app.Run();
    }

    private static void ConfigureConfigurationJson(WebApplicationBuilder builder)
    {
        const string configurationFileName = "configuration.json";
        var configurationFilePath = Path.Combine(builder.Environment.ContentRootPath, configurationFileName);

        using var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "[HH:mm:ss] ";
            });
            logging.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Program>();

        if (!File.Exists(configurationFilePath))
        {
            logger.LogWarning("Configuration file '{ConfigurationFile}' was not found at '{Path}'.", configurationFileName, configurationFilePath);
            builder.Configuration.AddJsonFile(configurationFileName, optional: true, reloadOnChange: true);
            return;
        }

        try
        {
            using var stream = File.OpenRead(configurationFilePath);
            using var document = JsonDocument.Parse(stream);

            logger.LogInformation("Configuration file '{ConfigurationFile}' was parsed successfully.", configurationFileName);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Configuration file '{ConfigurationFile}' contains invalid JSON.", configurationFileName);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Configuration file '{ConfigurationFile}' could not be read.", configurationFileName);
            throw;
        }

        builder.Configuration.AddJsonFile(configurationFileName, optional: true, reloadOnChange: true);
    }
}
