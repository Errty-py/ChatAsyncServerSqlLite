using ChatAsyncServerSqlLite.Contracts;

namespace ChatAsyncServerSqlLite.Abstractions.Interfaces;

public interface IMessageBroadcaster
{
    Task BroadcastAsync(byte[] data, ClientSession sender);
}