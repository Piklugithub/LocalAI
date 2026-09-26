using System.Windows;
using LocalAI.Application.Abstractions;
using LocalAI.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LocalAI.Desktop;

public partial class MainWindow
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IModelService _modelService;

    public MainWindow(
        IOptions<ApplicationOptions> applicationOptions,
        ILogger<MainWindow> logger,
        IModelService modelService)
    {
        InitializeComponent();

        _logger = logger;
        _modelService = modelService;

        Title = applicationOptions.Value.Name;

        _logger.LogInformation(
            "LocalAI desktop application started.");

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        var models = await _modelService
            .GetAvailableModelsAsync();

        _logger.LogInformation(
            "Application discovered {ModelCount} model(s).",
            models.Count);
    }
}