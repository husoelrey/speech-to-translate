# VoiceTranslate

**Win+B** kısayolu ile Türkçe sesli girişi Bulgarcaya çevirip otomatik yapıştıran Windows sistem tepsisi uygulaması.

## Nasıl Çalışır

1. `Win+B` tuşunu **basılı tut** → mikrofon açılır
2. **Türkçe konuş**
3. `Win+B` tuşunu **bırak** → ses işlenir
4. Bulgarca çeviri **otomatik olarak aktif alana yapıştırılır**
5. Sonuç aynı zamanda **pano (clipboard)** da kalır

## Kurulum

### Gereksinimler
- Windows 10/11 (x64)
- .NET 8 Runtime (veya self-contained exe)
- Google Cloud hesabı (Speech-to-Text API)
- Gemini API Key

### Yapılandırma

1. `appsettings.json` dosyasını açın:
   ```json
   {
     "GoogleCredentialsPath": "credentials/google-speech-key.json",
     "GeminiApiKey": "YOUR_KEY_HERE",
     ...
   }
   ```
2. Google Cloud Speech-to-Text Service Account JSON'unu `credentials/` klasörüne koyun
3. Uygulamayı başlatın

## Geliştirme

Detaylar için:
- [PLAN.md](PLAN.md) — Geliştirme aşamaları ve görev listesi
- [ARCHITECTURE.md](ARCHITECTURE.md) — Teknik mimari

### Build

```powershell
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true
```

## Dil Çiftleri

Şu an: **Türkçe → Bulgarca**

İleride `appsettings.json`'a eklenebilir:
- Türkçe → İngilizce
- İngilizce → Rusça
- İngilizce → Bulgarca

## Güvenlik Notu

`appsettings.json` ve `credentials/` klasörü `.gitignore`'da tutulur.
**API keylerini asla commit etmeyin.**
