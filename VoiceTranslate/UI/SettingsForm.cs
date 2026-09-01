using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using VoiceTranslate.Config;

namespace VoiceTranslate.UI;

public partial class SettingsForm : Form
{
    private TextBox _apiKeyTextBox = null!;
    private NumericUpDown _delayNumeric = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;

    public SettingsForm(AppSettings currentSettings)
    {
        InitializeComponent();
        
        _apiKeyTextBox.Text = currentSettings.GeminiApiKey;
        _delayNumeric.Value = currentSettings.PasteDelayMs;
    }
    
    private void InitializeComponent()
    {
        this.Text = "VoiceTranslate Settings";
        this.Size = new Size(400, 200);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        Label apiLabel = new Label { Text = "Gemini API Key:", Left = 20, Top = 20, Width = 100 };
        _apiKeyTextBox = new TextBox { Left = 120, Top = 20, Width = 240 };

        Label delayLabel = new Label { Text = "Paste Delay (ms):", Left = 20, Top = 60, Width = 100 };
        _delayNumeric = new NumericUpDown { Left = 120, Top = 60, Width = 100, Minimum = 0, Maximum = 5000 };

        _saveButton = new Button { Text = "Save", Left = 120, Top = 100, Width = 80 };
        _saveButton.Click += SaveButton_Click;

        _cancelButton = new Button { Text = "Cancel", Left = 210, Top = 100, Width = 80 };
        _cancelButton.Click += (s, e) => this.Close();

        this.Controls.Add(apiLabel);
        this.Controls.Add(_apiKeyTextBox);
        this.Controls.Add(delayLabel);
        this.Controls.Add(_delayNumeric);
        this.Controls.Add(_saveButton);
        this.Controls.Add(_cancelButton);
        
        this.AcceptButton = _saveButton;
        this.CancelButton = _cancelButton;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        var newSettings = new AppSettings
        {
            GeminiApiKey = _apiKeyTextBox.Text.Trim(),
            PasteDelayMs = (int)_delayNumeric.Value
        };

        try
        {
            string json = JsonSerializer.Serialize(newSettings, new JsonSerializerOptions { WriteIndented = true });
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            File.WriteAllText(configPath, json);

            MessageBox.Show("Settings saved. Please restart the application for changes to fully apply.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
