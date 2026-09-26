using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;

namespace LocalAI.Domain.Tests;

public class ConversationTests
{
    [Fact]
    public void AddMessage_WithValidMessage_AddsMessage()
    {
        // Arrange
        var conversation = new Conversation("Kafka Discussion");

        var message = new ChatMessage(
            conversation.Id,
            ChatRole.User,
            "Explain Kafka.");

        // Act
        conversation.AddMessage(message);

        // Assert
        Assert.Single(conversation.Messages);
        Assert.Equal("Explain Kafka.", conversation.Messages.First().Content);
    }

    [Fact]
    public void AddMessage_WithMessageFromAnotherConversation_ThrowsException()
    {
        // Arrange
        var conversation = new Conversation("Conversation 1");

        var anotherConversation = new Conversation("Conversation 2");

        var message = new ChatMessage(
            anotherConversation.Id,
            ChatRole.User,
            "Hello");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            conversation.AddMessage(message));
    }
}