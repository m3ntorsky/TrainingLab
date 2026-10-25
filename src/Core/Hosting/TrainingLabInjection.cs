using Microsoft.Extensions.DependencyInjection;

namespace TrainingLab.Core.Hosting;

internal static class TrainingLabCoreInjection
{
    public static IServiceCollection AddTrainingLab(this IServiceCollection self)
    {
        return self;
    }

    public static IServiceProvider UseTrainingLab(this IServiceProvider self)
    {
        return self;
    }
}