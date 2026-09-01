# VoiceTranslate — Architecture

## Genel Bakış

```
[Kullanıcı] --Ctrl+Shift+B--> [HotkeyManager] --> [AudioRecorder] --> [WAV buffer]
                                                                        |
                                                             [TranslationService]
                                                              Gemini API (flash-lite)
                                                                (Ses -> Metin)
                                                                        |
                                                             Bulgarca metin string
                                                                        |
                                                               [PasteManager]
                                                      Pano Yedekleme + InputSimulatorPlus
                                                                        |
                                                             [TrayIcon] bildirim
```

## Bileşenler

### HotkeyManager
- `RegisterHotKey` ile global Ctrl+Shift+B hook kaydeder
- WndProc mesajlarını dinler: `WM_HOTKEY`
- `OnKeyDown` ve `OnKeyUp` eventleri fırlatır
- Uygulama kapanırken `UnregisterHotKey` çağırır

### AudioRecorder
- `NAudio.Wave.WaveInEvent` ile mikrofon açar
- Format: **16kHz, 16-bit, Mono**
- `MemoryStream`'e PCM veri yazar
- `StopRecording()` → WAV header eklenmiş byte[] döner

### TranslationService
- Gemini REST API (`HttpClient`)
- Model: `gemini-3.5-flash-lite` (hızlı, uygun maliyetli, ses girdisi alabilir)
- Ses dosyasını (WAV) doğrudan API'ye gönderir.
- Prompt: "Translate this Turkish audio to Bulgarian text. Return only the translated text."
- Yanıt parse edilir ve çevrilmiş metin döner.

### PasteManager
- `Clipboard.GetText()` ile eski veriyi yedekle.
- `Clipboard.SetText(result)` ile çeviriyi panoya yaz.
- `await Task.Delay(PasteDelayMs)` (default 300ms, focus kaybı için bekleme)
- `InputSimulatorPlus` ile `Ctrl+V` tuş kombinasyonunu daha kararlı bir şekilde yolla.
- Kısa bir süre bekleyip, eski kopyalanan veriyi tekrar panoya geri yükle.

### TrayIcon (UI)
- `NotifyIcon` ile system tray ikonu
- İkon durumları: Idle / Recording / Processing / Success / Error
- Sağ tık menüsü: Ayarlar | Çık
- Balon bildirimi: çeviri özeti veya hata mesajı

## Durum Makinesi

```
IDLE ──[Ctrl+Shift+B down]──> RECORDING
RECORDING ──[Ctrl+Shift+B up]──> PROCESSING
PROCESSING ──[başarı]──> IDLE (yeşil flash)
PROCESSING ──[hata]──> IDLE (kırmızı flash + bildirim)
RECORDING ──[Ctrl+Shift+B down tekrar]──> IDLE (iptal)
```

## Hata Yönetimi

| Hata | Davranış |
|---|---|
| Mikrofon erişim hatası | Tray bildirimi, kayıt iptal |
| Boş ses / tanınamadı | "Ses anlaşılamadı" bildirimi |
| Gemini API hatası | Retry yok, hata bildirimi |
| Clipboard erişim hatası | Sessiz başarısızlık, log |

## Güvenlik

- API keyler `appsettings.json`'da saklanır, `.gitignore`'a eklenir
- İleride DPAPI ile encrypted storage eklenebilir
- Ses verisi bellekte işlenir, diske yazılmaz

## Genişletilebilirlik

- Kısayol değiştirme: `appsettings.json`'da `Hotkey` alanı (P4'te)
