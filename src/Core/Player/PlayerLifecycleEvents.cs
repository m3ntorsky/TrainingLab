using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Players;

namespace TrainingLab.Core.Player;

public sealed class PlayerLifecycleEvents : IDisposable
{
    private readonly ISwiftlyCore _core;
    private readonly PlayerSessionManager _sessions;

    private bool _initialized;

    public PlayerLifecycleEvents(ISwiftlyCore core, PlayerSessionManager sessions)
    {
        _core = core;
        _sessions = sessions;
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _core.Event.OnClientSteamAuthorize += OnClientSteamAuthorize;
        _core.Event.OnClientDisconnected += OnClientDisconnected;

        _initialized = true;

        foreach (IPlayer player in _core.PlayerManager.GetAllPlayers())
        {
            if (!player.IsValid || !player.IsAuthorized || player.IsFakeClient)
            {
                continue;
            }

            _sessions.Create(player);
        }
    }

    private void OnClientSteamAuthorize(IOnClientSteamAuthorizeEvent @event)
    {
        IPlayer? player = _core.PlayerManager.GetPlayer(@event.PlayerId);

        if (player is null || !player.IsAuthorized || player.IsFakeClient)
        {
            return;
        }

        _sessions.Create(player);
    }

    private void OnClientDisconnected(IOnClientDisconnectedEvent @event)
    {
        _sessions.Remove(@event.PlayerId);
    }

    public void Dispose()
    {
        if (!_initialized)
        {
            return;
        }

        _core.Event.OnClientSteamAuthorize -= OnClientSteamAuthorize;
        _core.Event.OnClientDisconnected -= OnClientDisconnected;

        _initialized = false;
    }
}