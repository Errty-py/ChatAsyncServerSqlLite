using ChatAsyncServerSqlLite.Contracts;

namespace ChatAsyncServerSqlLite.Core
{
    public class SessionManager
    {
        private readonly List<ClientSession> _sessions = [];
        private readonly object _lock = new();

        public void Add(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
            }
        }

        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }

        public List<ClientSession> GetAll()
        {
            lock (_lock)
            {
                return _sessions.ToList();
            }
        }
    }
}
