using Microsoft.Extensions.DependencyInjection;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;
using TrainingLab.Core.Hosting;


namespace TrainingLab;

[PluginMetadata(
    Id = "TrainingLab",
    Name = "TrainingLab",
    Description = "Advanced CS2 training plugin for custom practice modes, HUDs, sessions, and player training tools.",
    Author = "m3ntor",
    Version = "1.0.0"
)]

public sealed class Entrypoint : BasePlugin
{
    private ServiceProvider? _provider;

    public Entrypoint(ISwiftlyCore core) : base(core) { }

    public override void Load(bool hotReload)
    {
        var services = new ServiceCollection();
        services
            .AddSwiftly(Core)
            .AddTrainingLab();

        _provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
        try
        {
            _provider.UseTrainingLab();
        }
        catch (Exception loadException)
        {
            ServiceProvider provider = _provider;
            _provider = null;
            try
            {
                provider?.Dispose();
            }
            catch (Exception cleanupException)
            {
                throw new AggregateException("Plugin load and container cleanup failed.", loadException, cleanupException);
            }
            throw;
        }
    }

    public override void Unload()
    {
        ServiceProvider? provider = _provider;
        _provider = null;
        provider?.Dispose();
    }
}
