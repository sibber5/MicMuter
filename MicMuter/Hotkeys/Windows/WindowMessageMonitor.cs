using System;
using System.Runtime.InteropServices;
using System.Threading;
using MicMuter.Native.Win32;

namespace MicMuter.Hotkeys.Windows;

// From https://github.com/dotMorten/WinUIEx by dotMorten
// MIT License
// 
// Copyright(c) 2021 Morten Nielsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

/// <summary>
/// The message monitor allows you to monitor/listen to window message events for a given window.
/// </summary>
internal sealed class WindowMessageMonitor : IDisposable
{
    /// <summary>
    /// The handle of the window that is being monitored.
    /// </summary>
    public nint Hwnd { get; }

    private GCHandle? _monitorGCHandle;
    private readonly Lock _lockObject = new();

    private static nuint _classIdCounter = 101;
    private readonly nuint _classId;

    private bool _disposed;

    /// <summary>
    /// Initialize a new instance of the <see cref="WindowMessageMonitor"/> class.
    /// </summary>
    /// <param name="hwnd">The window handle to listen to messages for</param>
    public WindowMessageMonitor(nint hwnd)
    {
        Hwnd = hwnd;
        _classId = _classIdCounter++;
    }

    ~WindowMessageMonitor()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (_windowMessageReceived != null)
        {
            RemoveWindowSubclass();
            _windowMessageReceived = null;
        }

        _disposed = true;
    }

    /// <summary>
    /// Event raised when a windows message is received.
    /// </summary>
    public event EventHandler<WindowMessageEventArgs> WindowMessageReceived
    {
        add
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_windowMessageReceived is null) SetWindowSubclass();
            _windowMessageReceived += value;
        }
        remove
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            _windowMessageReceived -= value;
            if (_windowMessageReceived is null) RemoveWindowSubclass();
        }
    }

    private event EventHandler<WindowMessageEventArgs>? _windowMessageReceived;


    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static nint NewWindowProc(nint hWnd, uint uMsg, nuint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData)
    {
        var handle = GCHandle.FromIntPtr((nint)dwRefData);
        if (handle.IsAllocated && handle.Target is WindowMessageMonitor monitor)
        {
            var handler = monitor._windowMessageReceived;
            if (handler != null)
            {
                var args = new WindowMessageEventArgs(hWnd, uMsg, wParam, lParam);
                handler.Invoke(monitor, args);
                if (args.Handled) return args.Result;
            }
        }

        return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    private unsafe void SetWindowSubclass()
    {
        lock (_lockObject)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_monitorGCHandle.HasValue) return;
            
            _monitorGCHandle = GCHandle.Alloc(this);
            bool ok = PInvoke.SetWindowSubclass(Hwnd, &NewWindowProc, _classId, (nuint)GCHandle.ToIntPtr(_monitorGCHandle.Value).ToPointer());
            if (!ok) throw new Exception("Error setting window subclass.");
        }
    }

    private unsafe void RemoveWindowSubclass()
    {
        lock (_lockObject)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_monitorGCHandle.HasValue) return;
            
            bool ok = PInvoke.RemoveWindowSubclass(Hwnd, &NewWindowProc, _classId);
            if (!ok) throw new Exception("Error removing window subclass.");

            _monitorGCHandle?.Free();
            _monitorGCHandle = null;
        }
    }
}

/// <summary>
/// Event arguments for the <see cref="WindowMessageMonitor.WindowMessageReceived"/> event.
/// </summary>
internal sealed class WindowMessageEventArgs : EventArgs
{
    internal WindowMessageEventArgs(nint hwnd, uint messageId, nuint wParam, nint lParam)
    {
        Message = new(hwnd, messageId, wParam, lParam, default, default);
    }

    /// <summary>
    /// The result after processing the message. Use this to set the return result, after also setting <see cref="Handled"/> to <see langword="true"/>.
    /// </summary>
    /// <seealso cref="Handled"/>
    public nint Result { get; set; }

    /// <summary>
    /// Indicates whether this message was handled and the <see cref="Result"/> value should be returned.
    /// </summary>
    /// <remarks><see langword="true"/> is the message was handled and the <see cref="Result"/> should be returned, otherwise <see langword="false"/> and continue processing this message by other subsclasses.</remarks>
    /// <seealso cref="Result"/>
    public bool Handled { get; set; }

    /// <summary>
    /// The Windows WM Message
    /// </summary>
    public MSG Message { get; }
}

internal static partial class PInvoke
{
    [DllImport("COMCTL32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern nint DefSubclassProc(nint hWnd, uint uMsg, nuint wParam, nint lParam);
    
    [LibraryImport("COMCTL32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool SetWindowSubclass(nint hWnd, delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nuint, nuint, nint> pfnSubclass, nuint uIdSubclass, nuint dwRefData);

    [LibraryImport("COMCTL32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool RemoveWindowSubclass(nint hWnd, delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nuint, nuint, nint> pfnSubclass, nuint uIdSubclass);
}
