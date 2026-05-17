using System.Text.Json.Serialization;

namespace ChatAsyncServerSqlLite.Contracts.Packets;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PacketType
{
    Registration,
    Login,

    SendMessage,
    GetMessages
}