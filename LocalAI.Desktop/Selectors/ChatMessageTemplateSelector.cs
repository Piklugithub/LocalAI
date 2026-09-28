using System.Windows;
using System.Windows.Controls;
using LocalAI.Desktop.ViewModels;

namespace LocalAI.Desktop.Selectors;

public sealed class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserTemplate { get; set; }

    public DataTemplate? AssistantTemplate { get; set; }

    public override DataTemplate? SelectTemplate(
        object item,
        DependencyObject container)
    {
        if (item is ChatMessageViewModel message)
        {
            return message.IsUser
                ? UserTemplate
                : AssistantTemplate;
        }

        return base.SelectTemplate(
            item,
            container);
    }
}