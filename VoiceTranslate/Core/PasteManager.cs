using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace VoiceTranslate.Core;

public static class PasteManager
{
    public static async Task PasteAsync(string text, int delayMs)
    {
        if (string.IsNullOrEmpty(text))
            return;

        string backupText = string.Empty;

        // Clipboard operations must run on STA thread
        Thread staThread = new Thread(() =>
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    backupText = Clipboard.GetText();
                }
                Clipboard.SetText(text);
            }
            catch { /* Ignore clipboard errors */ }
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();

        await Task.Delay(delayMs);

        var simulator = new InputSimulator();
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

        await Task.Delay(100);

        if (!string.IsNullOrEmpty(backupText))
        {
            Thread restoreThread = new Thread(() =>
            {
                try
                {
                    Clipboard.SetText(backupText);
                }
                catch { /* Ignore clipboard errors */ }
            });
            restoreThread.SetApartmentState(ApartmentState.STA);
            restoreThread.Start();
            restoreThread.Join();
        }
    }
}
