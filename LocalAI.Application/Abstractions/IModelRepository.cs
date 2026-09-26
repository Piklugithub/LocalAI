using LocalAI.Domain.Entities;

namespace LocalAI.Application.Abstractions;

public interface IModelRepository
{
    Task<IReadOnlyList<LocalModel>> GetAllAsync(
        CancellationToken cancellationToken = default);
}