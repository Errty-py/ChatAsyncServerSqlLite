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
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(json);

            byte[] length = BitConverter.GetBytes(data.Length);

            await stream.WriteAsync(length);
            await stream.WriteAsync(data);

            _logger.LogDebug("Sent packet ({Length} bytes)",
                             data.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send packet");

            throw;
        }
    }

    public async Task<string?> ReadAsync(NetworkStream stream)
    {
        try
        {
            byte[] lengthBuffer = new byte[sizeof(int)];

            await stream.ReadExactlyAsync(lengthBuffer);

            int length = BitConverter.ToInt32(lengthBuffer);

            if (length <= 0 || length > MaxPacketSize)
            {
                _logger.LogWarning("Invalid packet size: {Length}",
                                   length);

                return null;
            }

            byte[] data = new byte[length];

            await stream.ReadExactlyAsync(data);

            _logger.LogDebug("Received packet ({Length} bytes)",
                             length);

            return Encoding.UTF8.GetString(data);
        }
        catch (EndOfStreamException)
        {
            _logger.LogInformation("Client disconnected");

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "Failed to read packet");

            return null;
        }
    }
}