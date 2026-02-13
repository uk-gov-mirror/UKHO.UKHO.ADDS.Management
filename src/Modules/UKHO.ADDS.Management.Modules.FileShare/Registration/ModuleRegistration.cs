using Microsoft.Extensions.DependencyInjection;
using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Modules.FileShare.Registration;

public static class ModuleRegistration
{
    public static IServiceCollection AddFileShareModule(this IServiceCollection services)
    {
        services.AddSingleton<IModule>(new FileShareModule());

        return services;
    }
}
