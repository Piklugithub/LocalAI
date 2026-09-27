using System.Windows;
using System.Windows.Input;
using LocalAI.Application.Abstractions;
using LocalAI.Application.Configuration;
using LocalAI.Application.Models;
using LocalAI.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LocalAI.Desktop;

public partial class MainWindow
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IModelService _modelService;
    private readonly IServiceScopeFactory _scopeFactory;

    private Conversation? _conversation;

    public MainWindow(
        IOptions<ApplicationOptions> applicationOptions,
        ILogger<MainWindow> logger,
        IModelService modelService,
        IServiceScopeFactory scopeFactory)
    {
        InitializeComponent();

        _logger = logger;
        _modelService = modelService;
        _scopeFactory = scopeFactory;

        Title = applicationOptions.Value.Name;

        _logger.LogInformation(
            "LocalAI desktop application started.");

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            var models = await _modelService
                .GetAvailableModelsAsync();

            _logger.LogInformation(
                "Application discovered {ModelCount} model(s).",
                models.Count);

            if (models.Count == 0)
            {
                ResponseTextBox.Text =
                    "No local GGUF model was found.";

                SendButton.IsEnabled = false;

                return;
            }

            _conversation = new Conversation(
                "LocalAI Test Conversation");

            ResponseTextBox.Text =
                $"Model ready: {models[0].Name}";
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to initialize LocalAI.");

            ResponseTextBox.Text =
                $"Initialization error: {ex.Message}";

            SendButton.IsEnabled = false;
        }
    }

    private async void SendButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private async void MessageTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        e.Handled = true;

        await SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        if (_conversation is null)
        {
            ResponseTextBox.Text =
                "No conversation is available.";

            return;
        }

        var message = MessageTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            SendButton.IsEnabled = false;
            MessageTextBox.IsEnabled = false;

            ResponseTextBox.Text = "Thinking...";

            MessageTextBox.Clear();

            using var scope =
                _scopeFactory.CreateScope();

            var chatService =
                scope.ServiceProvider
                    .GetRequiredService<IChatService>();

            var response =
                await chatService.SendMessageAsync(
                    new ChatRequest
                    {
                        Conversation = _conversation,
                        UserMessage = message
                    });

            ResponseTextBox.Text =
                response.Content;

            _logger.LogInformation(
                "Response generated in {Duration}.",
                response.Duration);
        }
        catch (OperationCanceledException)
        {
            ResponseTextBox.Text =
                "Generation cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while generating response.");

            ResponseTextBox.Text =
                $"Error: {ex.Message}";
        }
        finally
        {
            SendButton.IsEnabled = true;
            MessageTextBox.IsEnabled = true;

            MessageTextBox.Focus();
        }
    }
}