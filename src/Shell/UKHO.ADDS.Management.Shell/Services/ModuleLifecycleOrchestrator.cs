using Microsoft.Extensions.Logging;
using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Shell.Services;

public sealed class ModuleLifecycleOrchestrator : IDisposable
{
    private readonly IEnumerable<IModule> _modules;
    private readonly DeploymentContext _deploymentContext;
    private readonly ModuleHealthService _health;
    private readonly ILogger<ModuleLifecycleOrchestrator> _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _gate = new(1, 1);

    public ModuleLifecycleOrchestrator(
        IEnumerable<IModule> modules,
        DeploymentContext deploymentContext,
        ModuleHealthService health,
        ILogger<ModuleLifecycleOrchestrator> logger)
    {
        _modules = modules;
        _deploymentContext = deploymentContext;
        _health = health;
        _logger = logger;

        _deploymentContext.Changed += OnDeploymentChanged;
    }

    private void OnDeploymentChanged(object? sender, DeploymentChangedEventArgs e)
    {
        _ = HandleDeploymentChangedAsync(e.DeploymentId, _cts.Token);
    }

    public Task NotifyConfigurationReloadedAsync(CancellationToken cancellationToken)
        => HandleConfigurationReloadedAsync(cancellationToken);

    private async Task HandleDeploymentChangedAsync(string? deploymentId, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            foreach (var module in _modules)
            {
                if (_health.IsUnhealthy(module.Id))
                {
                    continue;
                }
                try
                {
                    await module.OnDeploymentChangedAsync(deploymentId, cancellationToken);
                    _health.MarkHealthy(module.Id);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Deployment change lifecycle failed for module '{ModuleId}'.", module.Id);
                    _health.MarkUnhealthy(module.Id, ex.Message);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task HandleConfigurationReloadedAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            foreach (var module in _modules)
            {
                if (_health.IsUnhealthy(module.Id))
                {
                    continue;
                }

                try
                {
                    await module.OnConfigurationReloadedAsync(cancellationToken);
                    _health.MarkHealthy(module.Id);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Configuration reload lifecycle failed for module '{ModuleId}'.", module.Id);
                    _health.MarkUnhealthy(module.Id, ex.Message);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _deploymentContext.Changed -= OnDeploymentChanged;
        _cts.Cancel();
        _cts.Dispose();
        _gate.Dispose();
    }
}
