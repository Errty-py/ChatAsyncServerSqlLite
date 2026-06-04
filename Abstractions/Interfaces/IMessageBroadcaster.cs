using SpaceChatServer.Core.Sessions;

namespace SpaceChatServer.Abstractions.Interfaces;

public interface ITcpBroadcaster
{
    Task BroadcastAsync(ClientSession sender, string data);
}