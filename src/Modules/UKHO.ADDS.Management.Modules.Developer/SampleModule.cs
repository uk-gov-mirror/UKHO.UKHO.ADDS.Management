using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Modules.Developer
{
    public class DeveloperModule : IModule
    {
        public string Id => "developer-module";

        public IEnumerable<ModulePage> Pages => [new()
        {
            Name = "Developer",
            Path = "/developer/main",
            Icon = "\ue88a",
            ModuleId = "Developer",
            RequiredRoles = ["developer"],
            Children =
            [
                new() { Name = "sub page", Path = "/developer/sub", ModuleId = "Developer", RequiredRoles = ["developer"] },
                new() { Name = "development", Path = "/developer/development", ModuleId = "Developer", RequiredRoles = ["developer"] },
                new()
                {
                    Name = "secure",
                    Path = "/developer/secure",
                    ModuleId = "Developer",
                    RequiredRoles = ["developer"]
                }
            ]
        }];

        public Task OnDeploymentChangedAsync(string? deploymentId, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task OnConfigurationReloadedAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
