using LocalAI.Application.Abstractions;
using LocalAI.Application.Configuration;
using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LocalAI.Infrastructure.Repositories;

public sealed class FileModelRepository(
    IOptions<InferenceOptions> options,
    ILogger<FileModelRepository> logger) : IModelRepository
{
    public Task<IReadOnlyList<LocalModel>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var modelsDirectory = options.Value.ModelsDirectory;

        var directoryPath = Path.IsPathRooted(modelsDirectory)
            ? modelsDirectory
            : Path.Combine(
                AppContext.BaseDirectory,
                modelsDirectory);

        logger.LogDebug(
            "Searching for local models in {ModelsDirectory}.",
            directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            logger.LogWarning(
                "Models directory does not exist: {ModelsDirectory}.",
                directoryPath);

            return Task.FromResult<IReadOnlyList<LocalModel>>([]);
        }

        var modelFiles = Directory
            .EnumerateFiles(
                directoryPath,
                "*.gguf",
                SearchOption.TopDirectoryOnly)
            .ToList();

        logger.LogInformation(
            "Found {ModelCount} local model(s).",
            modelFiles.Count);

        var models = modelFiles
            .Select(filePath =>
            {
                var fileInfo = new FileInfo(filePath);

                return new LocalModel(
                    name: Path.GetFileNameWithoutExtension(filePath),
                    filePath: filePath,
                    format: ModelFormat.Gguf,
                    capabilities: ModelCapability.Chat,
                    sizeInBytes: fileInfo.Length);
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<LocalModel>>(models);
    }
}