using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Services;

public class ClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        this._repository = repository;
    }
    
    public async Task<List<Client>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<(Client? client, string? error)> GetByIdAsync(Guid id)
    {
        Client? client = await _repository.GetByIdAsync(id);
        
        if (client is null)
            return (null, "Client not found");

        return (client, null);
    }

    public async Task<(Client? client, string? error)> UpdateAsync(Guid id, string name, string login, string password, byte[]? avatar)
    {
        var client = Client.Create(id, name, login, password, avatar);

        if (client.IsFailure)
            return (null, client.Error);

        bool clientExists = await _repository.ExistsByIdAsync(client.Value.Id);

        if (!clientExists)
            return (null, "Client not found");

        bool isLoginOccupied = await _repository.IsLoginOccupiedAsync(client.Value.Login, client.Value.Id);

        if (isLoginOccupied)
            return (null, "Login is already occupied by another client");


        await _repository.UpdateAsync(client.Value);

        return (client.Value, null);
    }

    public async Task<(Guid? id, string? error)> DeleteAsync(Guid id)
    {
        Client? client = await _repository.GetByIdAsync(id);
    
        if(client is null)
            return (null, "Client not found");

        await _repository.DeleteAsync(client);

        return (id, null);
    }
}