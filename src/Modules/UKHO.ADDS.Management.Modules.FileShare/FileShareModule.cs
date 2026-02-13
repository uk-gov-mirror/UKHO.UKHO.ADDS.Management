using UKHO.ADDS.Management.Shell.Modules;

namespace UKHO.ADDS.Management.Modules.FileShare;

public sealed class FileShareModule : IModule
{
    public string Id => "FileShare";

    public IEnumerable<ModulePage> Pages =>
    [
        new ModulePage
        {
            Name = "File Share",
            Path = "/fileshare",
            Icon = "\ue2c8",
            ModuleId = Id,
            RequiredRoles = ["fileshareuser"]
        }
    ];
}
