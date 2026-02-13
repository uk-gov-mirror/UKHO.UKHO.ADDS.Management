using Microsoft.Extensions.DependencyInjection;
using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Modules.Developer.Registration;

public static class ModuleRegistration
{
    public static IServiceCollection AddDeveloperModule(this IServiceCollection collection)
    {
        collection.AddSingleton<IModule>(new DeveloperModule());

        return collection;
    }
}
