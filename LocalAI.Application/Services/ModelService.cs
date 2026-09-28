using LocalAI.Application.Abstractions;
using LocalAI.Domain.Entities;

namespace LocalAI.Application.Services;

public sealed class ModelService(
    IModelRepository modelRepository) : IModelService
{
    private LocalModel? _selectedModel;

    public Task<IReadOnlyList<LocalModel>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default)
    {
        return modelRepository.GetAllAsync(
            cancellationToken);
    }

    public async Task<LocalModel?> GetDefaultModelAsync(
    CancellationToken cancellationToken = default)
    {
        if (_selectedModel is not null)
        {
            return _selectedModel;
        }

        var models = await modelRepository.GetAllAsync(
            cancellationToken);

        if (models.Count == 0)
        {
            return null;
        }

        _selectedModel = models[0];

        return _selectedModel;
    }

    public LocalModel? GetSelectedModel()
    {
        return _selectedModel;
    }

    public void SelectModel(LocalModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        _selectedModel = model;
    }
}