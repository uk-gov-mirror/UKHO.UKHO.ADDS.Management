using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using UKHO.ADDS.Management.Modules.Permit;
using UKHO.ADDS.Management.Shell.Services;
using Xunit;

namespace UKHO.ADDS.Management.Shell.Tests;

public class PermitModuleNavTests
{
    [Fact]
    public void WhenUserLacksPermitserviceuserRole_ThenPermitPageIsNotInNavigation()
    {
        var module = new PermitModule();
        var auth = new TestAuthenticationStateProvider(new ClaimsPrincipal(new ClaimsIdentity()));
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        var svc = new ModulePageService(new[] { module }, auth, health);

        Assert.DoesNotContain(Flatten(svc.Pages), p => p.Path == "/permit");
    }

    [Fact]
    public void WhenUserHasPermitserviceuserRole_ThenPermitPageIsInNavigation()
    {
        var module = new PermitModule();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "permitserviceuser") }, "test"));
        var auth = new TestAuthenticationStateProvider(principal);
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        var svc = new ModulePageService(new[] { module }, auth, health);

        Assert.Contains(Flatten(svc.Pages), p => p.Path == "/permit");
    }

    private static IEnumerable<UKHO.ADDS.Management.Shell.Modules.ModulePage> Flatten(IEnumerable<UKHO.ADDS.Management.Shell.Modules.ModulePage> pages)
    {
        foreach (var page in pages)
        {
            yield return page;
            if (page.Children is null)
            {
                continue;
            }

            foreach (var child in Flatten(page.Children))
            {
                yield return child;
            }
        }
    }

    private sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _principal;

        public TestAuthenticationStateProvider(ClaimsPrincipal principal) => _principal = principal;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_principal));
    }
}
