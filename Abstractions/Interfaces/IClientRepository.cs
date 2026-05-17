using TcpChatServer.Data.Entities;

namespace TcpChatServer.Abstractions.Interfaces;

public interface IClientRepository
{
    public Task AddAsync(ClientEntity client);
    public Task<ClientEntity?> GetByIdAsync(int id);
    public Task<ClientEntity?> GetByLoginAsync(string login);
    public Task<bool> ExistsByLoginAsync(string login);
    public Task RemoveAsync(int id);
}