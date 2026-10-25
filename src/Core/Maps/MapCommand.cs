using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;

namespace TrainingLab.Core.Maps;

public sealed class MapCommand
{
    private readonly ISwiftlyCore _core;
    private readonly MapService _mapService;

    public MapCommand(ISwiftlyCore core, MapService mapService)
    {
        _core = core;
        _mapService = mapService;

        core.Registrator.Register(this);
    }

    [Command("map", permission: "traininglab.admin.map", helpText: "Change the current map")]
    public void OnMapCommand(ICommandContext context)
    {
        if (context.Args.Length != 1)
        {
            context.Reply("Usage: !map <mapname>");
            return;
        }

        string mapName = context.Args[0];

        if (!_mapService.TryChangeMap(mapName))
        {
            context.Reply("Invalid map name.");
        }
    }
}