using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TcpChatServer.Core.Sessions;

public class SessionManager
{
    private readonly ConcurrentDictionary<Guid, ClientSession> _sessions = new();

    private readonly ILogger<SessionManager> _logger;

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger;
    }

    public void Add(ClientSession session)
    {
        bool added = _sessions.TryAdd(
            session.SessionId,
            session);

        if (added)
        {
            _logger.LogInformation(
                "Session {SessionId} connected. Total: {Count}",
                session.SessionId,
                _sessions.Count);
        }
    }

    public void Remove(Guid sessionId)
    {
        bool removed = _sessions.TryRemove(
            sessionId,
            out _);

        if (removed)
        {
            _logger.LogInformation(
                "Session {SessionId} disconnected. Total: {Count}",
                sessionId,
                _sessions.Count);
        }
    }

    public IReadOnlyCollection<ClientSession> GetAll()
    {
        return _sessions.Values.ToList();
    }

    public ClientSession? GetBySessionId(Guid sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);

        return session;
    }
}