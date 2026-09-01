using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using VoiceTranslate.UI;
using VoiceTranslate.Config;
using VoiceTranslate.Core;

namespace VoiceTranslate;

static class Program
{
    private static Mutex? mutex = null;

    [STAThread]
    static void Main()
    {
        const string appName = "VoiceTranslateAppMutex";
        bool createdNew;

        mutex = new Mutex(true, appName, out createdNew);

        if (!createdNew)
        {
            MessageBox.Show("VoiceTranslate is already running.", "VoiceTranslate", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = builder.Build();

        var appSettings = new AppSettings();
        configuration.Bind(appSettings);

        using var trayIcon = new TrayIcon();
        using var audioRecorder = new AudioRecorder();
        using var hotkeyManager = new HotkeyManager();

        hotkeyManager.RecordingStarted += (s, e) =>
        {
            trayIcon.SetState(TrayState.Recording);
            try
            {
                audioRecorder.StartRecording();
            }
            catch (Exception ex)
            {
                trayIcon.SetState(TrayState.Error);
                MessageBox.Show($"Failed to start recording: {ex.Message}", "VoiceTranslate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        hotkeyManager.RecordingStopped += (s, e) =>
        {
            trayIcon.SetState(TrayState.Processing);
            try
            {
                byte[] wavBytes = audioRecorder.StopRecording();
                // TODO: P2 - send to Gemini API
                // For now, simulate success after a delay
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                    // P2 will replace this
                    // For now, just reset to Idle
                    if (Application.OpenForms.Count == 0 && trayIcon != null)
                    {
                        // Needs to be on UI thread or trayIcon needs to handle Invoke, 
                        // but TrayIcon icon updates don't strictly require UI thread, though it's safer.
                    }
                });
                
                // P1 just stops recording and sets state to Processing.
            }
            catch (Exception ex)
            {
                trayIcon.SetState(TrayState.Error);
                MessageBox.Show($"Failed to stop recording: {ex.Message}", "VoiceTranslate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        Application.Run(new ApplicationContext());
    }
}