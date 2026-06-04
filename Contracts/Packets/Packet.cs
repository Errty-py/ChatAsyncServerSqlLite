using System.Text.Json;

namespace SpaceChatServer.Contracts.Packets;

public class Packet
{
    public PacketType Type { get; set; }
    public JsonElement Data { get; set; }
}