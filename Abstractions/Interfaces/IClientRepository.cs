using TcpChatServer.Data.Entities;

namespace TcpChatServer.Abstractions.Interfaces;

public interface IClientRepository
{
    public Task CreateAsync(ClientEntity client);
    public Task<List<ClientEntity>> GetAllAsync();
    public Task<ClientEntity?> GetByIdAsync(int id);
    public Task<ClientEntity?> GetByLoginAsync(string login);
    public Task<bool> ExistsByLoginAsync(string login);
    public Task UpdateAsync(ClientEntity client);
    public Task DeleteAsync(ClientEntity clientEntity);
}