using System.Text.Json;

namespace ChatAsyncServerSqlLite.Contracts;

public class Packet
{
    public string Type { get; set; } = string.Empty;
    public JsonElement Data { get; set; }
}