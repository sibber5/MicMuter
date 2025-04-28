using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    
    public DialogWindow(string title, string message, string? acceptText, string? cancelText, IImage? acceptIcon = null, IImage? cancelIcon = null, double width = 350, double height = -1, bool canResize = false, bool monospace = false)
    {
        DataContext = this;
        Title = title;
        Message = message;
        AcceptIcon = acceptIcon;
        AcceptText = acceptText;
        CancelIcon = cancelIcon;
        CancelText = cancelText;
        Width = width;
        if (height >= 0) Height = height;
        CanResize = canResize;
        
        InitializeComponent();
        
        if (monospace) Text.FontFamily = new FontFamily("Consolas");
        
        double contentHeight = ResizeToContent();
        if (height < 0 || height > contentHeight) Height = contentHeight;
    }
    
    private double ResizeToContent()
    {
        var textHeight = Helpers.GetRenderedTextDimensions(Text.Text!, Text.FontFamily, Text.FontSize, Width - OuterGrid.Margin.Left - OuterGrid.Margin.Right).Height;
        return 1 + textHeight + OuterGrid.Margin.Top + OuterGrid.Margin.Bottom + OuterGrid.RowDefinitions.Where(x => x.Height is { IsAuto: false, IsStar: false }).Sum(x => x.Height.Value) + ButtonRow.Height;
    }

    private void AcceptButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(DialogResult.Accept);
    }
    
    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;
    
    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(DialogResult.Cancel);
    }
    
    private void CustomTitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState is WindowState.FullScreen) return;
        if (WindowState is WindowState.Maximized) WindowState = WindowState.Normal;
        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }
    
    private void CustomTitleBar_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }
    
    private void CustomTitleBar_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;
        
        PointerPoint currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(
            Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X), 
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y)
        );
    }
}
