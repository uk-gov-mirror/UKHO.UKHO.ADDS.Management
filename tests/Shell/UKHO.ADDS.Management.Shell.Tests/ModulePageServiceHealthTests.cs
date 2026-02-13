using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using UKHO.ADDS.Management.Shell.Modules;
using UKHO.ADDS.Management.Shell.Services;
using Xunit;

namespace UKHO.ADDS.Management.Shell.Tests;

public class ModulePageServiceHealthTests
{
    [Fact]
    public void WhenModuleIsUnhealthyThenPagesAreDisabledButVisible()
    {
        var page = new ModulePage
        {
            Name = "Module Root",
            Path = "/module",
            ModuleId = "mod"
        };

        var modules = new[] { new TestModule(new[] { page }) };
        var authStateProvider = new TestAuthenticationStateProvider(new ClaimsPrincipal(new ClaimsIdentity()));
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        health.MarkUnhealthy("mod", "boom");

        var svc = new ModulePageService(modules, authStateProvider, health);

        var visible = Assert.Single(svc.Pages, p => p.Path == "/module");
        Assert.True(visible.Disabled);
        Assert.Equal("boom", visible.DisabledReason);
    }

    private sealed class TestModule : IModule
    {
        public TestModule(IEnumerable<ModulePage> pages) => Pages = pages;

        public string Id => "mod";

        public IEnumerable<ModulePage> Pages { get; }
    }

    private sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _principal;

        public TestAuthenticationStateProvider(ClaimsPrincipal principal) => _principal = principal;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_principal));
    }
}
