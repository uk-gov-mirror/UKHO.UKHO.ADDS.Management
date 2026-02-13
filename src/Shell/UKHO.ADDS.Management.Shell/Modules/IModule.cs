namespace UKHO.ADDS.Management.Shell.Modules
{
    public interface IModule
    {
        public string Id { get; }

        public IEnumerable<ModulePage> Pages { get; }

        public Task OnDeploymentChangedAsync(string? deploymentId, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task OnConfigurationReloadedAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
