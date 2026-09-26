using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;

namespace LocalAI.Domain.Tests;

public class ChatMessageTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesMessage()
    {
        // Arrange
        var conversationId = Guid.NewGuid();

        // Act
        var message = new ChatMessage(
            conversationId,
            ChatRole.User,
            "Hello LocalAI");

        // Assert
        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(conversationId, message.ConversationId);
        Assert.Equal(ChatRole.User, message.Role);
        Assert.Equal("Hello LocalAI", message.Content);
    }

    [Fact]
    public void Constructor_WithEmptyContent_ThrowsException()
    {
        // Arrange
        var conversationId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ChatMessage(
                conversationId,
                ChatRole.User,
                ""));
    }
}