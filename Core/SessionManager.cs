using ChatAsyncServerSqlLite.Contracts;
using Microsoft.Extensions.Logging;

namespace ChatAsyncServerSqlLite.Core;

public class SessionManager
{
    private readonly List<ClientSession> _sessions = new List<ClientSession>();
    private readonly object _lock = new object();
    private readonly ILogger<SessionManager> _logger;

    public SessionManager(ILogger<SessionManager> logger)
    {
        this._logger = logger;
    }

    public void Add(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Add(session);
        }

        _logger.LogInformation(
            "Session added. Total: {Count}",
            _sessions.Count);
    }

    public void Remove(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Remove(session);
        }

        _logger.LogInformation(
            "Session removed. Total: {Count}",
            _sessions.Count);
    }

    public List<ClientSession> GetAll()
    {
        lock (_lock)
        {
            return _sessions.ToList();
        }
    }
}