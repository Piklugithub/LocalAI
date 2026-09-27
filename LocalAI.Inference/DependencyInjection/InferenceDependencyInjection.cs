using LocalAI.Application.Abstractions;
using LocalAI.Inference.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace LocalAI.Inference.DependencyInjection;

public static class InferenceDependencyInjection
{
    public static IServiceCollection AddLocalAIInference(
        this IServiceCollection services)
    {
        services.AddSingleton<ILocalModelRuntime, LlamaSharpRuntime>();

        services.AddSingleton<
            IInferenceEngine,
            LlamaInferenceEngine>();

        return services;
    }
}