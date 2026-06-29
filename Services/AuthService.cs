using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Core.Security;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Services;

public class AuthService
{
    private readonly IClientRepository _repository;

    public AuthService(IClientRepository repository)
    {
        this._repository = repository;
    }

    public async Task<(Client? client, string? error)> RegisterAsync(string name, string login, string password)
    {
        string passwordHash = PasswordHasher.Hash(password);
        
        var client = Client.Create(Guid.NewGuid(), name, login, passwordHash, null);

        if (client.IsFailure)
        {
            return (null, client.Error);
        }
        
        bool exists = await _repository.ExistsByLoginAsync(client.Value.Login);

        if (exists)
        {
            return (null, "Client with this login already exists");
        }

        await _repository.CreateAsync(client.Value);

        return (client.Value, null);
    }

    public async Task<(Client? client, string? error)> LoginAsync(string login, string password)
    {
        Client? client = await _repository.GetByLoginAsync(login);

        if (client == null)
        {
            return (null, "Client not found");
        }

        bool verified = PasswordHasher.Verify(
            password,
            client.PasswordHash
        );

        if (!verified)
        {
            return (null, "Invalid password");
        }

        return (client, null);
    }
}