using System;
using System.Drawing;
using System.Windows.Forms;

namespace VoiceTranslate.UI;

public class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;

    public TrayIcon()
    {
        _contextMenu = new ContextMenuStrip();
        
        var titleItem = new ToolStripMenuItem("VoiceTranslate");
        titleItem.Enabled = false;
        
        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Enabled = false; // Grayed out for now
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => Application.Exit();

        _contextMenu.Items.Add(titleItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(settingsItem);
        _contextMenu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = GeneratePlaceholderIcon(),
            ContextMenuStrip = _contextMenu,
            Visible = true,
            Text = "VoiceTranslate"
        };
    }

    private Icon GeneratePlaceholderIcon()
    {
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Gray);
        return Icon.FromHandle(bitmap.GetHicon());
    }

    public void Dispose()
    {
        _notifyIcon?.Dispose();
        _contextMenu?.Dispose();
    }
}
