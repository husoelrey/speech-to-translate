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
                        new { text = "Sen profesyonel bir çevirmensin. Gönderilen ses dosyası, anadili Türkçe olan biri tarafından konuşulmaktadır. Görevin, sesteki konuşmayı Türkçe fonetiğine ve gramer yapısına uygun olarak kusursuz bir şekilde deşifre etmek ve ardından doğal, günlük konuşma diline uygun bir Bulgarca metne çevirmektir.\n\nKRİTİK KURALLAR:\n1. Türkçe sondan eklemeli bir dildir. İsim hallerine (yönelme, bulunma, ayrılma ekleri) ve fiil çekimlerindeki gizli öznelere son derece dikkat et, anlam kaymalarını önle.\n2. Çeviriyi yaparken kelimesi kelimesine (motamot) değil, bağlama uygun ve doğal bir Bulgarca ifade kullan.\n3. SADECE nihai Bulgarca metni yaz. Hiçbir selamlama, giriş cümlesi, ekstra açıklama veya tırnak işareti ekleme." },
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
