using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MicMuter.AppSettings;
using MicMuter.Audio;
using MicMuter.Hotkeys;
using Microsoft.Extensions.Logging;

namespace MicMuter.MainWindow;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly MainWindowViewModel _vm;

    private readonly ObservableCollection<string> _muteSfxSelections = [SfxSelection.DefaultMute.DisplayName, SfxSelection.None.DisplayName, "Browse..."];
    private readonly ObservableCollection<string> _unmuteSfxSelections = [SfxSelection.DefaultUnmute.DisplayName, SfxSelection.None.DisplayName, "Browse..."];
    private const int DefaultSfxIdx = 0;
    private const int NoneSfxIdx = 1;
    private const int CustomFileSfxIdx = 2;
    private static readonly Index BrowseSfxIdx = ^1;
    private bool _isChangingSfx = false;

    private bool _isEditingShortcut = false;

    public MainWindow() => throw new UnreachableException();

    public MainWindow(MainWindowViewModel vm, ILogger<MainWindow> logger)
    {
        _logger = logger;
        _vm = vm;
        DataContext = _vm;
        Closing += OnClosing;
        Loaded += OnLoaded;
        InitializeComponent();

        MuteSfxCombobox.ItemsSource = _muteSfxSelections;
        UpdateSfxComboBoxFromSettings(MuteSfxCombobox, _vm.Settings.MuteSfx);
        UnmuteSfxCombobox.ItemsSource = _unmuteSfxSelections;
        UpdateSfxComboBoxFromSettings(UnmuteSfxCombobox, _vm.Settings.UnmuteSfx);
        _vm.Settings.PropertyChanged += (_, args) =>
        {
            if (!_isChangingSfx && nameof(Settings.MuteSfx).Equals(args.PropertyName, StringComparison.Ordinal))
            {
                UpdateSfxComboBoxFromSettings(MuteSfxCombobox, _vm.Settings.MuteSfx);
                _logger.LogInformation("Mute SFX updated from ViewModel to: {SfxSelection}", _vm.Settings.MuteSfx);
            }
            else if (!_isChangingSfx && nameof(Settings.UnmuteSfx).Equals(args.PropertyName, StringComparison.Ordinal))
            {
                UpdateSfxComboBoxFromSettings(UnmuteSfxCombobox, _vm.Settings.UnmuteSfx);
                _logger.LogInformation("Unmute SFX updated from ViewModel to: {SfxSelection}", _vm.Settings.UnmuteSfx);
            }
        };
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

    private async void SfxCombobox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isChangingSfx) return;
        _isChangingSfx = true;

        VerifySfxComboBoxShape();
        var comboBox = (ComboBox)sender!;
        var selections = (ObservableCollection<string>)comboBox.ItemsSource!;
        bool isMuteSfx = selections == _muteSfxSelections;

        try
        {
            if (selections.Count > 3) selections.RemoveAt(CustomFileSfxIdx);

            var (sfx, comboBoxIdx) = comboBox.SelectedIndex switch
            {
                DefaultSfxIdx => (isMuteSfx ? SfxSelection.DefaultMute : SfxSelection.DefaultUnmute, DefaultSfxIdx),
                NoneSfxIdx => (SfxSelection.None, NoneSfxIdx),
                _ when (string?)comboBox.SelectedItem == selections[BrowseSfxIdx]
                    => await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = $"Select a sound effect for {(isMuteSfx ? "muting" : "unmuting")} the microphone",
                        SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Music),
                        AllowMultiple = false,
                        FileTypeFilter = [new FilePickerFileType(null) { Patterns = ["*.wav"] }],
                    }) is [var file] ? (new SfxSelection(file.Path.LocalPath), CustomFileSfxIdx) : (SfxSelection.None, NoneSfxIdx),
                _ => throw new UnreachableException(),
            };

            if (comboBoxIdx == CustomFileSfxIdx)
            {
                ToolTip.SetTip(comboBox, sfx.Path);
                selections.Insert(CustomFileSfxIdx, sfx.DisplayName);
            }
            else comboBox.ClearValue(ToolTip.TipProperty);

            comboBox.SelectedIndex = comboBoxIdx;
            if (isMuteSfx) _vm.Settings.MuteSfx = sfx;
            else _vm.Settings.UnmuteSfx = sfx;

            _logger.LogInformation("{MuteOrUnmute} sfx updated from View{FromBrowse} to: {SfxSelection}", isMuteSfx ? "Mute" : "Unmute", comboBoxIdx == CustomFileSfxIdx ? " (browse)" : "", sfx);
        }
        catch (Exception ex)
        {
            try
            {
                if (selections.Count > 3) selections.RemoveAt(CustomFileSfxIdx);
                comboBox.SelectedIndex = NoneSfxIdx;
                if (isMuteSfx) _vm.Settings.MuteSfx = SfxSelection.None;
                else _vm.Settings.UnmuteSfx = SfxSelection.None;

                Program.OnUnhandledException(ex);
            }
            catch (Exception ex2)
            {
                Program.OnUnhandledException(new AggregateException(ex, ex2));
            }
        }
        finally
        {
            _isChangingSfx = false;
        }
    }

    private void UpdateSfxComboBoxFromSettings(ComboBox comboBox, SfxSelection sfx)
    {
        if (_isChangingSfx) return;
        _isChangingSfx = true;

        VerifySfxComboBoxShape();
        var selections = (ObservableCollection<string>)comboBox.ItemsSource!;

        if (selections.Count > 3) selections.RemoveAt(CustomFileSfxIdx);

        if (sfx == SfxSelection.None)
        {
            comboBox.SelectedIndex = NoneSfxIdx;
            comboBox.ClearValue(ToolTip.TipProperty);
        }
        else if (sfx == SfxSelection.DefaultMute || sfx == SfxSelection.DefaultUnmute)
        {
            comboBox.SelectedIndex = DefaultSfxIdx;
            comboBox.ClearValue(ToolTip.TipProperty);
        }
        else
        {
            ToolTip.SetTip(comboBox, sfx.Path);
            selections.Insert(CustomFileSfxIdx, sfx.DisplayName);
            comboBox.SelectedIndex = CustomFileSfxIdx;
        }

        _isChangingSfx = false;
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
            case Key.Return:
                EndEditShortcut();
                return;

            case Key.Escape:
                _vm.Shortcut = _vm.Settings.MuteShortcut;
                EndEditShortcut();
                break;

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
        // If it is registered by MicMuter, then set it in the viewmodel to register it.

        Shortcut newShortcut = new(e.Key, e.KeyModifiers);
        bool isRegisteredByMicMuter = (!_vm.Settings.IgnoreExtraModifiers && newShortcut == _vm.Settings.MuteShortcut)
                                      || (_vm.Settings.IgnoreExtraModifiers && _vm.Settings.MuteShortcut.IsTriggeredBy(newShortcut));

        if (isRegisteredByMicMuter) _vm.Shortcut = newShortcut;

        EndEditShortcut();
    }

    private void StartEditShortcut()
    {
        _isEditingShortcut = true;
        ShortcutButton.Opacity = 0.6;
        _vm._isUpdatingFromSettings = false;
    }

    private void EndEditShortcut()
    {
        _isEditingShortcut = false;
        ShortcutButton.Opacity = 1;
        _vm._isUpdatingFromSettings = true;
        _vm.Settings.MuteShortcut = _vm.Shortcut;
    }


    [Conditional("DEBUG")]
    private void VerifySfxComboBoxShape()
    {
        Debug.Assert(_muteSfxSelections.Count is 3 or 4);
        Debug.Assert(_muteSfxSelections[DefaultSfxIdx] == SfxSelection.DefaultMute.DisplayName);
        Debug.Assert(_muteSfxSelections[NoneSfxIdx] == SfxSelection.None.DisplayName);
        Debug.Assert(_muteSfxSelections[BrowseSfxIdx] == "Browse...");

        Debug.Assert(_unmuteSfxSelections.Count is 3 or 4);
        Debug.Assert(_unmuteSfxSelections[DefaultSfxIdx] == SfxSelection.DefaultUnmute.DisplayName);
        Debug.Assert(_unmuteSfxSelections[NoneSfxIdx] == SfxSelection.None.DisplayName);
        Debug.Assert(_unmuteSfxSelections[BrowseSfxIdx] == "Browse...");
    }
}
