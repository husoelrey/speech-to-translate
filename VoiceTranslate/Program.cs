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
        var translationService = new TranslationService(appSettings.GeminiApiKey);

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
                
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        string translatedText = await translationService.TranslateAudioAsync(wavBytes);
                        
                        // TODO: P3 - Paste text using PasteManager
                        // For now, simulate success by showing a MessageBox and resetting state
                        trayIcon.SetState(TrayState.Success);
                        
                        if (Application.OpenForms.Count == 0 && trayIcon != null)
                        {
                            // Back to idle after a short delay
                            await System.Threading.Tasks.Task.Delay(2000);
                            trayIcon.SetState(TrayState.Idle);
                        }
                    }
                    catch (Exception ex)
                    {
                        trayIcon.SetState(TrayState.Error);
                        MessageBox.Show($"Translation failed: {ex.Message}", "VoiceTranslate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
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