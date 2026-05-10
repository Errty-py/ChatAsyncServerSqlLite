using System.Net.Sockets;

namespace ChatAsyncServerSqlLite.Core.Networking
{
    public class NetworkHelper
    {
        public async Task<byte[]> ReadAsync(NetworkStream stream)
        {
            byte[] lengthBuffer = new byte[4];

            int read = await stream.ReadAsync(lengthBuffer);
            
            if (read == 0) 
                return Array.Empty<byte>();

            int length = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] buffer = new byte[length];

            int offset = 0;

            while (offset < length)
            {
                int chunk = await stream.ReadAsync(buffer.AsMemory(offset, length - offset));
                if (chunk == 0) break;

                offset += chunk;
            }

            return buffer;
        }

        public async Task SendAsync(NetworkStream stream, byte[] data)
        {
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            await stream.WriteAsync(lengthPrefix);
            await stream.WriteAsync(data);
        }
    }
}
