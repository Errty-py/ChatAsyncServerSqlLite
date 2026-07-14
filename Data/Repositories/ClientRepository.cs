using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _dbContext;

    public ClientRepository(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task CreateAsync(Client client)
    {
        var clientEntity = new ClientEntity
        {
            Id = client.Id,
            Name = client.Name,
            Login = client.Login,
            PasswordHash = client.PasswordHash
        };

        await _dbContext.Clients.AddAsync(clientEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Client>> GetAllAsync()
    {
        var clientEntities = await _dbContext.Clients.AsNoTracking()
                                                     .ToListAsync();

        var clients = clientEntities.Select(c => Client.Create(c.Id,
                                                               c.Name,
                                                               c.Login,
                                                               c.PasswordHash,
                                                               c.Avatar).Value)
                                    .ToList();

        return clients;
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        var clientEntity = await _dbContext.Clients.FindAsync(id);

        if(clientEntity is null)
            return null;

        var client = Client.Create(clientEntity.Id,
                                   clientEntity.Name,
                                   clientEntity.Login,
                                   clientEntity.PasswordHash,
                                   clientEntity.Avatar)
                           .Value;

        return client;
    }

    public async Task<Client?> GetByLoginAsync(string login)
    {
        var clientEntity = await _dbContext.Clients.AsNoTracking()
                                                   .FirstOrDefaultAsync(c => c.Login == login);

        if(clientEntity is null)
            return null;

        var client = Client.Create(clientEntity.Id,
                                   clientEntity.Name,
                                   clientEntity.Login,
                                   clientEntity.PasswordHash,
                                   clientEntity.Avatar)
                           .Value;

        return client;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _dbContext.Clients.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByLoginAsync(string login)
    {
        return await _dbContext.Clients.AnyAsync(c => c.Login == login);
    }

    public async Task<bool> IsLoginOccupiedAsync(string login, Guid id)
    {
        return await _dbContext.Clients.AnyAsync(c => c.Login == login && c.Id != id);
    }

    public async Task<bool> UpdateAsync(Client client)
    {
        int updatedCount = await _dbContext.Clients
                                           .Where(c => c.Id == client.Id)
                                           .ExecuteUpdateAsync(setters => setters
                                               .SetProperty(c => c.Name, client.Name)
                                               .SetProperty(c => c.Login, client.Login)
                                               .SetProperty(c => c.PasswordHash, client.PasswordHash)
                                               .SetProperty(c => c.Avatar, client.Avatar));

        return updatedCount > 0;
    }

    public async Task<bool> DeleteAsync(Client client)
    {
        int deletedCount = await _dbContext.Clients
                                           .Where(c => c.Id == client.Id)
                                           .ExecuteDeleteAsync();

        return deletedCount > 0;
    }
}