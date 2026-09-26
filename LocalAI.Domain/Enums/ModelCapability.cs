using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalAI.Domain.Enums;

[Flags]
public enum ModelCapability
{
    None = 0,

    Chat = 1,
    Completion = 2,
    CodeGeneration = 4,
    ToolCalling = 8,
    Embeddings = 16,
    Vision = 32
}
