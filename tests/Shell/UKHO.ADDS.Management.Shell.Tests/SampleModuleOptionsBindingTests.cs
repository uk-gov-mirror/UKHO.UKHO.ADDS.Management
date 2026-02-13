using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using UKHO.ADDS.Management.Shell.Configuration;
using Xunit;

namespace UKHO.ADDS.Management.Shell.Tests;

public class SampleModuleOptionsBindingTests
{
    [Fact]
    public void GetOptions_BindsDeveloperModuleOptionsConfiguredValues()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Deployments:dev:Modules:Developer:BaseUrl"] = "https://dev.api.example.com/",
                ["Deployments:dev:Modules:Developer:DisplayName"] = "Developer (Dev)",
                ["Deployments:dev:Modules:Developer:TimeoutSeconds"] = "30"
            })
            .Build();

        var provider = new ModuleConfigurationProvider(configuration, NullLogger<ModuleConfigurationProvider>.Instance);

        var options = provider.GetOptions<TestSampleModuleOptions>("dev", "Developer");

        Assert.Equal("https://dev.api.example.com/", options.BaseUrl);
        Assert.Equal("Developer (Dev)", options.DisplayName);
        Assert.Equal(30, options.TimeoutSeconds);
    }

    [Fact]
    public void GetOptions_ReturnsDefaultDeveloperModuleOptionsWhenSectionMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var provider = new ModuleConfigurationProvider(configuration, NullLogger<ModuleConfigurationProvider>.Instance);

        var options = provider.GetOptions<TestSampleModuleOptions>("dev", "Developer");

        Assert.Null(options.BaseUrl);
        Assert.Null(options.DisplayName);
        Assert.Equal(default, options.TimeoutSeconds);
    }

    private class TestSampleModuleOptions
    {
        public string? BaseUrl { get; set; }
        public string? DisplayName { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}
