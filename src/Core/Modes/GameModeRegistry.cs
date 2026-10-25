using Microsoft.Extensions.Logging;

namespace TrainingLab.Core.Modes;

public sealed class GameModeRegistry
{
    private readonly Dictionary<string, IGameMode> _modes;
    private readonly IGameMode[] _all;

    public GameModeRegistry(IEnumerable<IGameMode> modes)
    {
        ArgumentNullException.ThrowIfNull(modes);

        _all = [.. modes];
        _modes = new Dictionary<string, IGameMode>(_all.Length, StringComparer.Ordinal);

        foreach (IGameMode mode in _all)
        {
            ArgumentNullException.ThrowIfNull(mode);
            string id = mode.Descriptor.Id;

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException($"Game mode '{mode.GetType().FullName}' has an empty descriptor Id");
            }

            if (!_modes.TryAdd(id, mode))
            {
                throw new InvalidOperationException($"Duplicated game mode detected '{id}'");
            }
        }
    }

    public IReadOnlyList<IGameMode> _All => _all;

    public int Count => _modes.Count;

    public bool TryGet(string modeId, out IGameMode? mode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modeId);
        return _modes.TryGetValue(modeId, out mode);
    }

    public IGameMode GetRequired(string modeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modeId);
        if (_modes.TryGetValue(modeId, out IGameMode? mode))
        {
            return mode;
        }
        throw new KeyNotFoundException($"Game mode '{modeId} is not registered.");
    }
}