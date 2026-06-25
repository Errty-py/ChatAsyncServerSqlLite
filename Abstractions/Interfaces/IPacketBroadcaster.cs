using SpaceChatServer.Core.Sessions;

namespace SpaceChatServer.Abstractions.Interfaces;

public interface IPacketBroadcaster
{
    Task BroadcastAsync(ClientSession sender, string data);
}