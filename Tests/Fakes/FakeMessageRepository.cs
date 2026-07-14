using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Tests.Fakes;

public class FakeMessageRepository : IMessageRepository
{
    private readonly Dictionary<Guid, Message> _messages = new();

    public int AddCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public void Seed(Message message) => _messages[message.Id] = message;

    public Task AddAsync(Message message)
    {
        AddCalls++;
        _messages[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<Message?> GetByIdAsync(Guid id)
        => Task.FromResult(_messages.TryGetValue(id, out var message) ? message : null);

    public Task<List<Message>> GetAllAsync() => Task.FromResult(_messages.Values.ToList());

    public Task<bool> IsMessageOccupiedAsync(Guid id, Guid fromClientId)
        => Task.FromResult(_messages.Values.Any(m => m.Id == id && m.FromClientId == fromClientId));

    public Task DeleteAsync(Message entity)
    {
        DeleteCalls++;
        _messages.Remove(entity.Id);
        return Task.CompletedTask;
    }
}
