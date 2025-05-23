using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace CocktailDebacle.Server.Service
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<TranslationService> _logger;
        
        // Lingue supportate - puoi espanderle secondo le tue esigenze
        private readonly HashSet<string> _supportedLanguages = new() 
        { 
            "it", "en", "fr", "es", "de", "pt", "ru", "zh", "ja", "ar" 
        };

        public TranslationService(
            HttpClient httpClient, 
            ILogger<TranslationService> logger, 
            IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
            
            // Configuro timeout più breve per evitare attese prolungate
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> TranslateTextAsync(string text, string toLanguage, string? fromLanguage = null)
        {
            try
            {
                // Validazione input
                ValidateInput(text, toLanguage, fromLanguage);

                // Recupero configurazione
                var endpoint = GetConfiguredEndpoint();
                var key = GetSubscriptionKey();
                var region = _config["TranslatorService:Region"];

                if (region == null)
                {
                    return string.Empty;
                }

                // Configuro headers
                ConfigureRequestHeaders(key, region);

                // Preparo la richiesta
                var requestBody = new object[] { new { Text = text } };
                var content = PrepareRequestContent(requestBody);

                // Costruisco URL
                var url = BuildTranslationUrl(endpoint, toLanguage, fromLanguage);

                // Invio richiesta
                var response = await _httpClient.PostAsync(url, content);
                
                // Gestisco risposta
                return await HandleResponse(response);
            }
            catch
            {
                throw;
            }
        }

        private void ValidateInput(string text, string toLanguage, string? fromLanguage)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("The text to be translated is mandatory");
            if (string.IsNullOrWhiteSpace(toLanguage))
                throw new ArgumentException("The target language is mandatory");
            if (!_supportedLanguages.Contains(toLanguage.ToLower()))
                throw new ArgumentException($"Target language not supported: {toLanguage}");
            if (!string.IsNullOrWhiteSpace(fromLanguage) && !_supportedLanguages.Contains(fromLanguage.ToLower()))
                throw new ArgumentException($"Source language not supported: {fromLanguage}");
        }

        private string GetConfiguredEndpoint()
        {
            var endpoint = _config["TranslatorService:Endpoint"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new InvalidOperationException("Translation endpoint not configured");

            return endpoint;
        }

        private string GetSubscriptionKey()
        {
            var key = Environment.GetEnvironmentVariable("TRANSLATOR_SUBSCRIPTION_KEY") ?? _config["TranslatorService:SubscriptionKey"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Subscription key not configured");
            return key;
        }

        private void ConfigureRequestHeaders(string key, string region)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            
            if (!string.IsNullOrWhiteSpace(region))
            {
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);
            }
        }

        private StringContent PrepareRequestContent(object requestBody)
        {
            return new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );
        }

        private string BuildTranslationUrl(string endpoint, string toLanguage, string? fromLanguage)
        {
            var fromParam = string.IsNullOrWhiteSpace(fromLanguage) ? "" : $"&from={fromLanguage.ToLower()}";

            return $"{endpoint}/translator/text/v3.0/translate?api-version=3.0{fromParam}&to={toLanguage.ToLower()}";
        }


        private async Task<string> HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException( $"Error in translation request: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<List<TranslationResult>>(result);

            if (parsed == null || !parsed.Any())
                throw new InvalidOperationException("No translation results received");

            if (parsed != null && parsed.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text != null)
            {
                var firstTranslation = parsed.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text;
                if (string.IsNullOrEmpty(firstTranslation))
                {
                    return string.Empty;
                }
                return firstTranslation;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class TranslationResult
    {
        [JsonPropertyName("translations")]
        public List<TranslationText>? Translations { get; set; }
    }

    public class TranslationText
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("to")]
        public string? To { get; set; }
    }
}