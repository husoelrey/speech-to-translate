# VoiceTranslate — Agent Prompts (P0 → P4)

> **Model Recommendation:**
> - **P0 → Flash High** (boilerplate, project setup — simple structural work)
> - **P1 → Pro High** (Win32 P/Invoke, NAudio, low-level Windows APIs — needs deep reasoning)
> - **P2 → Pro High** (Gemini Multimodal API with Audio, HTTP client + JSON parsing)
> - **P3 → Flash High** (clipboard backup/restore + InputSimulatorPlus + tray notifications — moderate)
> - **P4 → Flash High** (WinForms settings form + publish config — UI boilerplate)

---

## P0 — Project Skeleton

> 🤖 **Recommended Model: Flash High**

### Prompt

You are an expert C# .NET 8 developer building a Windows system tray application called **VoiceTranslate**.

**Context:**
The project lives at `C:\Users\husoelrey\Desktop\geminicoder\tr_to_bulgarian`. The repository already contains:
- `PLAN.md` — development phases and task checklist
- `ARCHITECTURE.md` — component diagram and design decisions
- `README.md` — user-facing documentation

**Your task: Implement P0 — Project Skeleton**

Create the complete Visual Studio / dotnet project structure inside `C:\Users\husoelrey\Desktop\geminicoder\tr_to_bulgarian\VoiceTranslate\`.

#### Requirements

1. **Project type:** `.NET 8` Windows Forms Application
   - Target framework: `net8.0-windows`
   - Output type: `WinExe` (no console window)

2. **Create `VoiceTranslate.csproj`** with these NuGet packages:
   - `NAudio` (latest stable) — microphone recording
   - `InputSimulatorPlus` (latest stable) — for reliable Ctrl+V keystrokes
   - `Microsoft.Extensions.Configuration` (latest stable)
   - `Microsoft.Extensions.Configuration.Json` (latest stable)

3. **Folder structure to create:**
   ```
   VoiceTranslate/
   ├── VoiceTranslate.csproj
   ├── Program.cs
   ├── Core/
   │   ├── HotkeyManager.cs        (stub)
   │   ├── AudioRecorder.cs        (stub)
   │   ├── TranslationService.cs   (stub)
   │   └── PasteManager.cs         (stub)
   ├── UI/
   │   ├── TrayIcon.cs             (stub)
   │   └── SettingsForm.cs         (stub)
   └── Config/
       └── AppSettings.cs          (fully implemented)
   ```

4. **`Config/AppSettings.cs`**:
   ```csharp
   public class AppSettings
   {
       public string GeminiApiKey { get; set; } = string.Empty;
       public int PasteDelayMs { get; set; } = 300;
   }
   ```

5. **`Program.cs`** — Entry point:
   - Single-instance enforcement using `Mutex`
   - Load `appsettings.json` from the executable directory using `Microsoft.Extensions.Configuration`
   - Start `Application.Run(new ApplicationContext())`

6. **`UI/TrayIcon.cs`** — Implement this fully as a working skeleton:
   - Uses `NotifyIcon` with a generated 16x16 gray placeholder icon (use `System.Drawing.Bitmap`)
   - Right-click context menu with items: `"VoiceTranslate"`, separator, `"Settings"` (grayed out for now), `"Exit"`
   - Exit closes the application cleanly

7. **Verify the project builds** by running `dotnet build` from the `VoiceTranslate/` folder.

8. **Update `PLAN.md`**: mark all P0 checkboxes as complete (`[x]`).

---

## P1 — Global Hotkey & Audio Recording

> 🤖 **Recommended Model: Pro High**

### Prompt

You are an expert C# .NET 8 developer.

**Your task: Implement P1 — Global Hotkey & Audio Recording**

#### 1. `Core/HotkeyManager.cs`
Implement a global hotkey listener for **Ctrl+Shift+B** using Win32 `RegisterHotKey` / `UnregisterHotKey` via P/Invoke.
- Use `NativeWindow` subclass to handle `WM_HOTKEY`
- Register `MOD_CONTROL | MOD_SHIFT` + `VK_B`
- **Hold-to-record behavior:** toggle on each hotkey message. First press fires `RecordingStarted`, second press fires `RecordingStopped`.
- Thread-safe: raise events on the UI thread using `SynchronizationContext`

#### 2. `Core/AudioRecorder.cs`
Use `NAudio.Wave.WaveInEvent` to capture microphone audio.
- Audio format: **16000 Hz, 16-bit, Mono**
- `StartRecording()`: opens default microphone, begins capture to internal `MemoryStream`
- `StopRecording()`: stops capture, returns `byte[]` containing a valid WAV file with RIFF header.

#### 3. `UI/TrayIcon.cs`
Add icon state management:
- Define enum: `TrayState { Idle, Recording, Processing, Success, Error }`
- `SetState(TrayState state)` method that changes the tray icon color and tooltip (Gray, Red, Yellow, Green, Red X).

#### 4. Wire Everything in `Program.cs`
- Instantiate `HotkeyManager` and `AudioRecorder`
- On `RecordingStarted`: call `AudioRecorder.StartRecording()`, set tray state to `Recording`
- On `RecordingStopped`: call `AudioRecorder.StopRecording()`, set tray state to `Processing`

#### 5. Update `PLAN.md`
Mark all P1 checkboxes as complete (`[x]`).

---

## P2 — Gemini API Audio Translation

> 🤖 **Recommended Model: Pro High**

### Prompt

You are an expert C# .NET 8 developer.

**Your task: Implement P2 — Gemini API Audio Translation**

We will send the recorded WAV audio directly to the Gemini API (`gemini-3.5-flash-lite`) to transcribe and translate it in one step.

#### 1. `Core/TranslationService.cs`
Use plain `HttpClient` + `System.Text.Json`.
```csharp
public class TranslationService
{
    public async Task<string> TranslateAudioAsync(byte[] wavBytes);
}
```
Requirements:
- Base URL: `https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent`
- API key passed as query parameter
- HTTP POST with JSON body containing the inline audio data (Base64 encoded):
  ```json
  {
    "contents": [
      {
        "parts": [
          { "text": "Translate this Turkish audio to Bulgarian text. Return only the translated text." },
          {
            "inline_data": {
              "mime_type": "audio/wav",
              "data": "<BASE64_ENCODED_WAV_BYTES>"
            }
          }
        ]
      }
    ]
  }
  ```
- Parse response and return the text.

#### 2. Wire into `Program.cs`
- After `AudioRecorder.StopRecording()` returns WAV bytes:
  - Call `await translationService.TranslateAudioAsync(wavBytes)`
  - Store the Bulgarian text result

#### 3. Update `PLAN.md`
Mark all P2 checkboxes as complete (`[x]`).

---

## P3 — PasteManager (Clipboard & InputSimulatorPlus)

> 🤖 **Recommended Model: Flash High**

### Prompt

You are an expert C# .NET 8 developer.

**Your task: Implement P3 — PasteManager & Notifications**

#### 1. `Core/PasteManager.cs`
Use `InputSimulatorPlus` for keystrokes.
```csharp
public static async Task PasteAsync(string text, int delayMs);
```
Requirements:
- Read current clipboard text using `Clipboard.GetText()` and store it as a backup.
- Set new text: `Clipboard.SetText(text)`
- `await Task.Delay(delayMs)`
- Use `InputSimulatorPlus` to simulate `Ctrl+V` (e.g., `new InputSimulator().Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V)`)
- `await Task.Delay(100)`
- Restore clipboard: `Clipboard.SetText(backupText)`
- Must run on STA thread / UI thread appropriately for Clipboard access.

#### 2. `UI/TrayIcon.cs`
Add balloon notifications (`ShowSuccess`, `ShowError`).

#### 3. Wire in `Program.cs`
Connect the translation result to `PasteManager.PasteAsync`. Ensure UI thread synchronization.

#### 4. Update `PLAN.md`
Mark all P3 checkboxes as complete (`[x]`).

---

## P4 — Settings Form & Packaging

> 🤖 **Recommended Model: Flash High**

### Prompt

You are an expert C# .NET 8 developer.

**Your task: Implement P4 — Settings Form & Packaging**

#### 1. `UI/SettingsForm.cs`
Create a simple WinForms `Form`:
- TextBox: "Gemini API Key"
- NumericUpDown: "Paste Delay (ms)"
- Save values to `appsettings.json`.

#### 2. Windows Startup
Add a toggle to the tray menu: `"Run at Windows Startup"` using the Registry Run key.

#### 3. Update `PLAN.md`
Mark all P4 checkboxes as complete (`[x]`).
