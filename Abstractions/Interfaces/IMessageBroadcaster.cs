using ChatAsyncServerSqlLite.Core.Sessions;

namespace ChatAsyncServerSqlLite.Abstractions.Interfaces;

public interface IMessageBroadcaster
{
    Task BroadcastAsync(string data, ClientSession sender);
}