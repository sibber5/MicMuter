using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter.MainWindow;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;
    private bool _isEditingShortcut = false;

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
        Helpers.DebugWriteLine("Initialized.");
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        EndEditShortcut();
        e.Cancel = true;
        Hide();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ResizeWindowToFitDeviceCombobox();
    }

    private void DeviceCombobox_OnDropDownOpened(object? sender, EventArgs e)
    {
        _vm.RefreshDeviceList();
    }
    
    private void ResizeWindowToFitDeviceCombobox()
    {
        if (DeviceCombobox.SelectedIndex == -1 || string.IsNullOrEmpty(((IMicDevice?)DeviceCombobox.SelectedItem)?.FriendlyName)) return;
        
        double comboboxWidth = 44 + Helpers.GetRenderedTextDimensions(((IMicDevice)DeviceCombobox.SelectedItem!).FriendlyName, DeviceCombobox.FontFamily, DeviceCombobox.FontSize).Width;
        var diff = this.Bounds.Width - 110 - comboboxWidth;
        if (diff < 0)
        {
            this.Width += -diff;
        }
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
                EndEditShortcut();
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
        
        if (e.Key is Key.LeftAlt or Key.LeftCtrl or Key.LeftShift or Key.RightAlt or Key.RightCtrl or Key.RightShift
            or Key.LWin or Key.RWin)
        {
            _vm.Shortcut = new(Key.None, e.KeyModifiers);
            return;
        }
        
        // This only executes if the key didnt trigger OnKeyDown, which means it is registered in a hotkey either by MicMuter or another application.
        // If it is registered by another application, then we cant register it, so do nothing.
        // If it is registered by MicMuter, then set it again in the viewmodel.
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers);
        
        if ((!_vm.Settings.IgnoreExtraModifiers && shortcut == _vm.Settings.MuteShortcut)
            || (_vm.Settings.IgnoreExtraModifiers && _vm.Settings.MuteShortcut.IsTriggeredBy(shortcut)))
        {
            _vm.Shortcut = _vm.Settings.MuteShortcut;
        }
        
        EndEditShortcut();
    }

    private void StartEditShortcut()
    {
        _isEditingShortcut = true;
        ShortcutButton.Opacity = 0.6;
        _vm._updateSettings = false;
    }

    private void EndEditShortcut()
    {
        _isEditingShortcut = false;
        ShortcutButton.Opacity = 1;
        _vm._updateSettings = true;
        _vm.Settings.MuteShortcut = _vm.Shortcut;
    }
}