using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LocalAI.Desktop.ViewModels;

public sealed class ChatMessageViewModel : INotifyPropertyChanged
{
    private string _content;

    public ChatMessageViewModel(
        bool isUser,
        string content)
    {
        IsUser = isUser;
        _content = content;
    }

    public bool IsUser { get; }

    public string Content
    {
        get => _content;
        set
        {
            if (_content == value)
            {
                return;
            }

            _content = value;

            OnPropertyChanged();
        }
    }

    public string SenderName =>
        IsUser ? "You" : "LocalAI";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}