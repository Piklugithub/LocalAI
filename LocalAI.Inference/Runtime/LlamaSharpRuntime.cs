using LLama;
using LLama.Common;
using LLama.Sampling;
using LLama.Transformers;
using LocalAI.Application.Models;
using LocalAI.Domain.Enums;

namespace LocalAI.Inference.Runtime;

public sealed class LlamaSharpRuntime : ILocalModelRuntime, IAsyncDisposable
{
    private readonly SemaphoreSlim _modelLock = new(1, 1);

    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
    public bool IsLoaded =>
    _weights is not null &&
    _context is not null &&
    _executor is not null;

    public string? LoadedModelPath { get; private set; }

    public async Task LoadModelAsync(
        string modelPath,
        int contextSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelPath);

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException(
                "The specified model file was not found.",
                modelPath);
        }

        if (contextSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contextSize),
                "Context size must be greater than zero.");
        }

        await _modelLock.WaitAsync(cancellationToken);

        try
        {
            // Model is already loaded.
            if (IsLoaded &&
                string.Equals(
                    LoadedModelPath,
                    modelPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await UnloadModelInternalAsync();

            var modelParams = new ModelParams(modelPath)
            {
                ContextSize = (uint)contextSize,

                // CPU inference for now.
                GpuLayerCount = 0
            };

            _weights = await LLamaWeights.LoadFromFileAsync(
                modelParams);

            _context = _weights.CreateContext(
                modelParams);

            _executor = new InteractiveExecutor(
                _context);

            LoadedModelPath = modelPath;

        }
        catch
        {
            await UnloadModelInternalAsync();
            throw;
        }
        finally
        {
            _modelLock.Release();
        }
    }

    public async Task<string> GenerateAsync(
    IReadOnlyList<ChatMessageRequest> messages,
    float temperature,
    int maxTokens,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        if (!IsLoaded || _executor is null)
        {
            throw new InvalidOperationException(
                "No model is currently loaded.");
        }

        if (messages.Count == 0)
        {
            throw new ArgumentException(
                "At least one message is required.",
                nameof(messages));
        }

        if (maxTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxTokens),
                "Maximum tokens must be greater than zero.");
        }

        await _modelLock.WaitAsync(cancellationToken);

        try
        {
            var lastMessage = messages[^1];

            if (lastMessage.Role != ChatRole.User)
            {
                throw new InvalidOperationException(
                    "The last inference message must be a user message.");
            }

            var chatHistory = new ChatHistory();

            foreach (var message in messages
                .Take(messages.Count - 1))
            {
                chatHistory.AddMessage(
                    MapRole(message.Role),
                    message.Content);
            }

            var session = new ChatSession(
                _executor,
                chatHistory);

            session.WithHistoryTransform(
            new PromptTemplateTransformer(
                _weights!,
                withAssistant: true));

            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,

                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = temperature
                }
            };

            var response =
                new System.Text.StringBuilder();

            await foreach (
                var token in session.ChatAsync(
                    new ChatHistory.Message(
                        AuthorRole.User,
                        lastMessage.Content),
                    inferenceParams)
                .WithCancellation(cancellationToken))
            {
                response.Append(token);
            }

            return response
                .ToString()
                .Trim();
        }
        finally
        {
            _modelLock.Release();
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamingAsync(
    IReadOnlyList<ChatMessageRequest> messages,
    float temperature,
    int maxTokens,
    [System.Runtime.CompilerServices.EnumeratorCancellation]
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        if (messages.Count == 0)
        {
            throw new ArgumentException(
                "At least one message is required.",
                nameof(messages));
        }

        if (maxTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxTokens),
                maxTokens,
                "Maximum tokens must be greater than zero.");
        }

        await _modelLock.WaitAsync(
            cancellationToken);

        try
        {
            if (_executor is null || _weights is null)
            {
                throw new InvalidOperationException(
                    "No model is currently loaded.");
            }

            var lastMessage = messages[^1];

            if (lastMessage.Role != ChatRole.User)
            {
                throw new InvalidOperationException(
                    "The last inference message must be a user message.");
            }

            var chatHistory = new ChatHistory();

            foreach (var message in messages.Take(messages.Count - 1))
            {
                chatHistory.AddMessage(
                    MapRole(message.Role),
                    message.Content);
            }

            var session = new ChatSession(
                _executor,
                chatHistory);

            session.WithHistoryTransform(
                new PromptTemplateTransformer(
                    _weights,
                    withAssistant: true));

            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,

                AntiPrompts =
                [
                    "<|im_end|>",
                    "<|im_start|>"
                ],

                SamplingPipeline =
                    new DefaultSamplingPipeline
                    {
                        Temperature = temperature,
                        TopP = 0.9f,
                        TopK = 40,
                        RepeatPenalty = 1.1f,
                        FrequencyPenalty = 0.1f,
                        PresencePenalty = 0.1f
                    }
            };

            await foreach (
                var token in session
                    .ChatAsync(
                        new ChatHistory.Message(
                            AuthorRole.User,
                            lastMessage.Content),
                        inferenceParams,
                        cancellationToken)
                    .WithCancellation(
                        cancellationToken))
            {
                if (!string.IsNullOrEmpty(token))
                {
                    yield return token;
                }
            }
        }
        finally
        {
            _modelLock.Release();
        }
    }

    public async Task UnloadModelAsync(
        CancellationToken cancellationToken = default)
    {
        await _modelLock.WaitAsync(
            cancellationToken);

        try
        {
            await UnloadModelInternalAsync();
        }
        finally
        {
            _modelLock.Release();
        }
    }

    private Task UnloadModelInternalAsync()
    {
        _executor = null;

        _context?.Dispose();
        _context = null;

        _weights?.Dispose();
        _weights = null;

        LoadedModelPath = null;

        return Task.CompletedTask;
    }

    private static AuthorRole MapRole(
        ChatRole role)
    {
        return role switch
        {
            ChatRole.System =>
                AuthorRole.System,

            ChatRole.User =>
                AuthorRole.User,

            ChatRole.Assistant =>
                AuthorRole.Assistant,

            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Unsupported chat role.")
        };
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await UnloadModelAsync();
        }
        finally
        {
            _modelLock.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}