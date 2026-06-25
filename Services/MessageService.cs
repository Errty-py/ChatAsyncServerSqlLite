using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Contracts.Requests;
using SpaceChatServer.Contracts.Responses;
using SpaceChatServer.Core.Models;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Data.Entities;

namespace SpaceChatServer.Services;

public class MessageService
{
    private readonly IMessageRepository _repository;
    
    public MessageService(IMessageRepository repository)
    {
        this._repository = repository;
    }

    public async Task<(Message? message, string? error)> AddAsync(Guid fromClientId, string text)
    {   
        var message = Message.Create(Guid.NewGuid(),
                                     fromClientId,
                                     text,
                                     DateTime.UtcNow); 

        if (message.IsFailure)
            return (null, message.Error);

        await _repository.AddAsync(message.Value);

        return (message.Value, null);
    }

    public async Task<List<Message>> GetAllAsync()
    {
        List<Message> messages = await _repository.GetAllAsync();

        return messages;
    }

    public async Task<(Guid id, string? error)> DeleteAsync(Guid fromClientId, Guid messageId)
    {
        Message? message = await _repository.GetByIdAsync(messageId);

        if(message is null)
            return (Guid.Empty, "Message not found");

        if(fromClientId != message.FromClientId)
            return (Guid.Empty, "You cannot delete other people's messages");

        await _repository.DeleteAsync(message);

        return (message.Id, null);
    }
}