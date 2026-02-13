namespace UKHO.ADDS.Management.Shell.Modules
{
    public class ModulePage
    {
        public bool New { get; set; }
        public bool Updated { get; set; }
        public bool Pro { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Expanded { get; set; }
        public IEnumerable<ModulePage> Children { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<ModulePageSection> Toc { get; set; }

        public IReadOnlyCollection<string>? RequiredRoles { get; set; }

        public string? ModuleId { get; set; }

        public bool Disabled { get; set; }

        public string? DisabledReason { get; set; }

        public bool UserHasAccess(System.Security.Claims.ClaimsPrincipal principal)
        {
            if (principal is null)
            {
                return false;
            }

            if (RequiredRoles is null || RequiredRoles.Count == 0)
            {
                return true;
            }

            return RequiredRoles.Any(principal.IsInRole);
        }
    }
}
