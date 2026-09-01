# VoiceTranslate — Implementation Plan

Türkçe sesli girişi gerçek zamanlı Bulgarcaya çeviren, Windows sistem tepsisinde çalışan masaüstü uygulaması.

## Proje Amacı

Kullanıcı `Ctrl+Shift+B` kısayolu ile mikrofonu aktif eder, Türkçe konuşur, uygulama sesi doğrudan Gemini API ile Bulgarcaya çevirir ve sonucu aktif pencereye otomatik yapıştırır.

## Tasarım Kararları

| Konu | Karar | Gerekçe |
|---|---|---|
| **Yazılım Dili** | C# (.NET 8) | Native Windows exe, runtime gerektirmez, system tray çok temiz |
| **Çeviri & STT** | Gemini API (`gemini-3.5-flash-lite`) | Tek adımda ses -> metin çevirisi. Hızlı, düşük maliyetli ve mimariyi basitleştiriyor. |
| **Arayüz** | System Tray (NotifyIcon) | Arka planda minimal çalışır, kullanışlı |
| **Tetikleyici** | `Ctrl+Shift+B` global hotkey | Basılı tut = kayıt, bırak = işle (Win+B çakışmasını önlemek için değiştirildi) |
| **Çıktı** | Pano Yedeklemeli Yapıştırma | Kullanıcının panodaki eski kopyaladığı veriyi silmeden yapıştırma işlemi yapar. |
| **Tuş Simülasyonu** | InputSimulatorPlus | SendKeys'e göre daha güvenilir ve stabil. |
| **Yapılandırma** | `appsettings.json` | API keyler ve kullanıcı tercihleri |

## Kullanıcı Akışı

```
[Ctrl+Shift+B basılı tut] → 🔴 Kayıt başlar (tray ikonu kırmızı olur + ses tonu)
[Türkçe konuş]
[Ctrl+Shift+B bırak]     → ⏳ İşlem başlar (tray ikonu döner)
                         → Gemini API: WAV Ses → Bulgarca metin
                         → Panodaki eski veri yedeklenir.
                         → ✅ Sonuç clipboard'a kopyalanır
                         → InputSimulatorPlus ile Ctrl+V yollanır
                         → Eski pano verisi geri yüklenir.
                         → Tray baloncuğu: "Yapıştırıldı: [kısa önizleme]"
```

## Mimari

```
VoiceTranslate.exe (.NET 8 Windows App)
│
├── Core/
│   ├── HotkeyManager.cs        # Ctrl+Shift+B global hook (User32.dll P/Invoke)
│   ├── AudioRecorder.cs        # NAudio ile mikrofon kaydı (WAV/PCM)
│   ├── TranslationService.cs   # Gemini API entegrasyonu (Ses -> Metin)
│   └── PasteManager.cs         # InputSimulatorPlus, Pano Yönetimi
│
├── UI/
│   ├── TrayIcon.cs             # NotifyIcon, context menu, balon bildirimleri
│   └── SettingsForm.cs         # API key, kısayol ayarları
│
├── Config/
│   ├── AppSettings.cs          # Settings modeli
│   └── appsettings.json        # API keyler ve kullanıcı tercihleri (gitignore'da)
│
└── Program.cs                  # Entry point, ApplicationContext
```

## Bağımlılıklar (NuGet)

| Paket | Amaç |
|---|---|
| `NAudio` | Mikrofon kaydı (Windows native) |
| `InputSimulatorPlus` | Kararlı tuş simülasyonu (Ctrl+V) |
| `Microsoft.Extensions.Configuration` | appsettings.json okuma |
| `System.Text.Json` | Gemini API yanıt parse (built-in) |

## Geliştirme Aşamaları

### P0 — Proje İskeleti
- [x] `VoiceTranslate` .NET 8 Windows Forms projesi oluştur
- [x] NuGet bağımlılıklarını ekle
- [x] `appsettings.json` yapısını kur, `.gitignore`'a ekle
- [x] `Program.cs` entry point + `ApplicationContext` kur
- [x] Temel `TrayIcon.cs` (ikon, "Çık" menüsü)

### P1 — Global Hotkey & Ses Kaydı
- [ ] `HotkeyManager.cs`: `Ctrl+Shift+B` için `RegisterHotKey` (User32 P/Invoke)
- [ ] Basılı tut / bırak mantığı
- [ ] `AudioRecorder.cs`: NAudio ile PCM 16kHz 16-bit mono kayıt
- [ ] Tray ikonu durumu: gri → kırmızı (kayıt) → sarı (işleniyor) → yeşil (tamam)

### P2 — Gemini API ile Sesli Çeviri
- [ ] `TranslationService.cs`: WAV byte[] → Bulgarca metin (Doğrudan ses dosyası gönderimi)
- [ ] Gemini `generateContent` endpoint (`gemini-3.5-flash-lite`)
- [ ] Prompt: "Translate this Turkish audio to Bulgarian text. Return only the translated text."
- [ ] Hata yönetimi: rate limit, timeout, geçersiz yanıt

### P3 — Yapıştırma & Bildirim (Pano Korumalı)
- [ ] `PasteManager.cs`: Mevcut panoyu yedekle
- [ ] Sonucu clipboard'a yaz, `InputSimulatorPlus` ile `Ctrl+V` gönder
- [ ] Focus koruması: yapıştırmadan önce kısa bir bekleme
- [ ] Panodaki eski veriyi geri yükle
- [ ] Tray balon bildirimi ve hata yönetimi

### P4 — Ayarlar & Paketleme
- [ ] `SettingsForm.cs`: API key ayarı
- [ ] Uygulama başlangıcıyla otomatik çalışma (Registry Run key)
- [ ] `dotnet publish` ile tek `.exe` (self-contained, Windows x64)

## API Konfigürasyonu

### Gemini API
- `appsettings.json`'da `GeminiApiKey` alanına yaz
- Model: `gemini-3.5-flash-lite`

## appsettings.json Şablonu

```json
{
  "GeminiApiKey": "YOUR_GEMINI_API_KEY_HERE",
  "PasteDelayMs": 300
}
```
