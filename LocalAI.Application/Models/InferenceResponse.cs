using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalAI.Application.Models
{
    public sealed class InferenceResponse
    {
        public required string Content { get; init; }

        public int? InputTokens { get; init; }

        public int? OutputTokens { get; init; }

        public TimeSpan Duration { get; init; }
    }
}
