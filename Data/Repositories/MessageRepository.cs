using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SpaceChatServer.Core.Models;

namespace SpaceChatServer.Data.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _dbContext;

    public MessageRepository(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task AddAsync(Message message)
    {
        MessageEntity messageEntity = new MessageEntity()
        {
            Id = message.Id,
            FromClientId = message.FromClientId,
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };

        await _dbContext.Messages.AddAsync(messageEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Message>> GetAllAsync()
    {
        var messageEntities = await _dbContext.Messages.AsNoTracking()
                                                                       .ToListAsync();

        return messageEntities.Select(m => Message.Create(m.Id,
                                                          m.FromClientId,
                                                          m.Text,
                                                          m.CreatedAt).Value).ToList();
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        MessageEntity? messageEntity = await _dbContext.Messages.FindAsync(id);

        if (messageEntity is null)
            return null;

        return Message.Create(messageEntity.Id,
                              messageEntity.FromClientId,
                              messageEntity.Text,
                              messageEntity.CreatedAt).Value;
    }

    public async Task<bool> IsMessageOccupiedAsync(Guid id, Guid fromClientId)
    {
        return await _dbContext.Messages.AnyAsync(m => m.Id == id && m.FromClientId != fromClientId);
    }

    public async Task<bool> DeleteAsync(Message message)
    {
        int deletedCount = await _dbContext.Messages
                                           .Where(m => m.Id == message.Id)
                                           .ExecuteDeleteAsync();

        return deletedCount > 0;
    }
}