using Microsoft.Extensions.DependencyInjection;
using TrainingLab.Core.Events;
using TrainingLab.Core.Sessions;

namespace TrainingLab.Core.Hosting;

internal static class TrainingLabCoreInjection
{
    public static IServiceCollection AddTrainingLab(this IServiceCollection self)
    {
        self.AddSingleton<PlayerSessionManager>();
        self.AddSingleton<PlayerLifecycleEvents>();
        return self;
    }

    public static IServiceProvider UseTrainingLab(this IServiceProvider self)
    {
        self.GetRequiredService<PlayerLifecycleEvents>().Initialize();
        return self;
    }
}