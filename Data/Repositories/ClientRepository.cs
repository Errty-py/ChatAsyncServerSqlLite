using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace TcpChatServer.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _dbContext;

    public ClientRepository(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task CreateAsync(ClientEntity client)
    {
        await _dbContext.Clients.AddAsync(client);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ClientEntity>> GetAllAsync()
    {
        return await _dbContext.Clients.AsNoTracking().ToListAsync();
    }

    public async Task<ClientEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.Clients.FindAsync(id);
    }

    public async Task<ClientEntity?> GetByLoginAsync(string login)
    {
        return await _dbContext.Clients.AsNoTracking()
                                       .FirstOrDefaultAsync(c => c.Login == login);
    }

    public async Task<bool> ExistsByLoginAsync(string login)
    {
        return await _dbContext.Clients.AnyAsync(c => c.Login == login);
    }

    public async Task UpdateAsync(ClientEntity client)
    {
        _dbContext.Clients.Update(client);

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(ClientEntity clientEntity) 
    {
        _dbContext.Clients.Remove(clientEntity);

        await _dbContext.SaveChangesAsync();
    }
}