using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Contracts.Responses;
using TcpChatServer.Data.Entities;

namespace TcpChatServer.Services;

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