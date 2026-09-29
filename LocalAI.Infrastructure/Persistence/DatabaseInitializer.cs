using Microsoft.EntityFrameworkCore;

namespace LocalAI.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    LocalAIDbContext dbContext)
{
    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(
            cancellationToken);
    }
}