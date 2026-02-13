using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace UKHO.ADDS.Management.Shell.Services;

public sealed class ConfigurationReloadNotifier : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ModuleLifecycleOrchestrator _orchestrator;
    private readonly CancellationTokenSource _cts = new();
    private IDisposable? _registration;

    public ConfigurationReloadNotifier(IConfiguration configuration, ModuleLifecycleOrchestrator orchestrator)
    {
        _configuration = configuration;
        _orchestrator = orchestrator;

        Register();
    }

    private void Register()
    {
        if (_configuration is not IConfigurationRoot root)
        {
            return;
        }

        _registration = ChangeToken.OnChange(
            root.GetReloadToken,
            () => _ = _orchestrator.NotifyConfigurationReloadedAsync(_cts.Token));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _registration?.Dispose();
        _cts.Dispose();
    }
}
