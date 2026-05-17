using TcpChatServer.Data.Entities;

namespace TcpChatServer.Abstractions.Interfaces;

public interface IMessageRepository
{
    Task AddAsync(MessageEntity message);
    Task<List<MessageEntity>> GetAllAsync();
    public Task RemoveAsync(int id);
}