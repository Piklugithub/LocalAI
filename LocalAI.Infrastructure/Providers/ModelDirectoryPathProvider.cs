using LocalAI.Application.Configuration;
using Microsoft.Extensions.Options;

namespace LocalAI.Infrastructure.Providers;

public sealed class ModelDirectoryPathProvider(
    IOptions<InferenceOptions> options)
{
    public string GetModelsDirectory()
    {
        var configuredPath = options.Value.ModelsDirectory;

        if (Path.IsPathRooted(configuredPath))
        {
            Directory.CreateDirectory(configuredPath);

            return configuredPath;
        }

        var localApplicationData =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        var applicationDirectory =
            Path.Combine(
                localApplicationData,
                "LocalAI");

        var modelsDirectory =
            Path.Combine(
                applicationDirectory,
                configuredPath);

        Directory.CreateDirectory(modelsDirectory);

        return modelsDirectory;
    }
}