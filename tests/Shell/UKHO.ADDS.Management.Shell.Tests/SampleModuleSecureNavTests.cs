using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using UKHO.ADDS.Management.Modules.Samples;
using UKHO.ADDS.Management.Shell.Services;
using Xunit;

namespace UKHO.ADDS.Management.Shell.Tests;

public class SampleModuleSecureNavTests
{
    [Fact]
    public void WhenUserLacksShowsamplepageRole_ThenSecurePageIsNotInNavigation()
    {
        var module = new SampleModule();
        var auth = new TestAuthenticationStateProvider(new ClaimsPrincipal(new ClaimsIdentity()));
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        var svc = new ModulePageService(new[] { module }, auth, health);

        Assert.DoesNotContain(Flatten(svc.Pages), p => p.Path == "/sample/secure");
    }

    [Fact]
    public void WhenUserHasShowsamplepageRole_ThenSecurePageIsInNavigation()
    {
        var module = new SampleModule();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "showsamplepage") }, "test"));
        var auth = new TestAuthenticationStateProvider(principal);
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        var svc = new ModulePageService(new[] { module }, auth, health);

        Assert.Contains(Flatten(svc.Pages), p => p.Path == "/sample/secure");
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
