using LocalAI.Application.Models;
using LocalAI.Application.Services;
using LocalAI.Application.Tests.Fakes;
using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;
using LocalAI.Application.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
namespace LocalAI.Application.Tests;

public class ChatServiceTests
{
    [Fact]
    public async Task SendMessageAsync_AddsUserAndAssistantMessages()
    {
        // Arrange
        var inferenceEngine = new FakeInferenceEngine();

        var repository = new FakeConversationRepository();

        var modelService = new FakeModelService();

        var inferenceOptions = Options.Create(
        new InferenceOptions());

        var chatService = new ChatService(
            inferenceEngine,
            repository,
            modelService,
            inferenceOptions,
            NullLogger<ChatService>.Instance);

        var conversation = new Conversation(
            "Test Conversation");

        var request = new ChatRequest
        {
            Conversation = conversation,
            UserMessage = "Hello LocalAI"
        };

        // Act
        var response = await chatService.SendMessageAsync(request);

        // Assert
        Assert.Equal(
            "This is a test response.",
            response.Content);

        Assert.Equal(2, conversation.Messages.Count);

        Assert.Equal(
            ChatRole.User,
            conversation.Messages.ElementAt(0).Role);

        Assert.Equal(
            ChatRole.Assistant,
            conversation.Messages.ElementAt(1).Role);
    }
}