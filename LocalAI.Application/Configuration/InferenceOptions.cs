namespace LocalAI.Application.Configuration;

public sealed class InferenceOptions
{
    public const string SectionName = "Inference";

    public string ModelsDirectory { get; set; } = "models";

    public string DefaultModel { get; set; } = string.Empty;

    public int ContextSize { get; set; } = 8192;

    public float Temperature { get; set; } = 0.7f;

    public int MaxTokens { get; set; } = 1024;

    public int GpuLayers { get; set; }

    public int Threads { get; set; } = 0;

    public bool FlashAttention { get; set; }
}