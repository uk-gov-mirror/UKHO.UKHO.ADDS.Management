using Microsoft.AspNetCore.Components.Authorization;
using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Shell.Services
{
    public class ModulePageService
    {
        public event EventHandler? PagesChanged;

        public ModulePageService(IEnumerable<IModule> modules, AuthenticationStateProvider authenticationStateProvider, ModuleHealthService moduleHealthService)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _moduleHealthService = moduleHealthService;

            _allPages = [new ModulePage { Name = "Home", Path = "/", Icon = "\ue88a" }];

            foreach (var module in modules)
            {
                _allPages.AddRange(module.Pages);
            }

            _ = UpdateFilteredPagesAsync();

            _authenticationStateProvider.AuthenticationStateChanged += async _ =>
            {
                await UpdateFilteredPagesAsync();
            };

            _moduleHealthService.Changed += (_, _) =>
            {
                _ = UpdateFilteredPagesAsync();
            };

            //_allPages.Add(new ModulePage
            //{
            //    Name = "Explorer",
            //    Path = "/_dashboard/explorer",
            //    Title = "",
            //    Description = "",
            //    Icon = "\ue0c6"
            //});
        }

        private readonly List<ModulePage> _allPages;
        private readonly List<ModulePage> _filteredPages = [];
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ModuleHealthService _moduleHealthService;

        public IEnumerable<ModulePage> Pages => _filteredPages;

        private async Task UpdateFilteredPagesAsync()
        {
            System.Security.Claims.ClaimsPrincipal principal;
            try
            {
                principal = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
            }
            catch
            {
                return;
            }

            _filteredPages.Clear();
            _filteredPages.AddRange(FilterPages(_allPages, principal, _moduleHealthService));

            PagesChanged?.Invoke(this, EventArgs.Empty);
        }

        private static IEnumerable<ModulePage> FilterPages(IEnumerable<ModulePage> pages, System.Security.Claims.ClaimsPrincipal principal, ModuleHealthService moduleHealthService)
        {
            foreach (var page in pages)
            {
                if (!page.UserHasAccess(principal))
                {
                    continue;
                }

                var isUnhealthy = page.ModuleId != null && moduleHealthService.IsUnhealthy(page.ModuleId);
                var unhealthyReason = isUnhealthy ? moduleHealthService.TryGet(page.ModuleId!)?.Reason : null;

                var filtered = page;
                if (page.Children != null)
                {
                    filtered = new ModulePage
                    {
                        New = page.New,
                        Updated = page.Updated,
                        Pro = page.Pro,
                        Name = page.Name,
                        Icon = page.Icon,
                        Path = page.Path,
                        Title = page.Title,
                        Description = page.Description,
                        Expanded = page.Expanded,
                        Tags = page.Tags,
                        Toc = page.Toc,
                        RequiredRoles = page.RequiredRoles,
                        ModuleId = page.ModuleId,
                        Disabled = isUnhealthy,
                        DisabledReason = unhealthyReason,
                        Children = FilterPages(page.Children, principal, moduleHealthService).ToArray()
                    };
                }
                else if (isUnhealthy)
                {
                    filtered = new ModulePage
                    {
                        New = page.New,
                        Updated = page.Updated,
                        Pro = page.Pro,
                        Name = page.Name,
                        Icon = page.Icon,
                        Path = page.Path,
                        Title = page.Title,
                        Description = page.Description,
                        Expanded = page.Expanded,
                        Tags = page.Tags,
                        Toc = page.Toc,
                        RequiredRoles = page.RequiredRoles,
                        ModuleId = page.ModuleId,
                        Disabled = true,
                        DisabledReason = unhealthyReason,
                        Children = page.Children
                    };
                }

                yield return filtered;
            }
        }

        public ModulePage? FindCurrent(Uri uri)
        {
            IEnumerable<ModulePage> Flatten(IEnumerable<ModulePage> e)
            {
                return e.SelectMany(c => c.Children != null ? Flatten(c.Children) : new[] { c });
            }

            return Flatten(Pages)
                .FirstOrDefault(example => example.Path == uri.AbsolutePath || $"/{example.Path}" == uri.AbsolutePath);
        }

        public string TitleFor(ModulePage? example)
        {
            if (example != null && example.Name != "Overview")
            {
                return example.Title ?? "";
            }

            return "";
        }

        public string DescriptionFor(ModulePage? example) => example?.Description ?? "";
    }
}
