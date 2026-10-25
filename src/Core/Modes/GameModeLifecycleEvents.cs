using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;

namespace TrainingLab.Core.Modes;

public sealed class GameModeLifecycleEvents : IDisposable
{
    private readonly ISwiftlyCore _core;
    private readonly GameModeManager _manager;
    private readonly ILogger<GameModeLifecycleEvents> _logger;
    private GameModeDescriptor? _configAfterMapLoad;
    private bool _initialized;
    private bool _disposed;

    public GameModeLifecycleEvents(ISwiftlyCore core, GameModeManager manager,
        ILogger<GameModeLifecycleEvents> logger)
    {
        _core = core;
        _manager = manager;
        _logger = logger;
    }

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized)
        {
            return;
        }

        _core.Event.OnMapUnload += OnMapUnload;
        _core.Event.OnMapLoad += OnMapLoad;
        _initialized = true;
    }

    private void OnMapUnload(IOnMapUnloadEvent @event)
    {
        _configAfterMapLoad = _manager.ActiveMode?.Descriptor ?? _configAfterMapLoad;
        try
        {
            _manager.Deactivate();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop game mode during unload of map '{Map}'.", @event.MapName);
        }
    }

    private void OnMapLoad(IOnMapLoadEvent @event)
    {
        GameModeDescriptor? descriptor = _manager.ActiveMode?.Descriptor ?? _configAfterMapLoad;
        _configAfterMapLoad = null;
        if (descriptor is not null)
        {
            _manager.ApplyServerConfig(descriptor);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _configAfterMapLoad = null;
        if (!_initialized)
        {
            return;
        }

        _core.Event.OnMapUnload -= OnMapUnload;
        _core.Event.OnMapLoad -= OnMapLoad;
        _initialized = false;
    }
}
