using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace TcpChatServer.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _dbContext;

    public ClientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ClientEntity client)
    {
        await _dbContext.Clients.AddAsync(client);
        await _dbContext.SaveChangesAsync();
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

    public async Task RemoveAsync(int id) 
    {
        await _dbContext.Clients.Where(c => c.Id == id).ExecuteDeleteAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}