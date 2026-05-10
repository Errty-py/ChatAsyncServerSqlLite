using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Contracts.Requests;
using ChatAsyncServerSqlLite.Contracts.Responses;
using ChatAsyncServerSqlLite.Data.Entities;
using ChatAsyncServerSqlLite.Core.Security;

namespace ChatAsyncServerSqlLite.Services;

public class AuthService
{
    private readonly IClientRepository _repository;

    public AuthService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<BaseResponse> RegisterAsync(RegisterRequest request)
    {
        bool exists = await _repository.ExistsByLoginAsync(request.Login);

        if (exists)
        {
            return new BaseResponse
            {
                Success = false,
                Message = "Login already exists"
            };
        }

        ClientEntity client = new()
        {
            Name = request.Name,
            Login = request.Login,
            PasswordHash = PasswordHasher.Hash(request.Password)
        };

        await _repository.AddAsync(client);

        return new BaseResponse
        {
            Success = true,
            Message = "Registered"
        };
    }

    public async Task<ClientResponse> LoginAsync(LoginRequest request)
    {
        ClientEntity? client = await _repository.GetByLoginAsync(request.Login);

        if (client == null)
        {
            return new ClientResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }

        bool verified = PasswordHasher.Verify(
            request.Password,
            client.PasswordHash
        );

        if (!verified)
        {
            return new ClientResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }

        return new ClientResponse
        {
            Success = true,
            Id = client.Id,
            Name = client.Name,
            Message = "Logged in"
        };
    }
}