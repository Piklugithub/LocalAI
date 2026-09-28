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
    private ChatSession? _session;

    public bool IsLoaded =>
        _weights is not null &&
        _context is not null &&
        _executor is not null &&
        _session is not null;

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

            // Create ONE persistent chat session.
            var chatHistory = new ChatHistory();

            _session = new ChatSession(
                _executor,
                chatHistory);

            // Use the model's prompt template.
            _session.WithHistoryTransform(
                new PromptTemplateTransformer(
                    _weights,
                    withAssistant: true));
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

        if (messages.Count == 0)
        {
            throw new ArgumentException(
                "At least one message is required.",
                nameof(messages));
        }

        var response = new System.Text.StringBuilder();

        await foreach (var token in GenerateStreamingAsync(
            messages,
            temperature,
            maxTokens,
            cancellationToken))
        {
            response.Append(token);
        }

        return response
            .ToString()
            .Trim();
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
            if (_session is null)
            {
                throw new InvalidOperationException(
                    "No model session is currently loaded.");
            }

            var lastUserMessage = messages
                .LastOrDefault(x =>
                    x.Role == ChatRole.User);

            if (lastUserMessage is null)
            {
                throw new ArgumentException(
                    "The conversation must contain a user message.",
                    nameof(messages));
            }

            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,

                AntiPrompts =
                [
                    "User:"
                ],

                SamplingPipeline =
                    new DefaultSamplingPipeline
                    {
                        Temperature = temperature
                    }
            };

            await foreach (
                var token in _session
                    .ChatAsync(
                        new ChatHistory.Message(
                            AuthorRole.User,
                            lastUserMessage.Content),
                        inferenceParams,
                        cancellationToken)
                    .WithCancellation(
                        cancellationToken))
            {
                var cleanedToken = token;

                /*
                 * LLamaSharp may include the anti-prompt
                 * in the generated stream.
                 *
                 * We don't want "User:" appearing in the UI.
                 */
                const string stopMarker = "User:";

                if (cleanedToken.Contains(
                        stopMarker,
                        StringComparison.OrdinalIgnoreCase))
                {
                    var index = cleanedToken.IndexOf(
                        stopMarker,
                        StringComparison.OrdinalIgnoreCase);

                    if (index >= 0)
                    {
                        cleanedToken =
                            cleanedToken[..index];
                    }
                }

                if (!string.IsNullOrEmpty(cleanedToken))
                {
                    yield return cleanedToken;
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
        _session = null;

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