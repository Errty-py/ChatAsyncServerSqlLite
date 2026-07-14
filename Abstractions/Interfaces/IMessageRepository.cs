using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Abstractions.Interfaces;

public interface IMessageRepository
{
    public Task AddAsync(Message message);
    public Task<Message?> GetByIdAsync(Guid id);
    public Task<List<Message>> GetAllAsync();
    public Task<bool> IsMessageOccupiedAsync(Guid id, Guid fromClientId);
    public Task<bool> DeleteAsync(Message entity);
}