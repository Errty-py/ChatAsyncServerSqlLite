using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;

namespace SpaceChatServer.Core.Networking;

public class NetworkHelper
{
    private const int MaxPacketSize = 1024 * 1024;

    private readonly ILogger<NetworkHelper> _logger;

    public NetworkHelper(ILogger<NetworkHelper> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(NetworkStream stream,
                                 string json)
    {
        byte[] data = Encoding.UTF8.GetBytes(json);

        byte[] length = BitConverter.GetBytes(data.Length);

        await stream.WriteAsync(length);
        await stream.WriteAsync(data);

        _logger.LogDebug("Sent packet ({Length} bytes)",
                         data.Length);
    }

    public async Task<string?> ReadAsync(NetworkStream stream)
    {
        byte[] lengthBuffer = new byte[sizeof(int)];

        if (!await TryReadExactlyAsync(stream, lengthBuffer))
        {
            _logger.LogInformation("Client disconnected");

            return null;
        }

        int length = BitConverter.ToInt32(lengthBuffer);

        if (length <= 0 || length > MaxPacketSize)
        {
            throw new InvalidDataException($"Invalid packet size: {length}");
        }

        byte[] data = new byte[length];

        await stream.ReadExactlyAsync(data);

        _logger.LogDebug("Received packet ({Length} bytes)",
                         length);

        return Encoding.UTF8.GetString(data);
    }

    private static async Task<bool> TryReadExactlyAsync(NetworkStream stream,
                                                        Memory<byte> buffer)
    {
        int bytesRead = 0;

        while (bytesRead < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer[bytesRead..]);

            if (read == 0)
            {
                if (bytesRead == 0)
                    return false;

                throw new EndOfStreamException("Connection closed while reading packet length");
            }

            bytesRead += read;
        }

        return true;
    }
}