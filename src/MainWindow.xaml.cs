using System;
using System.Text;
using System.Windows;

namespace MicMuter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.MainWind = this;
            LoadMainWindow();

            SaveButton.IsEnabled = false;
        }

        #region Event Handlers

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                MessageBoxResult result = MessageBox.Show("Changes haven't been saved.\nClose and discard changes?", "Changes not saved", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        App.LoadSettings();
                        break;
                    case MessageBoxResult.No:
                        e.Cancel = true;
                        return;
                }
            }
            if (App.Settings.NotifyRunning)
            {
                App.ShowRunningNotification();
            }
            App.MainWind = null;
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
        }

        #region Hotkey related methods

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

        public void OpenHotkeyRecorder()
        {
            this.IsHitTestVisible = false;
            this.ResizeMode = ResizeMode.NoResize;
            Window hotkeyRecorder = new HotkeyRecorder()
            {
                Height = this.Height - 16 + 7,
                Width = this.Width - 16,
                Owner = this,
                Top = this.Top + 1,
                Left = this.Left + 8
            };
            hotkeyRecorder.Show();
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
