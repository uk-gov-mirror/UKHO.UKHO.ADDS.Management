namespace UKHO.ADDS.Management.Shell.Modules
{
    public interface IModule
    {
        public string Id { get; }

        public IEnumerable<ModulePage> Pages { get; }

        public Task OnDeploymentChangedAsync(string? deploymentId, CancellationToken cancellationToken);

        public Task OnConfigurationReloadedAsync(CancellationToken cancellationToken);
    }
}
