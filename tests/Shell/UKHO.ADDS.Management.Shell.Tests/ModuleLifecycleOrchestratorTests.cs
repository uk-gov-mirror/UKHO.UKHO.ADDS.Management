using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using UKHO.ADDS.Management.Shell.Modules;
using UKHO.ADDS.Management.Shell.Services;
using Xunit;

namespace UKHO.ADDS.Management.Shell.Tests;

public class ModuleLifecycleOrchestratorTests
{
    [Fact]
    public async Task WhenDeploymentChanges_ThenModulesAreInvokedSequentially()
    {
        var calls = new List<string>();

        var module1 = new TestModule(
            id: "m1",
            onDeploymentChanged: _ => calls.Add("m1"));

        var module2 = new TestModule(
            id: "m2",
            onDeploymentChanged: _ => calls.Add("m2"));

        var ctx = new DeploymentContext();
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        using var orchestrator = new ModuleLifecycleOrchestrator(
            new[] { module1, module2 },
            ctx,
            health,
            new NullLogger<ModuleLifecycleOrchestrator>());

        ctx.SetSelectedDeploymentId("dep");

        await SpinWaitUntilAsync(() => calls.Count == 2);

        Assert.Equal(new[] { "m1", "m2" }, calls);
    }

    [Fact]
    public async Task WhenConfigurationReloads_ThenModulesAreInvokedSequentially()
    {
        var calls = new List<string>();

        var module1 = new TestModule(
            id: "m1",
            onDeploymentChanged: _ => { },
            onConfigurationReloaded: () => calls.Add("m1"));

        var module2 = new TestModule(
            id: "m2",
            onDeploymentChanged: _ => { },
            onConfigurationReloaded: () => calls.Add("m2"));

        var ctx = new DeploymentContext();
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        using var orchestrator = new ModuleLifecycleOrchestrator(
            new[] { module1, module2 },
            ctx,
            health,
            new NullLogger<ModuleLifecycleOrchestrator>());

        await orchestrator.NotifyConfigurationReloadedAsync(CancellationToken.None);

        Assert.Equal(new[] { "m1", "m2" }, calls);
    }

    [Fact]
    public async Task WhenConfigurationReloadThrows_ThenModuleIsMarkedUnhealthy_AndSkippedOnNextReload()
    {
        var calls = new List<string>();

        var module1 = new TestModule(
            id: "m1",
            onDeploymentChanged: _ => { },
            onConfigurationReloaded: () => throw new InvalidOperationException("boom"));

        var module2 = new TestModule(
            id: "m2",
            onDeploymentChanged: _ => { },
            onConfigurationReloaded: () => calls.Add("m2"));

        var ctx = new DeploymentContext();
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        using var orchestrator = new ModuleLifecycleOrchestrator(
            new[] { module1, module2 },
            ctx,
            health,
            new NullLogger<ModuleLifecycleOrchestrator>());

        await orchestrator.NotifyConfigurationReloadedAsync(CancellationToken.None);

        Assert.True(health.IsUnhealthy("m1"));

        calls.Clear();
        await orchestrator.NotifyConfigurationReloadedAsync(CancellationToken.None);

        Assert.Equal(new[] { "m2" }, calls);
    }

    [Fact]
    public async Task WhenModuleThrows_ThenModuleIsMarkedUnhealthy_AndSkippedOnNextChange()
    {
        var calls = new List<string>();

        var module1 = new TestModule(
            id: "m1",
            onDeploymentChanged: _ => throw new InvalidOperationException("boom"));

        var module2 = new TestModule(
            id: "m2",
            onDeploymentChanged: _ => calls.Add("m2"));

        var ctx = new DeploymentContext();
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        using var orchestrator = new ModuleLifecycleOrchestrator(
            new[] { module1, module2 },
            ctx,
            health,
            new NullLogger<ModuleLifecycleOrchestrator>());

        ctx.SetSelectedDeploymentId("dep");
        await SpinWaitUntilAsync(() => calls.Count == 1);

        Assert.True(health.IsUnhealthy("m1"));

        calls.Clear();
        ctx.SetSelectedDeploymentId("dep2");
        await SpinWaitUntilAsync(() => calls.Count == 1);

        Assert.Equal(new[] { "m2" }, calls);
    }

    private static async Task SpinWaitUntilAsync(Func<bool> condition)
    {
        for (var i = 0; i < 200; i++)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(5);
        }

        throw new TimeoutException("Timed out waiting for condition.");
    }

    private sealed class TestModule : IModule
    {
        private readonly Action<string?> _onDeploymentChanged;
        private readonly Action _onConfigurationReloaded;

        public TestModule(string id, Action<string?> onDeploymentChanged, Action? onConfigurationReloaded = null)
        {
            Id = id;
            _onDeploymentChanged = onDeploymentChanged;
            _onConfigurationReloaded = onConfigurationReloaded ?? (() => { });
        }

        public string Id { get; }

        public IEnumerable<ModulePage> Pages => Array.Empty<ModulePage>();

        public Task OnDeploymentChangedAsync(string? deploymentId, CancellationToken cancellationToken)
        {
            _onDeploymentChanged(deploymentId);
            return Task.CompletedTask;
        }

        public Task OnConfigurationReloadedAsync(CancellationToken cancellationToken)
        {
            _onConfigurationReloaded();
            return Task.CompletedTask;
        }
    }
}
