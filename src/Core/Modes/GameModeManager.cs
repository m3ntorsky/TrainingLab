using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;

namespace TrainingLab.Core.Modes;

public sealed class GameModeManager : IDisposable
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<GameModeManager> _logger;
    private readonly GameModeRegistry _registry;
    private bool _disposed;
    public GameModeManager(ISwiftlyCore core, ILogger<GameModeManager> logger, GameModeRegistry registry)
    {
        _core = core;
        _logger = logger;
        _registry = registry;
    }

    public IGameMode? ActiveMode { get; private set; }

    public ServerPhase Phase { get; private set; } = ServerPhase.Lobby;

    public void Activate(string modeId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (Phase is ServerPhase.Starting or ServerPhase.Stopping)
        {
            throw new InvalidOperationException("A game mode transition is already in progress.");
        }

        IGameMode mode = _registry.GetRequired(modeId);
        if (ActiveMode is not null)
        {
            if (ReferenceEquals(ActiveMode, mode))
            {
                ApplyServerConfig(mode.Descriptor);
                return;
            }

            Deactivate();
        }

        Phase = ServerPhase.Starting;
        try
        {
            mode.Start();
            ActiveMode = mode;

            ApplyServerConfig(mode.Descriptor);

            Phase = ServerPhase.Running;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate gamemode '{ModeId}'", mode.Descriptor.Id);
            try
            {
                mode.Stop();
            }
            catch (Exception cleanupException)
            {
                throw new AggregateException("Mode start and cleanup failed.", ex, cleanupException);
            }
            finally
            {
                ActiveMode = null;
                Phase = ServerPhase.Lobby;
            }
            throw;
        }
        _logger.LogInformation("Game mode activated: {CurrentMode}", mode.Descriptor.Id);
    }
    public void Deactivate()
    {
        IGameMode? mode = ActiveMode;

        if (mode is null || Phase == ServerPhase.Stopping)
        {
            return;
        }
        Phase = ServerPhase.Stopping;
        try
        {
            mode.Stop();
        }
        finally
        {
            ActiveMode = null;
            Phase = ServerPhase.Lobby;
        }

        _logger.LogInformation("Game mode {ModeId} deactivated.", mode.Descriptor.Id);
    }

    internal void ApplyServerConfig(GameModeDescriptor descriptor)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (string.IsNullOrWhiteSpace(descriptor.ServerConfig))
        {
            return;
        }

        string command = $"exec traininglab/{descriptor.ServerConfig}.cfg";
        _logger.LogInformation("Executing server config for '{ModeId}': {Command}", descriptor.Id, command);
        _core.Engine.ExecuteCommand(command);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Deactivate();
    }

}

