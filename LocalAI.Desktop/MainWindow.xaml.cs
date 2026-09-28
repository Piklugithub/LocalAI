using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using LocalAI.Application.Abstractions;
using LocalAI.Application.Configuration;
using LocalAI.Application.Models;
using LocalAI.Desktop.ViewModels;
using LocalAI.Domain.Entities;
using LocalAI.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LocalAI.Desktop;

public partial class MainWindow
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IModelService _modelService;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ObservableCollection<Conversation>
        _conversations = [];

    private readonly ObservableCollection<ChatMessageViewModel>
        _messages = [];

    private Conversation? _conversation;

    private bool _isLoadingConversation;

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

        ConversationsListBox.ItemsSource =
            _conversations;

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
            var models =
                await _modelService
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

            using var scope =
                _scopeFactory.CreateScope();

            var conversationService =
                scope.ServiceProvider
                    .GetRequiredService<IConversationService>();

            var conversations =
                await conversationService
                    .GetConversationsAsync();

            _logger.LogInformation(
                "Application loaded {ConversationCount} conversation(s).",
                conversations.Count);

            // Populate sidebar.
            _conversations.Clear();

            foreach (var conversation in conversations)
            {
                _conversations.Add(conversation);
            }

            if (conversations.Count > 0)
            {
                var latestConversationId =
                    conversations[0].Id;

                _isLoadingConversation = true;

                try
                {
                    _conversation =
                        await conversationService
                            .GetConversationAsync(
                                latestConversationId);

                    if (_conversation is not null)
                    {
                        LoadConversationMessages(
                            _conversation);

                        ConversationsListBox.SelectedItem =
                            conversations.FirstOrDefault(
                                x =>
                                    x.Id ==
                                    _conversation.Id);

                        _logger.LogInformation(
                            "Restored conversation {ConversationId}: {ConversationTitle}.",
                            _conversation.Id,
                            _conversation.Title);
                    }
                }
                finally
                {
                    _isLoadingConversation = false;
                }
            }
            else
            {
                _conversation =
                    new Conversation(
                        "New Conversation");

                _logger.LogInformation(
                    "No existing conversations found. " +
                    "Created a new conversation.");
            }

            _logger.LogInformation(
                "Model ready: {ModelName}.",
                models[0].Name);

            ScrollChatToBottom();
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

    private async void ConversationsListBox_SelectionChanged(
        object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isLoadingConversation)
        {
            return;
        }

        if (ConversationsListBox.SelectedItem
            is not Conversation selectedConversation)
        {
            return;
        }

        await LoadConversationAsync(
            selectedConversation.Id);
    }

    private async Task LoadConversationAsync(
        Guid conversationId)
    {
        _isLoadingConversation = true;

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var conversationService =
                scope.ServiceProvider
                    .GetRequiredService<IConversationService>();

            var conversation =
                await conversationService
                    .GetConversationAsync(
                        conversationId);

            if (conversation is null)
            {
                _logger.LogWarning(
                    "Conversation {ConversationId} was not found.",
                    conversationId);

                return;
            }

            _conversation = conversation;

            LoadConversationMessages(
                conversation);

            _logger.LogInformation(
                "Loaded conversation {ConversationId}: {ConversationTitle}.",
                conversation.Id,
                conversation.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load conversation {ConversationId}.",
                conversationId);

            _messages.Clear();

            _messages.Add(
                new ChatMessageViewModel(
                    false,
                    $"Failed to load conversation: {ex.Message}"));
        }
        finally
        {
            _isLoadingConversation = false;
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

        var message =
            MessageTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _generationCancellation =
            new CancellationTokenSource();

        try
        {
            _conversation.GenerateTitleFromFirstMessage(message);
            // Add user message to UI.
            _messages.Add(
                new ChatMessageViewModel(
                    true,
                    message));

            // Create assistant message.
            // Streaming tokens will be appended to it.
            var assistantMessage =
                new ChatMessageViewModel(
                    false,
                    string.Empty);

            _messages.Add(
                assistantMessage);

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

            AddConversationToSidebarIfNeeded();

            _logger.LogInformation(
                "Streaming response completed.");

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

    private void AddConversationToSidebarIfNeeded()
    {
        if (_conversation is null)
        {
            return;
        }

        var existingConversation =
            _conversations.FirstOrDefault(
                x => x.Id == _conversation.Id);

        if (existingConversation is null)
        {
            _conversations.Insert(
                0,
                _conversation);

            _logger.LogInformation(
                "Added new conversation {ConversationId} to sidebar.",
                _conversation.Id);
        }

        ConversationsListBox.Items.Refresh();

        ConversationsListBox.SelectedItem =
            _conversation;
    }
    private void ScrollChatToBottom()
    {
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }));
    }

    private void LoadConversationMessages(
        Conversation conversation)
    {
        _messages.Clear();

        foreach (var message in conversation.Messages)
        {
            var isUser =
                message.Role == ChatRole.User;

            _messages.Add(
                new ChatMessageViewModel(
                    isUser,
                    message.Content));
        }

        ScrollChatToBottom();
    }

    private void NewChatButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StartNewConversation();
    }

    private void StartNewConversation()
    {
        _generationCancellation?.Cancel();

        _conversation =
            new Conversation(
                "New Conversation");

        _messages.Clear();

        MessageTextBox.Clear();

        ConversationsListBox.SelectedItem = null;

        MessageTextBox.Focus();

        _logger.LogInformation(
            "Started a new conversation {ConversationId}.",
            _conversation.Id);
    }
}