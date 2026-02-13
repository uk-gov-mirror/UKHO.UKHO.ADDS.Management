using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Modules.Permit;

public sealed class PermitModule : IModule
{
    public string Id => "Permit";

    public IEnumerable<ModulePage> Pages =>
    [
        new ModulePage
        {
            Name = "Permit",
            Path = "/permit",
            Icon = "\uef63",
            ModuleId = Id,
            RequiredRoles = ["permitserviceuser"]
        }
    ];

    public Task OnDeploymentChangedAsync(string? deploymentId, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task OnConfigurationReloadedAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
