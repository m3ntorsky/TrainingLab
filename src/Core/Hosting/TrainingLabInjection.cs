using Microsoft.Extensions.DependencyInjection;
using TrainingLab.Core.Maps;
using TrainingLab.Core.Modes;
using TrainingLab.Core.Player;

namespace TrainingLab.Core.Hosting;

internal static class TrainingLabCoreInjection
{
    public static IServiceCollection AddTrainingLab(this IServiceCollection self)
    {
        // Player
        self.AddSingleton<PlayerSessionManager>();
        self.AddSingleton<PlayerLifecycleEvents>();
        // Maps
        self.AddSingleton<MapService>();
        self.AddSingleton<MapCommand>();
        // Modes
        self.AddSingleton<GameModeRegistry>();
        self.AddSingleton<GameModeManager>();
        self.AddSingleton<GameModeLifecycleEvents>();
        return self;
    }

    public static IServiceProvider UseTrainingLab(this IServiceProvider self)
    {
        // Player
        self.GetRequiredService<PlayerLifecycleEvents>().Initialize();
        // Maps
        _ = self.GetRequiredService<MapCommand>();
        // Modes
        self.GetRequiredService<GameModeLifecycleEvents>().Initialize();
        return self;
    }
}
