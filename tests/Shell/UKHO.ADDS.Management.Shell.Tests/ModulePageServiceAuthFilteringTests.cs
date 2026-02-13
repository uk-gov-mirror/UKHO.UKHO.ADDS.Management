using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using UKHO.ADDS.Management.Shell.Modules;
using UKHO.ADDS.Management.Shell.Services;
using Xunit;

namespace UKHO.ADDS.Management.Shell.Tests;

public class ModulePageServiceAuthFilteringTests
{
    [Fact]
    public async Task WhenUserHasNoRolesThenShellPagesLoad()
    {
        var modules = new[]
        {
            new TestModule(new[]
            {
                new ModulePage { Name = "Sample", Path = "/sample/main" },
                new ModulePage { Name = "Secure", Path = "/sample/secure", RequiredRoles = new[] { "showsamplepage" } }
            })
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "test"));
        var authStateProvider = new TestAuthenticationStateProvider(principal);
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        var service = new ModulePageService(modules, authStateProvider, health);

        await WaitForPagesConditionAsync(service, pages => pages.Any(p => p.Path == "/"));

        Assert.Contains(service.Pages, p => p.Path == "/");
        Assert.Contains(service.Pages, p => p.Path == "/sample/main");
        Assert.DoesNotContain(service.Pages, p => p.Path == "/sample/secure");
    }

    [Fact]
    public async Task WhenUserLacksRequiredRoleThenPagesAreFiltered()
    {
        var modules = new[]
        {
            new TestModule(new[]
            {
                new ModulePage
                {
                    Name = "Restricted",
                    Path = "/restricted",
                    RequiredRoles = new[] { "role-a" }
                }
            })
        };

        var authStateProvider = new TestAuthenticationStateProvider(new ClaimsPrincipal(new ClaimsIdentity()));
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        var service = new ModulePageService(modules, authStateProvider, health);

        await WaitForPagesConditionAsync(service, pages => pages.Any(p => p.Path == "/"));

        Assert.DoesNotContain(service.Pages, p => p.Path == "/restricted");
    }

    [Fact]
    public async Task WhenUserHasRequiredRoleThenPagesAreVisible()
    {
        var modules = new[]
        {
            new TestModule(new[]
            {
                new ModulePage
                {
                    Name = "Restricted",
                    Path = "/restricted",
                    RequiredRoles = new[] { "role-a" }
                }
            })
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "role-a") }, "test"));
        var authStateProvider = new TestAuthenticationStateProvider(principal);
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());

        var service = new ModulePageService(modules, authStateProvider, health);

        await WaitForPagesConditionAsync(service, pages => pages.Any(p => p.Path == "/"));

        Assert.Contains(service.Pages, p => p.Path == "/restricted");
    }

    [Fact]
    public async Task WhenAuthStateChangesThenPagesChangedIsRaised()
    {
        var modules = new[]
        {
            new TestModule(new[]
            {
                new ModulePage
                {
                    Name = "Restricted",
                    Path = "/restricted",
                    RequiredRoles = new[] { "role-a" }
                }
            })
        };

        var authStateProvider = new TestAuthenticationStateProvider(new ClaimsPrincipal(new ClaimsIdentity()));
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());
        var service = new ModulePageService(modules, authStateProvider, health);

        await WaitForPagesConditionAsync(service, pages => pages.Any(p => p.Path == "/"));

        var raised = false;
        service.PagesChanged += (_, _) => raised = true;

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "role-a") }, "test"));
        authStateProvider.SetPrincipal(principal);

        Assert.True(raised);
    }

    [Fact]
    public async Task WhenCurrentPageIsFilteredOutThenFindCurrentReturnsNull()
    {
        var modules = new[]
        {
            new TestModule(new[]
            {
                new ModulePage
                {
                    Name = "Restricted",
                    Path = "/restricted",
                    RequiredRoles = new[] { "role-a" }
                }
            })
        };

        var authStateProvider = new TestAuthenticationStateProvider(new ClaimsPrincipal(new ClaimsIdentity()));
        var health = new ModuleHealthService(new NullLogger<ModuleHealthService>());
        var service = new ModulePageService(modules, authStateProvider, health);

        await WaitForPagesConditionAsync(service, pages => pages.Any(p => p.Path == "/"));

        var current = service.FindCurrent(new Uri("http://localhost/restricted"));

        Assert.Null(current);
    }

    private static async Task WaitForPagesConditionAsync(ModulePageService service, Func<IEnumerable<ModulePage>, bool> predicate)
    {
        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(2);

        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            if (predicate(service.Pages))
            {
                return;
            }

            await Task.Delay(10);
        }

        throw new TimeoutException("Timed out waiting for ModulePageService pages to update.");
    }

    private sealed class TestModule : IModule
    {
        public TestModule(IEnumerable<ModulePage> pages) => Pages = pages;

        public string Id => "test";

        public IEnumerable<ModulePage> Pages { get; }
    }

    private sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _principal;

        public TestAuthenticationStateProvider(ClaimsPrincipal principal) => _principal = principal;

        public void SetPrincipal(ClaimsPrincipal principal)
        {
            _principal = principal;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_principal));
    }
}
