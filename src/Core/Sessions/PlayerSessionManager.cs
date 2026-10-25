using System.Diagnostics.CodeAnalysis;
using SwiftlyS2.Shared.Players;

namespace TrainingLab.Core.Sessions;

public sealed class PlayerSessionManager
{
    private readonly Dictionary<int, PlayerSession> _sessions = [];

    public int Count => _sessions.Count;

    public IEnumerable<PlayerSession> Sessions => _sessions.Values;

    public PlayerSession Create(IPlayer player)
    {
        if (_sessions.TryGetValue(player.PlayerID, out PlayerSession? existing) && existing.SessionId == player.SessionId)
        {
            return existing;
        }

        var session = new PlayerSession
        {
            SessionId = player.SessionId,
            SteamId = player.SteamID
        };

        _sessions[player.PlayerID] = session;

        return session;
    }

    public bool TryGet(int playerId, [NotNullWhen(true)] out PlayerSession? session)
    {
        return _sessions.TryGetValue(playerId, out session);
    }

    public bool TryGet(int playerId, ulong sessionId, [NotNullWhen(true)] out PlayerSession? session)
    {
        if (_sessions.TryGetValue(playerId, out PlayerSession? current) && current.SessionId == sessionId)
        {
            session = current;
            return true;
        }

        session = null;
        return false;
    }

    public bool Remove(int playerId)
    {
        return _sessions.Remove(playerId);
    }

    public bool Remove(int playerId, ulong sessionId)
    {
        if (!TryGet(playerId, sessionId, out _))
        {
            return false;
        }

        return _sessions.Remove(playerId);
    }

    public void Clear()
    {
        _sessions.Clear();
    }
}