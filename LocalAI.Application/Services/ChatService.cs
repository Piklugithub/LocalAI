using LocalAI.Application.Abstractions;
using LocalAI.Application.Configuration;
using LocalAI.Application.Models;
using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace LocalAI.Application.Services;

public sealed class ChatService(
    IInferenceEngine inferenceEngine,
    IConversationRepository conversationRepository,
    IModelService modelService,
    IOptions<InferenceOptions> inferenceOptions,
    ILogger<ChatService> logger) : IChatService
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

        var model = modelService.GetSelectedModel();

        if (model is null)
        {
            model = await modelService.GetDefaultModelAsync(
                cancellationToken);
        }

        if (model is null)
        {
            throw new InvalidOperationException(
                "No local model is available.");
        }

        var options = inferenceOptions.Value;

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

        logger.LogDebug(
        "Building inference context for conversation {ConversationId}. " +
        "Message count: {MessageCount}.",
        request.Conversation.Id,
        messages.Count);

        foreach (var message in messages)
        {
            logger.LogDebug(
                "Inference message: {Role} - {Content}",
                message.Role,
                message.Content);
        }

        var inferenceRequest = new InferenceRequest
        {
            ModelId = model.Id.ToString(),
            ModelPath = model.FilePath,
            Messages = messages,
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            ContextSize = options.ContextSize
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

    public async IAsyncEnumerable<string> StreamMessageAsync(
    ChatRequest request,
    [System.Runtime.CompilerServices.EnumeratorCancellation]
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            throw new ArgumentException(
                "User message cannot be empty.",
                nameof(request));
        }

        var model = modelService.GetSelectedModel();

        if (model is null)
        {
            model = await modelService.GetDefaultModelAsync(
                cancellationToken);
        }

        if (model is null)
        {
            throw new InvalidOperationException(
                "No local model is available.");
        }

        var options = inferenceOptions.Value;

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
            ModelId = model.Id.ToString(),
            ModelPath = model.FilePath,
            Messages = messages,
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            ContextSize = options.ContextSize
        };

        var responseBuilder =
            new System.Text.StringBuilder();

        await foreach (
            var token in inferenceEngine
                .GenerateStreamingAsync(
                    inferenceRequest,
                    cancellationToken)
                .WithCancellation(cancellationToken))
        {
            responseBuilder.Append(token);

            yield return token;
        }

        var assistantContent =
            responseBuilder
                .ToString()
                .Trim();

        if (!string.IsNullOrWhiteSpace(assistantContent))
        {
            var assistantMessage = new ChatMessage(
                request.Conversation.Id,
                ChatRole.Assistant,
                assistantContent);

            request.Conversation.AddMessage(
                assistantMessage);

            await conversationRepository.SaveAsync(
                request.Conversation,
                cancellationToken);
        }
    }
}