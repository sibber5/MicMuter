using System;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MicMuter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LoadMainWindow();

            SaveButton.IsEnabled = false;
        }

        #region Event Handlers

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                MessageBoxResult result = MessageBox.Show("You Haven't saved\nDo you want to close and discard changes?", "Changes not saved", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        App.LoadSettings();
                        break;
                    case MessageBoxResult.No:
                        e.Cancel = true;
                        break;
                }
            }
            if (App.Settings.NotifyRunning)
            {
                App.ShowRunningNotification();
            }
        }

        private void MuteHotkeyButton_Clicked(object sender, RoutedEventArgs e)
        {
            OpenHotkeyRecorder();
        }

        private void InputDevices_SelectionChanged(object sender, EventArgs e)
        {
            App.Settings.InputDeviceID = App.InputDevices[InputDevicesComboBox.SelectedIndex].ID;
            App.SelectedInputDevice = App.InputDevices[InputDevicesComboBox.SelectedIndex];

            OnSettingChanged(sender, null);
        }

        private void StartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            if (App.HasAdminPrivileges) RequireAdminCheckBox.IsEnabled = true;

            OnSettingChanged(sender, null);
        }
        private void StartWithWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            RequireAdminCheckBox.IsEnabled = false;

            OnSettingChanged(sender, null);
        }

        private void OnSettingChanged(object sender, RoutedEventArgs e)
        {
            if (SaveButton != null) SaveButton.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            App.SaveSettings();
            SaveButton.IsEnabled = false;
        }

        #endregion

        private void LoadMainWindow()
        {
            int selectedIndex = -1;
            for (int i = 0; i < App.InputDevices.Count; i++)
            {
                InputDevicesComboBox.Items.Add(App.InputDevices[i].FriendlyName);
                if (App.Settings.InputDeviceID == null)
                {
                    continue;
                }
                if (App.InputDevices[i].ID == App.Settings.InputDeviceID)
                {
                    selectedIndex = i;
                }
            }
            InputDevicesComboBox.SelectedIndex = selectedIndex;

            MuteHotkeyButton.Content = GetHotkeyString();

            if (App.HasAdminPrivileges)
            {
                RequireAdminCheckBox.IsEnabled = App.Settings.StartWithWindows;
            }
            else
            {
                if (App.Settings.RequireAdmin)
                {
                    StartWithWindowsCheckBox.IsEnabled = false;
                    StartWithWindowsCheckBox.ToolTip = "Run MicMuter as Administrator to change this setting.\n" + StartWithWindowsCheckBox.ToolTip;
                }

                RequireAdminCheckBox.IsEnabled = false;
                RequireAdminCheckBox.ToolTip = "Run MicMuter as Administrator to change this setting.\n" + RequireAdminCheckBox.ToolTip;
            }
        }

        #region Hotkey related methods

        private Window MakeGrayWindow(Window mainWindow)
        {
            return new Window()
            {
                Background = Brushes.Black,
                Opacity = 0.7,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                WindowState = WindowState.Normal,
                Width = mainWindow.Width - 14,
                Height = mainWindow.Height - 7,
                Left = mainWindow.Left + 7,
                Top = mainWindow.Top,
                IsHitTestVisible = false,
                ShowInTaskbar = false
            };
        }

        public void ApplyHotkey(string hotkeyText, int[] keys)
        {
            SaveButton.IsEnabled = true;

            if (keys.Length != 2)
            {
                throw new ArgumentOutOfRangeException("Invalid array size", nameof(keys));
            }

            MuteHotkeyButton.Content = hotkeyText;
            App.SetNewHotkey(keys[0], keys[1]);
        }

        public void RemoveOwner()
        {
            this.Owner = null;
        }

        public void OpenHotkeyRecorder()
        {
            Window mainWindow = Application.Current.MainWindow;
            this.IsHitTestVisible = false;
            var grayOut = MakeGrayWindow(mainWindow);
            grayOut.Show();
            Window hotkeyRecorder = new HotkeyRecorder()
            {
                Left = mainWindow.Left + mainWindow.Width / 2 - 105,
                Top = mainWindow.Top + mainWindow.Height / 2 - 43
            };
            HotkeyRecorder.SetGrayOutWindow(grayOut);
            hotkeyRecorder.Show();
            this.Owner = hotkeyRecorder;
            grayOut.Owner = hotkeyRecorder;
        }

        private string GetHotkeyString()
        {
            StringBuilder sb = new StringBuilder();
            if (App.Settings.HotkeyModifiers != 0)
            {
                if (App.Settings.HotkeyModifiers == 2 || App.Settings.HotkeyModifiers == 3 || App.Settings.HotkeyModifiers == 6 || App.Settings.HotkeyModifiers == 7)
                {
                    sb.Append("Ctrl+");
                }
                if (App.Settings.HotkeyModifiers >= 4 || App.Settings.HotkeyModifiers <= 7)
                {
                    sb.Append("Shift+");
                }
                if (App.Settings.HotkeyModifiers % 2 != 0)
                {
                    sb.Append("Alt+");
                }
            }
            sb.Append(((System.Windows.Input.Key)App.Settings.HotkeyKey).ToString());
            return sb.ToString();
        }
        
        #endregion
    }
}
