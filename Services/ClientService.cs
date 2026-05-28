using Microsoft.Extensions.Logging;
using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Contracts.Responses;
using TcpChatServer.Core.Sessions;
using TcpChatServer.Data.Entities;

namespace TcpChatServer.Services;

public class ClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        this._repository = repository;
    }
    
    public async Task<List<ClientResponse>> GetAllAsync()
    {
        List<ClientEntity> clients = await _repository.GetAllAsync();

        List<ClientResponse> responses = 
            clients.Select(client => new ClientResponse
            {
                Id = client.Id,
                Name = client.Name
            })
            .ToList();

        return responses;
    }

    public async Task<ClientResponse> GetByIdAsync(int id)
    {
        ClientEntity? client = await _repository.GetByIdAsync(id);
        
        if (client is null)
            return new ClientResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };

        return new ClientResponse 
        {
            Id = client.Id,
            Name = client.Name
        };
    }

    public async Task<BaseResponse> DeleteAsync(int id)
    {
        ClientEntity? clientEntity = await _repository.GetByIdAsync(id);

        if(clientEntity is null)
            return new BaseResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };

        await _repository.DeleteAsync(clientEntity);

        return new BaseResponse
        {
            Success = true,
            Message = "The client was deleted"
        };
    }
}