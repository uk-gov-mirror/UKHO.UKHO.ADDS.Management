using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Modules.Samples
{
    public class SampleModule : IModule
    {
        public string Id => "sample-module";

        public IEnumerable<ModulePage> Pages => [new()
        {
            Name = "Sample",
            Path = "/sample/main",
            Icon = "\ue88a",
            ModuleId = "Samples",
            Children =
            [
                new() { Name = "sub page", Path = "/sample/sub", ModuleId = "Samples" },
                new()
                {
                    Name = "secure",
                    Path = "/sample/secure",
                    ModuleId = "Samples",
                    RequiredRoles = ["showsamplepage"]
                }
            ]
        }];
    }
}
