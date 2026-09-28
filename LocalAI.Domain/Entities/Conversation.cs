using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalAI.Domain.Entities
{
    public class Conversation
    {
        private readonly List<ChatMessage> _messages = [];

        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

        private Conversation()
        {
            Title = string.Empty;
        }

        public Conversation(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Conversation title cannot be empty.",
                    nameof(title));
            }

            Id = Guid.NewGuid();
            Title = title.Trim();

            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public void AddMessage(ChatMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (message.ConversationId != Id)
            {
                throw new InvalidOperationException(
                    "The message does not belong to this conversation.");
            }

            _messages.Add(message);

            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public static Conversation Rehydrate(
        Guid id,
        string title,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        {
            return new Conversation
            {
                Id = id,
                Title = title,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }

        public void GenerateTitleFromFirstMessage(string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            if (Title != "New Conversation")
            {
                return;
            }

            var title = message.Trim();

            const int maxTitleLength = 50;

            if (title.Length > maxTitleLength)
            {
                title = title[..maxTitleLength].TrimEnd() + "...";
            }

            Title = title;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
