namespace TrainingLab.Core.Player;


public sealed class PlayerSession
{
    public required ulong SessionId { get; init; }
    public required ulong SteamId { get; init; }

}