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

        using var trayIcon = new TrayIcon(() => 
        {
            using var settingsForm = new SettingsForm(appSettings);
            settingsForm.ShowDialog();
            
            // Reload settings
            configuration.Reload();
            configuration.Bind(appSettings);
        });
        using var audioRecorder = new AudioRecorder();
        using var hotkeyManager = new HotkeyManager();
        var translationService = new TranslationService(appSettings);

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
                        
                        await PasteManager.PasteAsync(translatedText, appSettings.PasteDelayMs);
                        
                        trayIcon.SetState(TrayState.Success);
                        
                        string previewText = translatedText.Length > 30 ? translatedText.Substring(0, 27) + "..." : translatedText;
                        trayIcon.ShowSuccess($"Pasted: {previewText}");
                        
                        if (Application.OpenForms.Count == 0 && trayIcon != null)
                        {
                            await System.Threading.Tasks.Task.Delay(2000);
                            trayIcon.SetState(TrayState.Idle);
                        }
                    }
                    catch (Exception ex)
                    {
                        trayIcon.SetState(TrayState.Error);
                        trayIcon.ShowError(ex.Message);
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