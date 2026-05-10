using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Contracts.Responses;
using ChatAsyncServerSqlLite.Data.Entities;

namespace ChatAsyncServerSqlLite.Services;

public class ClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        this._clientRepository = clientRepository;
    }

    public async Task<ClientResponse?> GetByIdAsync(int id)
    {
        ClientEntity? client = await _clientRepository.GetByIdAsync(id);
        
        if (client == null)
            return null;

        return new ClientResponse 
        {
            Id = client.Id,
            Name = client.Name
        };
    }
}