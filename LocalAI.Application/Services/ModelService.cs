using LocalAI.Application.Abstractions;
using LocalAI.Domain.Entities;

namespace LocalAI.Application.Services;

public sealed class ModelService(
    IModelRepository modelRepository) : IModelService
{
    public Task<IReadOnlyList<LocalModel>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default)
    {
        return modelRepository.GetAllAsync(cancellationToken);
    }

    public async Task<LocalModel?> GetDefaultModelAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await modelRepository.GetAllAsync(
            cancellationToken);

        return models.Count > 0
            ? models[0]
            : null;
    }
}