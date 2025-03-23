using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MicMuter.Audio;

namespace MicMuter.MainWindow;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;
    private bool _isEditingShortcut = false;
    private bool _loaded = false;

    public MainWindow()
    {
        throw new UnreachableException();
    }
    
    public MainWindow(MainWindowViewModel vm)
    {
        _vm = vm;
        DataContext = vm;
        Closing += OnClosing;
        Loaded += OnLoaded;
        InitializeComponent();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ResizeWindowToFitDeviceCombobox();
        _loaded = true;
    }

    private void DeviceCombobox_OnDropDownOpened(object? sender, EventArgs e)
    {
        _vm.RefreshDeviceList();
    }
    
    private void DeviceCombobox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_loaded) return;
        ResizeWindowToFitDeviceCombobox();
    }

    private void ResizeWindowToFitDeviceCombobox()
    {
        if (DeviceCombobox.SelectedIndex == -1 || string.IsNullOrEmpty(((IMicDevice?)DeviceCombobox.SelectedItem)?.FriendlyName)) return;
        
        double comboboxWidth = 44 + GetRenderedTextWidth(((IMicDevice)DeviceCombobox.SelectedItem!).FriendlyName, DeviceCombobox.FontFamily, DeviceCombobox.FontSize);
        var diff = this.Bounds.Width - 110 - comboboxWidth;
        if (diff < 0)
        {
            this.Width += -diff;
        }
    }
    
    private static double GetRenderedTextWidth(string text, FontFamily fontFamily, double fontSize)
    {
        var typeface = new Typeface(fontFamily);
        FormattedText formatted = new(
            text,
            Thread.CurrentThread.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            null);
        return formatted.Width;
    }

    private void EditShortcut_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        EndEditShortcut();
    }

    private void EditShortcut_OnClick(object? sender, RoutedEventArgs e)
    {
        StartEditShortcut();
    }

    private void EditShortcut_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_isEditingShortcut) return;
        
        switch (e.Key)
        {
            case Key.Escape:
                EndEditShortcut();
                return;
            
            case Key.Back or Key.Delete:
                _vm.Shortcut = default;
                return;
            
            case Key.LeftAlt or Key.LeftCtrl or Key.LeftShift or Key.RightAlt or Key.RightCtrl or Key.RightShift
                or Key.LWin or Key.RWin:
                _vm.Shortcut = new(Key.None, e.KeyModifiers);
                return;
            
            default:
                _vm.Shortcut = new(e.Key, e.KeyModifiers);
                EndEditShortcut();
                break;
        }
    }

    private void EditShortcut_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!_isEditingShortcut) return;

        var key = e.Key;
        
        if (e.Key is Key.LeftAlt or Key.LeftCtrl or Key.LeftShift or Key.RightAlt or Key.RightCtrl or Key.RightShift
            or Key.LWin or Key.RWin)
        {
            key = Key.None;
        }
        
        _vm.Shortcut = new(key, e.KeyModifiers);
    }

    private void StartEditShortcut()
    {
        _isEditingShortcut = true;
        ShortcutButton.Opacity = 0.6;
    }

    private void EndEditShortcut()
    {
        _isEditingShortcut = false;
        ShortcutButton.Opacity = 1;
    }
}