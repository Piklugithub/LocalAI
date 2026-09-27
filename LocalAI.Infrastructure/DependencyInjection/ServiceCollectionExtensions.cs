using LocalAI.Application.Abstractions;
using LocalAI.Infrastructure.Persistence;
using LocalAI.Infrastructure.Providers;
using LocalAI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalAI.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalAIInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<DatabasePathProvider>();

        services.AddSingleton<ModelDirectoryPathProvider>();

        services.AddDbContext<LocalAIDbContext>(
            (serviceProvider, options) =>
            {
                var databasePathProvider =
                    serviceProvider.GetRequiredService<
                        DatabasePathProvider>();

                var databasePath =
                    databasePathProvider.GetDatabasePath();

                options.UseSqlite(
                    $"Data Source={databasePath}");
            });

        services.AddScoped<
            IConversationRepository,
            SqliteConversationRepository>();

        services.AddSingleton<
            IModelRepository,
            FileModelRepository>();

        return services;
    }
}