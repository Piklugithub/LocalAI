using LocalAI.Domain.Entities;

namespace LocalAI.Application.Abstractions;

public interface IModelService
{
    Task<IReadOnlyList<LocalModel>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default);

    Task<LocalModel?> GetDefaultModelAsync(
        CancellationToken cancellationToken = default);

    LocalModel? GetSelectedModel();

    void SelectModel(LocalModel model);

    Task LoadSelectedModelAsync(
    CancellationToken cancellationToken = default);
}