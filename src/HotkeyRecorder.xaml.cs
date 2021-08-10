using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MicMuter
{
    /// <summary>
    /// Interaction logic for HotkeyRecorder.xaml
    /// </summary>
    public partial class HotkeyRecorder : Window
    {
        private int[] keys = new int[2];
        private List<string> keyNames = new List<string>();
        private bool clearHotkey = false;
        private bool hotkeyHasBeenRecorded = false;

        public HotkeyRecorder()
        {
            InitializeComponent();
        }

        private void HotkeyRecorder_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Owner.IsHitTestVisible = true;
                    Owner.ResizeMode = ResizeMode.CanResize;
                    this.Close();
                    break;
                case Key.Return:
                    if (hotkeyHasBeenRecorded) ((MainWindow)Application.Current.MainWindow).ApplyHotkey(String.Join("+", keyNames), keys);
                    Owner.IsHitTestVisible = true;
                    Owner.ResizeMode = ResizeMode.CanResize;
                    this.Close();
                    break;
                default:
                    Record(e);
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            clearHotkey = !KeysDown().Any();
            if (!hotkeyHasBeenRecorded)
            {
                RecordAndDisplayModKeys();
            }
        }

        private void Record(KeyEventArgs e)
        {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
            if (key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            e.Handled = true;

            if (hotkeyHasBeenRecorded)
            {
                if (clearHotkey)
                {
                    Array.Clear(keys, 0, keys.Length);
                    keyNames.Clear();
                    hotkeyHasBeenRecorded = false;
                }
            }
            else
            {
                if (!IsModifierKey(key))
                {
                    keys[0] = (int)key;
                    keyNames.Add(key.ToString());
                    hotkeyHasBeenRecorded = true;
                    MuteShortcut.Text = String.Join("+", keyNames);
                }
                else
                {
                    RecordAndDisplayModKeys();
                }
            }
        }

        private void RecordAndDisplayModKeys()
        {
            keyNames.Clear();
            keys[1] = 0;
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                keys[1] = keys[1] | (int)WindowsHotkeys.KeyModifier.Ctrl;
                keyNames.Add("Ctrl");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
            {
                keys[1] = keys[1] | (int)WindowsHotkeys.KeyModifier.Shift;
                keyNames.Add("Shift");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) > 0)
            {
                keys[1] = keys[1] | (int)WindowsHotkeys.KeyModifier.Alt;
                keyNames.Add("Alt");
            }
            MuteShortcut.Text = String.Join("+", keyNames);
        }

        private bool IsModifierKey(Key key)
        {
            return (int)key >= 116 && (int)key <= 121;
        }

        private static IEnumerable<Key> KeysDown()
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key != Key.None && Keyboard.IsKeyDown(key) && key != Key.LWin && key != Key.RWin)
                    yield return key;
            }
        }
    }
}
