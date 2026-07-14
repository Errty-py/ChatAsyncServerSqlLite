using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Core.Models;
using SpaceChatServer.Core.Security;

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

    public async Task<(Client? client, string? error)> UpdateAsync(Guid id, string name, string login, byte[]? avatar)
    {
        var client = await _repository.GetByIdAsync(id);

        if (client is null)
            return (null, "Client not found");

        bool isLoginOccupied = await _repository.IsLoginOccupiedAsync(login, id);

        if (isLoginOccupied)
            return (null, "Login is already occupied by another client");

        var updateResult = client.Update(name,
                                         login,
                                         avatar);

        if (updateResult.IsFailure)
            return (null, updateResult.Error);

        await _repository.UpdateAsync(client);

        return (client, null);
    }

    public async Task<(Client? client, string? error)> ChangePassword(Guid id, string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < AuthService.MinPasswordLength)
            return (null, $"Password must be at least {AuthService.MinPasswordLength} characters long.");

        var client = await _repository.GetByIdAsync(id);

        if (client is null)
            return (null, "Client not found");

        var passwordHash = PasswordHasher.Hash(password);
        var changePasswordResult = client.ChangePassword(passwordHash);

        if (changePasswordResult.IsFailure)
            return (null, changePasswordResult.Error);

        await _repository.UpdateAsync(client);

        return (client, null);
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