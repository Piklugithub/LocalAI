using LocalAI.Application.Abstractions;
using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;

namespace LocalAI.Application.Tests.Fakes;

public sealed class FakeModelService : IModelService
{
    private readonly LocalModel _model = new(
        "Test Model",
        "test-model.gguf",
        ModelFormat.Gguf,
        ModelCapability.Chat,
        1024);

    public Task<IReadOnlyList<LocalModel>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LocalModel> models = [_model];

        return Task.FromResult(models);
    }

    public Task<LocalModel?> GetDefaultModelAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<LocalModel?>(_model);
    }
}