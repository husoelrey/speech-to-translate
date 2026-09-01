# VoiceTranslate 🎙️ 🇧🇬

**VoiceTranslate**, Windows sistem tepsisinde (System Tray) arka planda sessizce çalışan, Türkçe sesli girdilerinizi gerçek zamanlı olarak Bulgarcaya çevirip aktif pencereye otomatik yapıştıran profesyonel bir masaüstü asistanıdır.

Gücünü **Google Gemini 3.5 Flash Lite** modelinden alır. Metne dönüştürme (STT) ve çeviri işlemlerini tek adımda, ışık hızında gerçekleştirerek iş akışınızı kesintiye uğratmaz.

---

## 🌟 Özellikler

- **Global Kısayol (Ctrl+Shift+B):** Hangi uygulamada (Word, Chrome, Discord vb.) olursanız olun, kısayola basarak çeviriyi tetikleyebilirsiniz.
- **Akıllı Yapıştırma & Pano Koruması:** Çevrilen metin doğrudan yazma alanına yapıştırılır. Üstelik panonuzda (Clipboard) daha önceden kopyaladığınız önemli verileriniz silinmez, işlem bitince otomatik olarak geri yüklenir.
- **Minimalist Sistem Tepsisi Arayüzü:** Görev çubuğunda yer kaplamaz. Renkli ikon ve balon bildirimleriyle durum (Kayıt, İşleniyor, Başarılı, Hata) hakkında sizi bilgilendirir.
- **Grafiksel Ayarlar Formu:** API anahtarınızı ve yapıştırma hızı gibi ayarlarınızı kolayca arayüz üzerinden güncelleyebilirsiniz.
- **Otomatik Başlatma:** İsteğe bağlı olarak Windows açılışında otomatik olarak başlar.

---

## 🚀 Kurulum ve Başlangıç

VoiceTranslate uygulamasını kullanmaya başlamak için aşağıdaki adımları sırasıyla izleyin.

### 1. Uygulamayı İndirme / Derleme
Eğer uygulamanın kaynak kodunu indirdiyseniz, proje dizininde aşağıdaki komutu çalıştırarak çalıştırılabilir `.exe` dosyasını oluşturabilirsiniz:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
Derlenen dosya `VoiceTranslate/bin/Release/net8.0-windows/win-x64/publish/VoiceTranslate.exe` yolunda bulunacaktır.

### 2. Gemini API Anahtarı (API Key) Edinme
VoiceTranslate'in çeviri yapabilmesi için ücretsiz bir Google Gemini API anahtarına ihtiyacınız vardır:
1. [Google AI Studio](https://aistudio.google.com/)'ya gidin ve Google hesabınızla giriş yapın.
2. Sol menüdeki **"Get API key"** butonuna tıklayın.
3. **"Create API key"** butonuna tıklayarak yeni bir anahtar oluşturun.
4. Oluşturulan, uzun harf ve rakamlardan oluşan anahtarı kopyalayın. *(Bu anahtarı kimseyle paylaşmayın)*.

### 3. Uygulamayı Yapılandırma
1. `VoiceTranslate.exe` dosyasını çalıştırın. Windows sağ alt köşede (Sistem Tepsisi) gri bir ikon belirecektir.
2. Bu ikona **sağ tıklayın** ve menüden **"Settings..."** (Ayarlar) seçeneğini seçin.
3. Açılan pencerede **"Gemini API Key"** kutucuğuna kopyaladığınız API anahtarını yapıştırın.
4. **"Save"** (Kaydet) butonuna tıklayın. Ayarlarınızın aktif olması için uygulamaya sağ tıklayıp **Exit** (Çıkış) diyerek kapatın ve uygulamayı yeniden başlatın.

*(İsteğe bağlı)* Uygulamanın her bilgisayar açıldığında hazır olmasını isterseniz, tepsi ikonuna sağ tıklayıp **"Run at Windows Startup"** seçeneğini işaretleyebilirsiniz.

---

## 🎤 Nasıl Kullanılır?

VoiceTranslate, global kısayol mantığıyla "Aç / Kapat" (Toggle) şeklinde çalışır.

1. **Kayda Başla:** Bir metin kutusundayken (örn. mesajlaşma uygulaması) `Ctrl+Shift+B` tuş kombinasyonuna bir kez basın. 
   - *Tepsi ikonu **Kırmızı** renge döner ve kayıt başlar.*
2. **Konuşun:** Türkçe olarak mikrofona konuşun.
3. **Kaydı Bitir ve Çevir:** Konuşmanız bittiğinde tekrar `Ctrl+Shift+B` kombinasyonuna bir kez basın.
   - *Tepsi ikonu **Sarı** renge döner (İşleniyor).*
   - *Çeviri tamamlandığında ikon **Yeşil** olur.*
   - *Bulgarca metin, imlecinizin bulunduğu yere otomatik olarak yapıştırılır.*
   - *Ekranın sağ alt köşesinde "Pasted: ..." şeklinde bir başarı balonu görünür.*

---

## 🛠️ Geliştirici & Mimari

Proje modern C# .NET 8 WinForms mimarisi ile tasarlanmıştır.

- [PLAN.md](PLAN.md) — Geliştirme aşamaları ve görev listesi
- [ARCHITECTURE.md](ARCHITECTURE.md) — Teknik mimari kararları, component şeması ve modüller
- Ses kaydı için `NAudio`, tuş simülasyonları için `InputSimulatorPlus` kütüphaneleri kullanılmıştır.

### Güvenlik Notu
Uygulamaya girdiğiniz API anahtarı, `VoiceTranslate.exe` ile aynı klasördeki `appsettings.json` dosyasına şifresiz olarak salt metin (plain text) formatında kaydedilir. Uygulamayı güvenmediğiniz kişilerle paylaşırken bu dosyayı (veya içindeki API anahtarınızı) silmeyi unutmayın. Geliştirme sürecinde `.gitignore` dosyası aracılığıyla bu dosya Github depolarından izole edilmiştir.
