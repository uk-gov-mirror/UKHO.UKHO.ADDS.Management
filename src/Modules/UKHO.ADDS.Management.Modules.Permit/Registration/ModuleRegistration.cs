using Microsoft.Extensions.DependencyInjection;
using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Modules.Permit.Registration;

public static class ModuleRegistration
{
    public static IServiceCollection AddPermitModule(this IServiceCollection services)
    {
        services.AddSingleton<IModule>(new PermitModule());

        return services;
    }
}
