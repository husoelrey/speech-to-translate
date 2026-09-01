using System;
using System.Drawing;
using System.Windows.Forms;

namespace VoiceTranslate.UI;

public enum TrayState
{
    Idle,
    Recording,
    Processing,
    Success,
    Error
}

public class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private TrayState _currentState;

    public TrayIcon(Action onSettingsClicked)
    {
        _contextMenu = new ContextMenuStrip();
        
        var titleItem = new ToolStripMenuItem("VoiceTranslate");
        titleItem.Enabled = false;
        
        var settingsItem = new ToolStripMenuItem("Settings...");
        settingsItem.Click += (s, e) => onSettingsClicked();
        
        var startupItem = new ToolStripMenuItem("Run at Windows Startup");
        startupItem.CheckOnClick = true;
        startupItem.Checked = IsStartupEnabled();
        startupItem.CheckedChanged += (s, e) => SetStartup(startupItem.Checked);
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => Application.Exit();

        _contextMenu.Items.Add(titleItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(settingsItem);
        _contextMenu.Items.Add(startupItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = _contextMenu,
            Visible = true,
            Text = "VoiceTranslate - Idle"
        };
        
        SetState(TrayState.Idle);
    }

    private const string RegistryRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "VoiceTranslate";

    private bool IsStartupEnabled()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryRunKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch { return false; }
    }

    private void SetStartup(bool enable)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryRunKey, true);
            if (key != null)
            {
                if (enable)
                {
                    string appPath = Application.ExecutablePath;
                    key.SetValue(AppName, $"\"{appPath}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not update startup settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void SetState(TrayState state)
    {
        _currentState = state;
        
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        
        switch (state)
        {
            case TrayState.Idle:
                graphics.Clear(Color.Gray);
                _notifyIcon.Text = "VoiceTranslate - Idle";
                break;
            case TrayState.Recording:
                graphics.Clear(Color.Red);
                _notifyIcon.Text = "VoiceTranslate - Recording (Press Ctrl+Shift+B to stop)";
                break;
            case TrayState.Processing:
                graphics.Clear(Color.Gold);
                _notifyIcon.Text = "VoiceTranslate - Processing...";
                break;
            case TrayState.Success:
                graphics.Clear(Color.LimeGreen);
                _notifyIcon.Text = "VoiceTranslate - Success";
                break;
            case TrayState.Error:
                graphics.Clear(Color.White);
                using (var pen = new Pen(Color.Red, 2))
                {
                    graphics.DrawLine(pen, 2, 2, 14, 14);
                    graphics.DrawLine(pen, 14, 2, 2, 14);
                }
                _notifyIcon.Text = "VoiceTranslate - Error";
                break;
        }

        var oldIcon = _notifyIcon.Icon;
        _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
        
        if (oldIcon != null)
        {
            // Clean up the old icon to prevent GDI leak
            NativeMethods.DestroyIcon(oldIcon.Handle);
            oldIcon.Dispose();
        }
    }

    public void ShowSuccess(string message)
    {
        _notifyIcon.ShowBalloonTip(3000, "VoiceTranslate", message, ToolTipIcon.Info);
    }

    public void ShowError(string message)
    {
        _notifyIcon.ShowBalloonTip(5000, "VoiceTranslate Error", message, ToolTipIcon.Error);
    }

    public void Dispose()
    {
        if (_notifyIcon.Icon != null)
        {
            NativeMethods.DestroyIcon(_notifyIcon.Icon.Handle);
            _notifyIcon.Icon.Dispose();
        }
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
    }
    
    private static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr handle);
    }
}
