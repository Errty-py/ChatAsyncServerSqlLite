using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace SpaceChatServer.Data.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _dbContext;

    public MessageRepository(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task AddAsync(MessageEntity message)
    {
        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<MessageEntity>> GetAllAsync()
    {
        return await _dbContext.Messages.Include(x => x.FromClient.Name)
                                        .AsNoTracking()
                                        .ToListAsync();
    }

    public async Task<MessageEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.Messages.FindAsync(id);
    }

    public async Task DeleteAsync(MessageEntity entity) 
    {
        _dbContext.Messages.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}