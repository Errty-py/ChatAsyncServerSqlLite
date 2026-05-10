using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace ChatAsyncServerSqlLite.Core.Networking;

public class NetworkHelper
{
    private readonly ILogger<NetworkHelper> _logger;

    public NetworkHelper(ILogger<NetworkHelper> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ReadAsync(NetworkStream stream)
    {
        byte[] lengthBuffer = new byte[4];

        int totalRead = 0;

        while (totalRead < 4)
        {
            int read = await stream.ReadAsync(
                lengthBuffer.AsMemory(totalRead, 4 - totalRead));

            if (read == 0)
            {
                _logger.LogWarning("Stream closed while reading length prefix");
                return Array.Empty<byte>();
            }

            totalRead += read;
        }

        int length = BitConverter.ToInt32(lengthBuffer, 0);

        if (length <= 0 || length > 10_000_000)
        {
            _logger.LogWarning(
                "Invalid packet length: {Length}",
                length);

            return Array.Empty<byte>();
        }

        byte[] buffer = new byte[length];
        int offset = 0;

        while (offset < length)
        {
            int chunk = await stream.ReadAsync(
                buffer.AsMemory(offset, length - offset));

            if (chunk == 0)
            {
                _logger.LogWarning(
                    "Stream closed while reading payload. Expected {Length}, got {Offset}",
                    length,
                    offset);

                return Array.Empty<byte>();
            }

            offset += chunk;
        }

        _logger.LogDebug(
            "Received packet {Length} bytes",
            length);

        return buffer;
    }

    public async Task SendAsync(byte[] data, NetworkStream stream)
    {
        try
        {
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            await stream.WriteAsync(lengthPrefix);
            await stream.WriteAsync(data);

            _logger.LogDebug(
                "Sent packet {Length} bytes",
                data.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send packet");
        }
    }
}