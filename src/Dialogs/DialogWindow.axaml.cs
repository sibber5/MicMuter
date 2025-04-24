using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace MicMuter.Dialogs;

public partial class DialogWindow : Window
{
    public string Message { get; }
    public IImage? AcceptIcon { get; }
    public string? AcceptText { get; }
    public IImage? CancelIcon { get; }
    public string? CancelText { get; }
    
    public DialogWindow() => throw new NotSupportedException();
    
    public DialogWindow(string title, string message, string? acceptText, string? cancelText, IImage? acceptIcon = null, IImage? cancelIcon = null, double width = 250)
    {
        DataContext = this;
        Title = title;
        Message = message;
        AcceptIcon = acceptIcon;
        AcceptText = acceptText;
        CancelIcon = cancelIcon;
        CancelText = cancelText;
        Width = width;
        InitializeComponent();
        ResizeToContent();
    }

    private void ResizeToContent()
    {
        var textHeight = Helpers.GetRenderedTextDimensions(Text.Text!, Text.FontFamily, Text.FontSize, Width - OuterStackPanel.Margin.Left - OuterStackPanel.Margin.Right).Height;
        Height = textHeight + OuterStackPanel.Margin.Top + OuterStackPanel.Margin.Bottom + OuterStackPanel.Spacing + ButtonRow.Height;
    }

    private void AcceptButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(DialogResult.Accept);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(DialogResult.Cancel);
    }
}
