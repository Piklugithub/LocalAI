using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using LocalAI.Application.Abstractions;
using LocalAI.Application.Configuration;
using LocalAI.Application.Models;
using LocalAI.Desktop.ViewModels;
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

    private readonly ObservableCollection<ChatMessageViewModel>
        _messages = [];

    private Conversation? _conversation;

    private CancellationTokenSource? _generationCancellation;

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

        MessagesItemsControl.ItemsSource = _messages;

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
                _messages.Add(
                    new ChatMessageViewModel(
                        false,
                        "No local GGUF model was found."));

                SendButton.IsEnabled = false;

                return;
            }

            _conversation = new Conversation(
                "LocalAI Test Conversation");

            _logger.LogInformation(
                "Model ready: {ModelName}.",
                models[0].Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to initialize LocalAI.");

            _messages.Add(
                new ChatMessageViewModel(
                    false,
                    $"Initialization error: {ex.Message}"));

            SendButton.IsEnabled = false;
        }
    }

    private async void SendButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_generationCancellation is not null)
        {
            _generationCancellation.Cancel();
            return;
        }

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

        if (_generationCancellation is not null)
        {
            return;
        }

        await SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        if (_conversation is null)
        {
            _messages.Add(
                new ChatMessageViewModel(
                    false,
                    "No conversation is available."));

            return;
        }

        var message = MessageTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _generationCancellation =
            new CancellationTokenSource();

        try
        {
            // Add the user's message to the UI.
            _messages.Add(
                new ChatMessageViewModel(
                    true,
                    message));

            // Create an empty assistant message.
            // Streaming tokens will be appended to this.
            var assistantMessage =
                new ChatMessageViewModel(
                    false,
                    string.Empty);

            _messages.Add(assistantMessage);

            MessageTextBox.Clear();

            SendButton.IsEnabled = true;
            SendButton.Content = "Cancel";

            MessageTextBox.IsEnabled = false;

            ScrollChatToBottom();

            using var scope =
                _scopeFactory.CreateScope();

            var chatService =
                scope.ServiceProvider
                    .GetRequiredService<IChatService>();

            var request = new ChatRequest
            {
                Conversation = _conversation,
                UserMessage = message
            };

            await foreach (
                var token in chatService
                    .StreamMessageAsync(
                        request,
                        _generationCancellation.Token)
                    .WithCancellation(
                        _generationCancellation.Token))
            {
                assistantMessage.Content += token;

                ScrollChatToBottom();
            }

            _logger.LogInformation(
                "Streaming response completed.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "LLM generation was cancelled by the user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while generating response.");

            _messages.Add(
                new ChatMessageViewModel(
                    false,
                    $"Error: {ex.Message}"));
        }
        finally
        {
            _generationCancellation?.Dispose();
            _generationCancellation = null;

            SendButton.Content = "Send";
            SendButton.IsEnabled = true;

            MessageTextBox.IsEnabled = true;
            MessageTextBox.Focus();

            ScrollChatToBottom();
        }
    }

    private void ScrollChatToBottom()
    {
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }));
    }
}