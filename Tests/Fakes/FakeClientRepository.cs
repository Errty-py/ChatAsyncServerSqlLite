using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Tests.Fakes;

public class FakeClientRepository : IClientRepository
{
    private readonly Dictionary<Guid, Client> _clients = new();

    public int CreateCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public void Seed(Client client) => _clients[client.Id] = client;

    public Task CreateAsync(Client client)
    {
        CreateCalls++;
        _clients[client.Id] = client;
        return Task.CompletedTask;
    }

    public Task<List<Client>> GetAllAsync() => Task.FromResult(_clients.Values.ToList());

    public Task<Client?> GetByIdAsync(Guid id)
        => Task.FromResult(_clients.TryGetValue(id, out var client) ? client : null);

    public Task<Client?> GetByLoginAsync(string login)
        => Task.FromResult(_clients.Values.FirstOrDefault(c => c.Login == login));

    public Task<bool> ExistsByIdAsync(Guid id) => Task.FromResult(_clients.ContainsKey(id));

    public Task<bool> ExistsByLoginAsync(string login)
        => Task.FromResult(_clients.Values.Any(c => c.Login == login));

    public Task<bool> IsLoginOccupiedAsync(string login, Guid id)
        => Task.FromResult(_clients.Values.Any(c => c.Login == login && c.Id != id));

    public Task UpdateAsync(Client client)
    {
        UpdateCalls++;
        _clients[client.Id] = client;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Client client)
    {
        DeleteCalls++;
        _clients.Remove(client.Id);
        return Task.CompletedTask;
    }
}
