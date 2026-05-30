using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Contracts.Requests;
using TcpChatServer.Contracts.Responses;
using TcpChatServer.Data.Entities;
using TcpChatServer.Core.Security;

namespace TcpChatServer.Services;

public class AuthService
{
    private readonly IClientRepository _repository;

    public AuthService(IClientRepository repository)
    {
        this._repository = repository;
    }

    public async Task<(BaseResponse, ClientResponse?)> RegisterAsync(RegisterRequest request)
    {
        bool exists = await _repository.ExistsByLoginAsync(request.Login);

        if (exists)
        {
            return (new BaseResponse
            {
                Success = false,
                Message = "Login already exists"
            }, null);
        }

        ClientEntity client = new ClientEntity()
        {
            Name = request.Name,
            Login = request.Login,
            PasswordHash = PasswordHasher.Hash(request.Password)
        };

        await _repository.CreateAsync(client);

        BaseResponse baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Registered"
        };
        ClientResponse clientResponse = new ClientResponse
        {
            Id = client.Id,
            Name = client.Name
        };

        return (baseResponse, clientResponse);
    }

    public async Task<(BaseResponse, ClientResponse?)> LoginAsync(LoginRequest request)
    {
        ClientEntity? client = await _repository.GetByLoginAsync(request.Login);

        if (client == null)
        {
            return (new BaseResponse
            {
                Success = false,
                Message = "Invalid credentials"
            }, null);
        }

        bool verified = PasswordHasher.Verify(
            request.Password,
            client.PasswordHash
        );

        if (!verified)
        {
            return (new BaseResponse
            {
                Success = false,
                Message = "Invalid credentials"
            }, null);
        }

        BaseResponse baseResponse = new BaseResponse
        {
            Success = true,
            Message = "Logged in"
        };
        ClientResponse clientResponse = new ClientResponse
        {
            Id = client.Id,
            Name = client.Name
        };

        return (baseResponse, clientResponse);
    }
}