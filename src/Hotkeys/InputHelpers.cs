using System;
using Avalonia.Input;

namespace MicMuter.Hotkeys;

internal static class InputHelpers
{
    public static uint KeyToWindowsVirtualKey(Key key) => key switch
    {
        // VirtualKey.None - 100% match
        Key.None => 0,

        // VirtualKey.Cancel - 100% match
        Key.Cancel => 3,

        // VirtualKey.Back - 100% match
        Key.Back => 8,

        // VirtualKey.Tab - 100% match
        Key.Tab => 9,

        // VirtualKey.Clear - 100% match
        Key.Clear => 12,

        // VirtualKey.Pause - 100% match
        Key.Pause => 19,

        // VirtualKey.Escape - 100% match
        Key.Escape => 27,

        // VirtualKey.Space - 100% match
        Key.Space => 32,

        // VirtualKey.PageUp - 100% match
        Key.PageUp => 33,

        // VirtualKey.PageDown - 100% match
        Key.PageDown => 34,

        // VirtualKey.End - 100% match
        Key.End => 35,

        // VirtualKey.Home - 100% match
        Key.Home => 36,

        // VirtualKey.Left - 100% match
        Key.Left => 37,

        // VirtualKey.Up - 100% match
        Key.Up => 38,

        // VirtualKey.Right - 100% match
        Key.Right => 39,

        // VirtualKey.Down - 100% match
        Key.Down => 40,

        // VirtualKey.Select - 100% match
        Key.Select => 41,

        // VirtualKey.Print - 100% match
        Key.Print => 42,

        // VirtualKey.Execute - 100% match
        Key.Execute => 43,

        // VirtualKey.Insert - 100% match
        Key.Insert => 45,

        // VirtualKey.Delete - 100% match
        Key.Delete => 46,

        // VirtualKey.Help - 100% match
        Key.Help => 47,

        // VirtualKey.A - 100% match
        Key.A => 65,

        // VirtualKey.B - 100% match
        Key.B => 66,

        // VirtualKey.C - 100% match
        Key.C => 67,

        // VirtualKey.D - 100% match
        Key.D => 68,

        // VirtualKey.E - 100% match
        Key.E => 69,

        // VirtualKey.F - 100% match
        Key.F => 70,

        // VirtualKey.G - 100% match
        Key.G => 71,

        // VirtualKey.H - 100% match
        Key.H => 72,

        // VirtualKey.I - 100% match
        Key.I => 73,

        // VirtualKey.J - 100% match
        Key.J => 74,

        // VirtualKey.K - 100% match
        Key.K => 75,

        // VirtualKey.L - 100% match
        Key.L => 76,

        // VirtualKey.M - 100% match
        Key.M => 77,

        // VirtualKey.N - 100% match
        Key.N => 78,

        // VirtualKey.O - 100% match
        Key.O => 79,

        // VirtualKey.P - 100% match
        Key.P => 80,

        // VirtualKey.Q - 100% match
        Key.Q => 81,

        // VirtualKey.R - 100% match
        Key.R => 82,

        // VirtualKey.S - 100% match
        Key.S => 83,

        // VirtualKey.T - 100% match
        Key.T => 84,

        // VirtualKey.U - 100% match
        Key.U => 85,

        // VirtualKey.V - 100% match
        Key.V => 86,

        // VirtualKey.W - 100% match
        Key.W => 87,

        // VirtualKey.X - 100% match
        Key.X => 88,

        // VirtualKey.Y - 100% match
        Key.Y => 89,

        // VirtualKey.Z - 100% match
        Key.Z => 90,

        // VirtualKey.Sleep - 100% match
        Key.Sleep => 95,

        // VirtualKey.Multiply - 100% match
        Key.Multiply => 106,

        // VirtualKey.Add - 100% match
        Key.Add => 107,

        // VirtualKey.Separator - 100% match
        Key.Separator => 108,

        // VirtualKey.Subtract - 100% match
        Key.Subtract => 109,

        // VirtualKey.Decimal - 100% match
        Key.Decimal => 110,

        // VirtualKey.Divide - 100% match
        Key.Divide => 111,

        // VirtualKey.F1 - 100% match
        Key.F1 => 112,

        // VirtualKey.F2 - 100% match
        Key.F2 => 113,

        // VirtualKey.F3 - 100% match
        Key.F3 => 114,

        // VirtualKey.F4 - 100% match
        Key.F4 => 115,

        // VirtualKey.F5 - 100% match
        Key.F5 => 116,

        // VirtualKey.F6 - 100% match
        Key.F6 => 117,

        // VirtualKey.F7 - 100% match
        Key.F7 => 118,

        // VirtualKey.F8 - 100% match
        Key.F8 => 119,

        // VirtualKey.F9 - 100% match
        Key.F9 => 120,

        // VirtualKey.F10 - 100% match
        Key.F10 => 121,

        // VirtualKey.F11 - 100% match
        Key.F11 => 122,

        // VirtualKey.F12 - 100% match
        Key.F12 => 123,

        // VirtualKey.F13 - 100% match
        Key.F13 => 124,

        // VirtualKey.F14 - 100% match
        Key.F14 => 125,

        // VirtualKey.F15 - 100% match
        Key.F15 => 126,

        // VirtualKey.F16 - 100% match
        Key.F16 => 127,

        // VirtualKey.F17 - 100% match
        Key.F17 => 128,

        // VirtualKey.F18 - 100% match
        Key.F18 => 129,

        // VirtualKey.F19 - 100% match
        Key.F19 => 130,

        // VirtualKey.F20 - 100% match
        Key.F20 => 131,

        // VirtualKey.F21 - 100% match
        Key.F21 => 132,

        // VirtualKey.F22 - 100% match
        Key.F22 => 133,

        // VirtualKey.F23 - 100% match
        Key.F23 => 134,

        // VirtualKey.F24 - 100% match
        Key.F24 => 135,

        // VirtualKey.Scroll - 100% match
        Key.Scroll => 145,

        // VirtualKey.LeftShift - 100% match
        Key.LeftShift => 160,

        // VirtualKey.RightShift - 100% match
        Key.RightShift => 161,

        // VirtualKey.NonConvert - 87% match
        Key.ImeNonConvert => 29,

        // VirtualKey.ModeChange - 87% match
        Key.ImeModeChange => 31,

        // VirtualKey.RightControl - 86% match
        Key.RightCtrl => 163,

        // VirtualKey.LeftControl - 84% match
        Key.LeftCtrl => 162,

        // VirtualKey.Convert - 82% match
        Key.ImeConvert => 28,

        // VirtualKey.NumberPad0 - 82% match
        Key.NumPad0 => 96,

        // VirtualKey.NumberPad1 - 82% match
        Key.NumPad1 => 97,

        // VirtualKey.NumberPad2 - 82% match
        Key.NumPad2 => 98,

        // VirtualKey.NumberPad3 - 82% match
        Key.NumPad3 => 99,

        // VirtualKey.NumberPad4 - 82% match
        Key.NumPad4 => 100,

        // VirtualKey.NumberPad5 - 82% match
        Key.NumPad5 => 101,

        // VirtualKey.NumberPad6 - 82% match
        Key.NumPad6 => 102,

        // VirtualKey.NumberPad7 - 82% match
        Key.NumPad7 => 103,

        // VirtualKey.NumberPad8 - 82% match
        Key.NumPad8 => 104,

        // VirtualKey.NumberPad9 - 82% match
        Key.NumPad9 => 105,

        // VirtualKey.Accept - 80% match
        Key.ImeAccept => 30,

        // Matched manually
        Key.LaunchMail => 180,

        // Matched manually
        Key.MediaNextTrack => 176,

        // Matched manually
        Key.MediaPreviousTrack => 177,

        // Matched manually
        Key.MediaStop => 178,

        // Matched manually
        Key.MediaPlayPause => 179,

        // Matched manually
        Key.BrowserBack => 166,
        
        // Matched manually
        Key.BrowserForward => 167,
        
        // Matched manually
        Key.BrowserRefresh => 168,
        
        // Matched manually
        Key.BrowserStop => 169,
        
        // Matched manually
        Key.BrowserSearch => 170,
        
        // Matched manually
        Key.BrowserFavorites => 171,
        
        // Matched manually
        Key.BrowserHome => 172,

        // Matched manually
        Key.SelectMedia => 181,

        // Matched manually
        Key.LaunchApplication1 => 182,

        // Matched manually
        Key.LaunchApplication2 => 183,

        // VirtualKey.Hangul - 75% match
        Key.HangulMode => 21,

        // VirtualKey.CapitalLock - 74% match
        Key.CapsLock => 20,

        // VirtualKey.Junja - 71% match
        Key.JunjaMode => 23,

        // VirtualKey.Final - 71% match
        Key.FinalMode => 24,

        // VirtualKey.Hanja - 71% match
        Key.HanjaMode => 25,

        // VirtualKey.NumberKeyLock - 70% match
        Key.NumLock => 144,

        // Matched manually
        Key.D0 => 48,

        // Matched manually
        Key.D1 => 49,

        // Matched manually
        Key.D2 => 50,

        // Matched manually
        Key.D3 => 51,

        // Matched manually
        Key.D4 => 52,

        // Matched manually
        Key.D5 => 53,

        // Matched manually
        Key.D6 => 54,

        // Matched manually
        Key.D7 => 55,

        // Matched manually
        Key.D8 => 56,

        // Matched manually
        Key.D9 => 57,

        // Matched manually
        Key.LeftAlt => 164,

        // Matched manually
        Key.RightAlt => 165,

        // Matched manually
        Key.Zoom => 251,

        // Matched manually
        Key.RWin => 92,

        // Matched manually
        Key.LWin => 91,

        // Matched manually
        Key.PrintScreen => 44,

        // Matched manually
        Key.Return => 13,

        // Matched manually
        Key.OemPlus => 187,

        // Matched manually
        Key.OemMinus => 189,

        // Matched manually
        Key.OemComma => 188,

        // Matched manually
        Key.OemPeriod => 190,

        // Matched manually
        Key.OemClear => 254,

        // Matched manually
        Key.Oem1 => 186,
        
        // Matched manually
        Key.Oem2 => 191,
        
        // Matched manually
        Key.Oem3 => 192,
        
        // Matched manually
        Key.Oem4 => 219,
        
        // Matched manually
        Key.Oem5 => 220,
        
        // Matched manually
        Key.Oem6 => 221,
        
        // Matched manually
        Key.Oem7 => 222,
        
        // Matched manually
        Key.Oem8 => 223,
        
        // Matched manually
        Key.Oem102 => 226,

        // Matched manually
        Key.NoName => 252,

        // Matched manually
        Key.VolumeMute => 173,
        
        // Matched manually
        Key.VolumeUp => 175,
        
        // Matched manually
        Key.VolumeDown => 174,

        // Matched manually
        Key.Apps => 93,

        _ => throw new NotSupportedException()
    };
}