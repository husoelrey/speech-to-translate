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
                        new { text = "Sen uzman bir çevirmensin. Gönderilen sesteki konuşma kesinlikle Türkçedir. Öncelikle duyduğun sesi Türkçe fonetiğine ve kelime dağarcığına (örneğin 'atıştırmalık' gibi kelimelere) uygun olarak kusursuzca algıla. Ardından bu Türkçe metni doğal ve günlük Bulgarcaya çevir.\n\nKRİTİK KURALLAR:\n1. Türkçe eklere (-den, -e) ve gizli özneye dikkat et ('marketten alacağım' -> ben marketten alacağım demektir, market alacak demek değildir).\n2. Gündelik Türkçede 'yurt' kelimesi öğrenci yurdu (общежитие) demektir.\n3. SADECE nihai Bulgarca çeviriyi yaz. Merhaba, 'İşte çeviri' vb. hiçbir sohbet, açıklama veya tırnak işareti ekleme." },
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
