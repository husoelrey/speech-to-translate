using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VoiceTranslate.Core;

public class HotkeyManager : IDisposable
{
    public event EventHandler? RecordingStarted;
    public event EventHandler? RecordingStopped;

    private readonly HotkeyWindow _window;
    private readonly SynchronizationContext? _syncContext;
    private bool _isRecording = false;

    public HotkeyManager()
    {
        _syncContext = SynchronizationContext.Current;
        _window = new HotkeyWindow();
        _window.HotkeyPressed += OnHotkeyPressed;
        _window.Register(1 /* id */, 0x0002 | 0x0004 /* MOD_CONTROL | MOD_SHIFT */, 0x42 /* VK_B */);
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        _isRecording = !_isRecording;
        var eventToRaise = _isRecording ? RecordingStarted : RecordingStopped;

        if (_syncContext != null)
        {
            _syncContext.Post(_ => eventToRaise?.Invoke(this, EventArgs.Empty), null);
        }
        else
        {
            eventToRaise?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        _window?.Dispose();
    }

    private class HotkeyWindow : NativeWindow, IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private int _id;

        public event EventHandler? HotkeyPressed;

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
        }

        public void Register(int id, uint modifiers, uint key)
        {
            _id = id;
            RegisterHotKey(Handle, id, modifiers, key);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            UnregisterHotKey(Handle, _id);
            DestroyHandle();
        }
    }
}
