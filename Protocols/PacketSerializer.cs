using System.Text.Json;
using System.Text;

namespace ChatAsyncServerSqlLite.Protocols
{
    public static class PacketSerializer
    {
        public static byte[] Serialize<T>(T data)
        {
            string json = JsonSerializer.Serialize(data);

            return Encoding.UTF8.GetBytes(json);
        }

        public static T? Deserialize<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
