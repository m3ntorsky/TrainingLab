using Microsoft.Extensions.DependencyInjection;
using TrainingLab.Core.Maps;
using TrainingLab.Core.Player;

namespace TrainingLab.Core.Hosting;

internal static class TrainingLabCoreInjection
{
    public static IServiceCollection AddTrainingLab(this IServiceCollection self)
    {
        self.AddSingleton<PlayerSessionManager>();
        self.AddSingleton<PlayerLifecycleEvents>();
        self.AddSingleton<MapService>();
        self.AddSingleton<MapCommand>();
        return self;
    }

    public static IServiceProvider UseTrainingLab(this IServiceProvider self)
    {
        self.GetRequiredService<PlayerLifecycleEvents>().Initialize();
        _ = self.GetRequiredService<MapCommand>();
        return self;
    }
}