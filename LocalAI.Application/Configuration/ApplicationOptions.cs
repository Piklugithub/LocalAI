namespace LocalAI.Application.Configuration;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string Name { get; set; } = "LocalAI";

    public string DataDirectory { get; set; } = "data";

    public string DatabaseName { get; set; } = "LocalAI.db";
}