using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VoiceTranslate.Core;

public class TranslationService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public TranslationService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<string> TranslateAudioAsync(byte[] wavBytes)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
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
                        new { text = "Translate this Turkish audio to Bulgarian text. Return only the translated text." },
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

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent?key={_apiKey}";

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
