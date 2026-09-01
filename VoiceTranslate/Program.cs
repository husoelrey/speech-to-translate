using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using VoiceTranslate.UI;
using VoiceTranslate.Config;

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
        Application.Run(new ApplicationContext());
    }
}