using Microsoft.Extensions.Configuration;
using UKHO.ADDS.Management.Modules.Samples.Configuration;
using UKHO.ADDS.Management.Shell.Configuration;

namespace UKHO.ADDS.Management.Modules.Samples.Pages;

public static class SamplePageState
{
    public static (SampleModuleOptions Options, string? ErrorMessage) BindOptions(IModuleConfigurationProvider configurationProvider, string deploymentId, string moduleId)
    {
        ArgumentNullException.ThrowIfNull(configurationProvider);

        if (string.IsNullOrWhiteSpace(deploymentId))
        {
            return (new SampleModuleOptions(), $"Invalid deployment id for module '{moduleId}'.");
        }

        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return (new SampleModuleOptions(), "Invalid module id.");
        }

        var section = configurationProvider.GetSection(deploymentId, moduleId);
        if (!section.Exists())
        {
            return (new SampleModuleOptions(), $"Missing configuration for deployment '{deploymentId}' and module '{moduleId}'.");
        }

        var options = configurationProvider.GetOptions<SampleModuleOptions>(deploymentId, moduleId);
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return (options, $"Invalid configuration for deployment '{deploymentId}' and module '{moduleId}': BaseUrl is required.");
        }

        return (options, null);
    }
}
