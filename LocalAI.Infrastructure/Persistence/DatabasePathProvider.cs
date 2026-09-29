using LocalAI.Application.Configuration;
using Microsoft.Extensions.Options;

namespace LocalAI.Infrastructure.Persistence;

public sealed class DatabasePathProvider(
    IOptions<ApplicationOptions> options)
{
    public string GetDatabasePath()
    {
        var dataDirectory = options.Value.DataDirectory;

        var localAppData =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        var applicationDirectory =
            Path.Combine(localAppData, "LocalAI");

        var fullDataDirectory =
            Path.IsPathRooted(dataDirectory)
                ? dataDirectory
                : Path.Combine(
                    applicationDirectory,
                    dataDirectory);

        Directory.CreateDirectory(fullDataDirectory);

        return Path.Combine(
            fullDataDirectory,
            options.Value.DatabaseName);
    }
}