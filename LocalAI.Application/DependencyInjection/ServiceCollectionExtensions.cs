using LocalAI.Application.Abstractions;
using LocalAI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LocalAI.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalAIApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();

        services.AddSingleton<IModelService, ModelService>();

        services.AddSingleton<
            IInferenceEngine,
            NotConfiguredInferenceEngine>();

        return services;
    }
}