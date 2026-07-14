using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Abstractions.Interfaces;

public interface IClientRepository
{
    public Task CreateAsync(Client client);
    public Task<List<Client>> GetAllAsync();
    public Task<Client?> GetByIdAsync(Guid id);
    public Task<Client?> GetByLoginAsync(string login);
    public Task<bool> ExistsByIdAsync(Guid id);
    public Task<bool> ExistsByLoginAsync(string login);
    public Task<bool> IsLoginOccupiedAsync(string login, Guid id);
    public Task<bool> UpdateAsync(Client client);
    public Task<bool> DeleteAsync(Client clientEntity);
}