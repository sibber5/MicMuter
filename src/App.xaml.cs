using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;
using NAudio.CoreAudioApi;
using WindowsHotkeys;
using System.Reflection;

namespace MicMuter
{
    public partial class App : Application
    {
        public static MainWindow MainWind = null;

        public static readonly string Name = "MicMuter";
        public static readonly string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MicMuter\";
        public static readonly string SaveFileName = @"UserSettings.xml";

        public static MMDeviceCollection InputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        public static MMDevice SelectedInputDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);

        public static Settings Settings { get; private set; } = new Settings();
        private static bool? RequiredAdmin = null;

        private static List<HotKey> hotkey = new List<HotKey>();

        private static TaskbarIcon tb;

        void App_Startup(object sender, StartupEventArgs e)
        {
            tb = (TaskbarIcon)FindResource("NotifyIcon");

            LoadSettings();
            RequiredAdmin = Settings.RequireAdmin;

            // in case the setting was changed from the settings file
            if (!App.Settings.RequireAdmin) SetStartupKey(App.Settings.StartWithWindows);

            if (!App.Settings.StartMinimized)
            {
                this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }

            if (App.Settings.NotifyRunning)
            {
                ShowRunningNotification();
            }
        }

        public static void Close()
        {
            /*var mainWins = Application.Current.Windows.OfType<MainWindow>();
            if (mainWins.Any())
            {
                mainWins.First().Close();
            }*/
            foreach (HotKey h in hotkey)
            {
                if (h != null) h.Unregister();
            }
            Application.Current.Shutdown();
        }

        private void tbSettings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
        private void tbExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowRunningNotification()
        {
            tb.ShowBalloonTip(Name, Name + " is running in the background", BalloonIcon.Info);
        }

        private static void OnHotkey(HotKey hotKey)
        {
            if (!App.Settings.MuteMicHotkeyIsEnabled) return;

            ToggleMicMute();
        }

        public static void SetNewHotkey(int key, int modifiers)
        {
            foreach (HotKey h in hotkey)
            {
                if (h != null) h.Unregister();
            }
            hotkey.Clear();

            hotkey.Add(new HotKey((Key)key, (KeyModifier)modifiers, OnHotkey));

            // if for example we do new Hotkey(Key.F7, KeyModifier.None, OnHotkey), then OnHotkey will only be called
            // when F7 is down *and* Shift, Alt and Control are all up. KeyModifier has to match the state off all modifier keys.
            // so we if this is the case, we add a hotkey for when any modifier key is down (or specific ones depending on the hotkey)
            if (modifiers != 7)
            {
                switch (modifiers)
                {
                    case 0:
                        for (int i = 1; i < 7; i++)
                        {
                            hotkey.Add(new HotKey((Key)key, (KeyModifier)i, OnHotkey));
                        }
                        break;
                    case 1:
                        hotkey.Add(new HotKey((Key)key, (KeyModifier)3, OnHotkey));
                        hotkey.Add(new HotKey((Key)key, (KeyModifier)5, OnHotkey));
                        break;
                    case 2:
                        hotkey.Add(new HotKey((Key)key, (KeyModifier)3, OnHotkey));
                        hotkey.Add(new HotKey((Key)key, (KeyModifier)6, OnHotkey));
                        break;
                    case 4:
                        hotkey.Add(new HotKey((Key)key, (KeyModifier)5, OnHotkey));
                        hotkey.Add(new HotKey((Key)key, (KeyModifier)6, OnHotkey));
                        break;
                }

                hotkey.Add(new HotKey((Key)key, (KeyModifier)7, OnHotkey));
            }
            Debug.WriteLine("\nkey modifieres: " + ((KeyModifier)modifiers) + "\nkey modifieres string: " + ((KeyModifier)modifiers).ToString());

            App.Settings.HotkeyKey = key;
            App.Settings.HotkeyModifiers = modifiers;
        }

        private static void ToggleMicMute()
        {
            SelectedInputDevice.AudioEndpointVolume.Mute = !SelectedInputDevice.AudioEndpointVolume.Mute;
        }

        public static void SetStartupTask(bool enable) // use a windows task so the UAC prompt doesn't pop up every time the user logs in
        {
            MessageBox.Show("Administrator privilages required to set the startup task (enable/disable autorun on log in)", "UAC", MessageBoxButton.OK, MessageBoxImage.Information);
            string pathToExe = Assembly.GetExecutingAssembly().Location;
            ProcessStartInfo taskCreator = new ProcessStartInfo(System.IO.Path.GetDirectoryName(pathToExe) + "\\StartupTaskCreator\\StartupTaskCreator.exe",
                $"{(enable ? "1" : "0")} {App.Name} \"{pathToExe}\"")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };
            using (var process = Process.Start(taskCreator))
            {
                process.WaitForExit();
            }
        }
        public static void SetStartupKey(bool create)
        {
            string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(path, true);

            if (create)
            {
                key.SetValue(Name, Assembly.GetExecutingAssembly().Location);
                Debug.WriteLine("\nCreated startup registry key");
            }
            else
            {
                if (key.GetValue(Name) != null)
                {
                    key.DeleteValue(Name);
                    Debug.WriteLine("\nDeleted startup registry key");
                }
            }
        }
        public static void SetStartWithWindows(bool enable)
        {
            if (enable)
            {
                if (App.Settings.RequireAdmin)
                {
                    SetStartupKey(false);
                    SetStartupTask(true);
                }
                else
                {
                    if (RequiredAdmin == true) SetStartupTask(false);
                    SetStartupKey(true);
                }
            }
            else
            {
                if (RequiredAdmin == true) SetStartupTask(false);
                SetStartupKey(false);
            }

            Debug.WriteLine($"\n{(enable ? "Enabled" : "Disabled")} startup task");
        }

        public static void SaveSettings()
        {
            SetStartWithWindows(App.Settings.StartWithWindows);
            try
            {
                InstanceSerializer.Serialize(App.Settings, SavePath, SaveFileName);
            }
            catch (System.Runtime.Serialization.SerializationException e)
            {
                MessageBox.Show("Saving failed.\n" + e.Message, "Serialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            RequiredAdmin = App.Settings.RequireAdmin;
        }

        public static void LoadSettings()
        {
            Settings loadedSettings = null;
            try
            {
                loadedSettings = InstanceSerializer.Deserialize<Settings>(SavePath, SaveFileName);
            }
            catch (System.Runtime.Serialization.SerializationException e)
            {
                MessageBox.Show("Loading failed.\n" + e.Message, "Deserialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (loadedSettings == null)
            {
                SaveSettings();
            }
            else
            {
                App.Settings = loadedSettings;
            }
            foreach (MMDevice device in InputDevices)
            {
                if (device.ID == App.Settings.InputDeviceID)
                {
                    SelectedInputDevice = device;
                    break;
                }
            }
            SetNewHotkey(App.Settings.HotkeyKey, App.Settings.HotkeyModifiers);
        }
    }
}
