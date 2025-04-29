using Avalonia.Controls;

namespace MicMuter.Controls;

public partial class Setting : UserControl
{
    public string? Text
    {
        get => _textBlock.Text;
        set => _textBlock.Text = value;
    }

    // for some fucking reason when setting the content of the usercontrol directly (with the [Content] attribute)
    // it sets it before the component is initialized, meaning _contentControl would still be null so it throws.
    // it works fine when setting <UserControl.Content> directly.
    // [Content]
    public new object? Content
    {
        get => _contentControl.Content;
        set => _contentControl.Content = value;
    }
    
    public Setting()
    {
        InitializeComponent();
    }
}