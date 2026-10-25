namespace TrainingLab.Core.Modes;

public sealed record GameModeDescriptor(
    string Id,
    string DisplayName,
    string Description,
    string? ServerConfig = null
);