using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Contracts.Requests;
using SpaceChatServer.Contracts.Responses;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Data.Entities;

namespace SpaceChatServer.Services;

public class ClientService
{
    private readonly IClientRepository _repository;
    private readonly SessionManager _sessionManager;

    public ClientService(IClientRepository repository,
                         SessionManager sessionManager)
    {
        this._repository = repository;
        this._sessionManager = sessionManager;
    }
    
    public async Task<List<ClientResponse>> GetAllAsync()
    {
        List<ClientEntity> clients = await _repository.GetAllAsync();

        List<ClientResponse> responses = 
            clients.Select(client => new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                Avatar = client.Avatar,
                IsOnline = _sessionManager.IsOnline(client.Id)
            })
            .ToList();

        return responses;
    }

    public async Task<(BaseResponse, ClientResponse?)> GetByIdAsync(Guid id)
    {
        ClientEntity? client = await _repository.GetByIdAsync(id);
        
        if (client is null)
        {
            BaseResponse errorResponse = new BaseResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };

            return (errorResponse, null);
        }

        BaseResponse baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Receiving customer data was successful"
        };
        ClientResponse clientResponse = new ClientResponse 
        {
            Id = client.Id,
            Name = client.Name,
            Avatar = client.Avatar,
            IsOnline = _sessionManager.IsOnline(client.Id)
        };


        return (baseResponse, clientResponse);
    }

    public async Task<(BaseResponse, ClientProfileResponse?)> UpdateAsync(Guid id, UpdateClientRequest request)
    {
        ClientEntity? client = await _repository.GetByIdAsync(id);

        if (client is null)
        {
            return (new BaseResponse
            {
                Success = false,
                Message = "Client not found"
            }, null);
        }

        client.Name = request.Name;
        client.Login = request.Login;

        if (request.Avatar is not null)
        {
            if (request.Avatar.Length > 1024 * 1024 * 8)
            {
                return (new BaseResponse
                {
                    Success = false,
                    Message = "Avatar too large (max 8MB)"
                }, null);
            }

            client.Avatar = request.Avatar;
        }

        await _repository.UpdateAsync(client);

        BaseResponse baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Client updated"
        };
        ClientProfileResponse clientProfileResponse = new ClientProfileResponse
        {
            Id = client.Id,
            Name = client.Name,
            Login = client.Login,
            Avatar = client.Avatar
        };

        return (baseResponse, clientProfileResponse);
    }

    public async Task<(BaseResponse, Guid?)> DeleteAsync(Guid id)
    {
        ClientEntity? clientEntity = await _repository.GetByIdAsync(id);

        if(clientEntity is null)
        {
            BaseResponse errorResponse = new BaseResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };

            return (errorResponse, null);
        }

        await _repository.DeleteAsync(clientEntity);

        BaseResponse baseResponse = new BaseResponse
        {
            Success = true,
            Message = "The client was deleted",
        };

        return (baseResponse, id);
    }
}