using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VoiceTranslate.Config;

namespace VoiceTranslate.Core;

public class TranslationService
{
    private readonly AppSettings _appSettings;
    private readonly HttpClient _httpClient;

    public TranslationService(AppSettings appSettings)
    {
        _appSettings = appSettings;
        _httpClient = new HttpClient();
    }

    public async Task<string> TranslateAudioAsync(byte[] wavBytes)
    {
        string apiKey = _appSettings.GeminiApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        string base64Audio = Convert.ToBase64String(wavBytes);

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = """
Sen Türkçe ses kayıtlarını doğrudan Bulgarcaya çeviren bir ses çeviri motorusun.

GÖREV:
Gelen Türkçe ses kaydındaki konuşmayı anla, konuşma pürüzlerini (ııı, şey, tekrar eden kelimeler) temizle ve doğrudan doğal, akıcı bir günlük konuşma diliyle Bulgarcaya çevir.

KURALLAR:
1. Türkçe cümlelerdeki gizli özneleri, zaman eklerini ve yönelme/ayrılma gibi hal eklerini bağlama göre eksiksiz aktar.
2. Motamot (kelimesi kelimesine) çeviri yapma; Bulgarca konuşma dilindeki doğal kalıpları ve deyimleri tercih et.
3. Seste anlaşılır bir konuşma yoksa veya sadece arka plan gürültüsü varsa hiçbir şey üretme (boş metin döndür).
4. ÇIKTI FORMATI: SADECE çevrilmiş Bulgarca metni döndür. Tırnak işareti, transkripsiyon, Türkçe metin, açıklama veya selamlama asla ekleme.
""" },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "audio/wav",
                                data = base64Audio
                            }
                        }
                    }
                }
            }
        };

        string jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent?key={apiKey}";

        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {errorContent}");
        }

        string responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        
        try
        {
            // Traverse the JSON to get the translated text: 
            // root -> candidates[0] -> content -> parts[0] -> text
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to parse the response from Gemini API.", ex);
        }
    }
}
