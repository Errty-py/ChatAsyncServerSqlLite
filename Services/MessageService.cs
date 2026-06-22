using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Contracts.Requests;
using SpaceChatServer.Contracts.Responses;
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

    public async Task<(BaseResponse, MessageResponse?)> AddAsync(ClientSession session, MessageRequest request)
    {   
        if (session.ClientId != request.FromClientId)
        {
            BaseResponse errorResponse = new BaseResponse()
            {
                Success = false,
                Message = "You cannot send messages on behalf of someone else"
            };
            return (errorResponse, null);
        }

        MessageEntity message = new MessageEntity()
        {
            FromClientId = session.ClientId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(message);

        BaseResponse baseResponse = new BaseResponse()
        {
            Success = true,
            Message = "Message has been added"
        };
        MessageResponse messageResponse = new MessageResponse()
        {
            Id = message.Id,
            FromClientId = message.FromClientId,
            SenderName = session.Name,
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };

        return (baseResponse, messageResponse);
    }

    public async Task<List<MessageResponse>> GetAllAsync()
    {
        List<MessageEntity> messages = await _repository.GetAllAsync();

        List<MessageResponse> responses =
            messages.Select(message => new MessageResponse
            {
                Id = message.Id,
                FromClientId = message.FromClientId,
                SenderName = message.FromClient.Login,
                Text = message.Text,
                CreatedAt = message.CreatedAt
            })
            .ToList();

        return responses;
    }

    public async Task<BaseResponse> DeleteAsync(ClientSession session, Guid messageId)
    {
        MessageEntity? message = await _repository.GetByIdAsync(messageId);

        if(message is null)
            return new BaseResponse
            {
                Success = false,
                Message = "Message not found"
            };

        if(session.ClientId != message.FromClientId)
            return new BaseResponse
            {
                Success = false,
                Message = "You cannot delete other people's messages"
            };

        await _repository.DeleteAsync(message);

        return new BaseResponse
        {
            Success = true,
            Message = "Message deleted"
        };
    }
}