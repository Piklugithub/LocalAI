using LocalAI.Application.Abstractions;
using LocalAI.Application.Models;
using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;

namespace LocalAI.Application.Services;

public sealed class ChatService(
    IInferenceEngine inferenceEngine,
    IConversationRepository conversationRepository) : IChatService
{
    public async Task<ChatResponse> SendMessageAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            throw new ArgumentException(
                "User message cannot be empty.",
                nameof(request));
        }

        var userMessage = new ChatMessage(
            request.Conversation.Id,
            ChatRole.User,
            request.UserMessage);

        request.Conversation.AddMessage(userMessage);

        var messages = request.Conversation.Messages
            .Select(message => new ChatMessageRequest
            {
                Role = message.Role,
                Content = message.Content
            })
            .ToList();

        var inferenceRequest = new InferenceRequest
        {
            ModelId = string.Empty,
            ModelPath = string.Empty,
            Messages = messages
        };

        var response = await inferenceEngine.GenerateAsync(
            inferenceRequest,
            cancellationToken);

        var assistantMessage = new ChatMessage(
            request.Conversation.Id,
            ChatRole.Assistant,
            response.Content);

        request.Conversation.AddMessage(assistantMessage);

        await conversationRepository.SaveAsync(
            request.Conversation,
            cancellationToken);

        return new ChatResponse
        {
            Content = response.Content,
            Duration = response.Duration,
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens
        };
    }
}