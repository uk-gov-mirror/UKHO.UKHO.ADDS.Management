using Microsoft.Extensions.Logging;

namespace UKHO.ADDS.Management.Shell.Services;

public sealed class ModuleHealthService
{
    private readonly ILogger<ModuleHealthService> _logger;
    private readonly Dictionary<string, ModuleHealthState> _states = new(StringComparer.Ordinal);

    public event EventHandler? Changed;

    public ModuleHealthService(ILogger<ModuleHealthService> logger)
    {
        _logger = logger;
    }

    public bool IsUnhealthy(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return false;
        }

        return _states.TryGetValue(moduleId, out var state) && state.IsUnhealthy;
    }

    public ModuleHealthState? TryGet(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return null;
        }

        return _states.TryGetValue(moduleId, out var state) ? state : null;
    }

    public void MarkUnhealthy(string moduleId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);

        var newState = new ModuleHealthState(IsUnhealthy: true, Reason: string.IsNullOrWhiteSpace(reason) ? null : reason);
        if (_states.TryGetValue(moduleId, out var existing) && existing.Equals(newState))
        {
            return;
        }

        _states[moduleId] = newState;
        _logger.LogWarning("Module '{ModuleId}' marked unhealthy. Reason: {Reason}", moduleId, newState.Reason);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void MarkHealthy(string moduleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);

        if (_states.TryGetValue(moduleId, out var existing) && existing.IsUnhealthy)
        {
            _states[moduleId] = existing with { IsUnhealthy = false, Reason = null };
            _logger.LogInformation("Module '{ModuleId}' marked healthy.", moduleId);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}

public readonly record struct ModuleHealthState(bool IsUnhealthy, string? Reason);
