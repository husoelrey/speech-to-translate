# VoiceTranslate

**Ctrl+Shift+B** kısayolu ile Türkçe sesli girişi doğrudan Gemini API kullanarak Bulgarcaya çevirip otomatik yapıştıran Windows sistem tepsisi uygulaması.

## Nasıl Çalışır

1. `Ctrl+Shift+B` tuşunu **basılı tut** → mikrofon açılır
2. **Türkçe konuş**
3. `Ctrl+Shift+B` tuşunu **bırak** → ses doğrudan Gemini'a gönderilir
4. Bulgarca çeviri **otomatik olarak aktif alana yapıştırılır**
5. Kullanıcının panodaki (clipboard) eski verisi korunur.

## Kurulum

### Gereksinimler
- Windows 10/11 (x64)
- .NET 8 Runtime (veya self-contained exe)
- Gemini API Key

### Yapılandırma

1. `appsettings.json` dosyasını açın:
   ```json
   {
     "GeminiApiKey": "YOUR_KEY_HERE",
     "PasteDelayMs": 300
   }
   ```
2. Uygulamayı başlatın

## Geliştirme

Detaylar için:
- [PLAN.md](PLAN.md) — Geliştirme aşamaları ve görev listesi
- [ARCHITECTURE.md](ARCHITECTURE.md) — Teknik mimari

### Build

```powershell
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true
```

## Güvenlik Notu

`appsettings.json` `.gitignore`'da tutulur.
**API keylerini asla commit etmeyin.**
