using System.Text.RegularExpressions;
using SwiftlyS2.Shared;

namespace TrainingLab.Core.Maps;

public sealed class MapService
{
    private readonly ISwiftlyCore _core;

    public MapService(ISwiftlyCore core)
    {
        _core = core;
    }

    public bool TryChangeMap(string mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
        {
            return false;
        }

        string input = mapName.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(input, @"^[a-z0-9_]+$"))
        {
            return false;
        }

        string[] candidates =
        [
            input,
            $"de_{input}",
            $"cs_{input}"
        ];

        foreach (string candidate in candidates)
        {
            if (!_core.Engine.IsMapValid(candidate))
                continue;

            _core.Engine.ExecuteCommand($"changelevel {candidate}");
            return true;
        }

        return false;
    }
}