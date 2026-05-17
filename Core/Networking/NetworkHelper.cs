using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
namespace ChatAsyncServerSqlLite.Core.Networking;

public class NetworkHelper
{
    private readonly ILogger<NetworkHelper> _logger;

    public NetworkHelper(ILogger<NetworkHelper> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(
        NetworkStream stream,
        string json)
    {
        byte[] data = Encoding.UTF8.GetBytes(json);

        byte[] length = BitConverter.GetBytes(data.Length);

        await stream.WriteAsync(length);

        await stream.WriteAsync(data);
    }

    public async Task<string?> ReadAsync(
        NetworkStream stream)
    {
        byte[] lengthBuffer = new byte[4];

        int read = await stream.ReadAsync(lengthBuffer);

        if (read == 0)
            return null;

        int length = BitConverter.ToInt32(lengthBuffer);

        byte[] data = new byte[length];

        int totalRead = 0;

        while (totalRead < length)
        {
            int currentRead =
                await stream.ReadAsync(
                    data.AsMemory(
                        totalRead,
                        length - totalRead));

            if (currentRead == 0)
                return null;

            totalRead += currentRead;
        }

        return Encoding.UTF8.GetString(data);
    }
}