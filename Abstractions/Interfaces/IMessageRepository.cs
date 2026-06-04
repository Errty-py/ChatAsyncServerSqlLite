using SpaceChatServer.Data.Entities;

namespace SpaceChatServer.Abstractions.Interfaces;

public interface IMessageRepository
{
    public Task AddAsync(MessageEntity message);
    public Task<MessageEntity?> GetByIdAsync(int id);
    public Task<List<MessageEntity>> GetAllAsync();
    public Task DeleteAsync(MessageEntity entity);
}