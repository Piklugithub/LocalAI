using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LocalAI.Domain.Entities;

namespace LocalAI.Application.Models;

public sealed class ChatRequest
{
    public required Conversation Conversation { get; init; }

    public required string UserMessage { get; init; }

    public string? SystemPrompt { get; init; }
}
