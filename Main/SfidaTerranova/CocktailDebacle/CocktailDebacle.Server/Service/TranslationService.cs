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
            
            // Configura timeout più breve per evitare attese infinite
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> TranslateTextAsync(
            string text, 
            string toLanguage, 
            string? fromLanguage = null)
        {
            try
            {
                // Validazione input
                ValidateInput(text, toLanguage, fromLanguage);

                // Recupera configurazione
                var endpoint = GetConfiguredEndpoint();
                var key = GetSubscriptionKey();
                var region = _config["TranslatorService:Region"];

                // Configura headers
                ConfigureRequestHeaders(key, region);

                // Prepara richiesta
                var requestBody = new object[] { new { Text = text } };
                var content = PrepareRequestContent(requestBody);

                // Costruisci URL
                var url = BuildTranslationUrl(endpoint, toLanguage, fromLanguage);

                // Log dettagliato
                _logger.LogInformation("🔍 Invio richiesta di traduzione a {Url}", url);
                _logger.LogDebug("Testo da tradurre: {Text}", text);

                // Invia richiesta
                var response = await _httpClient.PostAsync(url, content);
                
                // Gestisci risposta
                return await HandleResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la traduzione");
                throw; // Rilancia per gestione nell'API controller
            }
        }

        private void ValidateInput(string text, string toLanguage, string? fromLanguage)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Il testo da tradurre è obbligatorio");
            if (string.IsNullOrWhiteSpace(toLanguage))
                throw new ArgumentException("La lingua di destinazione è obbligatoria");
            if (!_supportedLanguages.Contains(toLanguage.ToLower()))
                throw new ArgumentException($"Lingua di destinazione non supportata: {toLanguage}");
            if (!string.IsNullOrWhiteSpace(fromLanguage) && 
                !_supportedLanguages.Contains(fromLanguage.ToLower()))
            {
                throw new ArgumentException($"Lingua di origine non supportata: {fromLanguage}");
            }
        }

        private string GetConfiguredEndpoint()
        {
            var endpoint = _config["TranslatorService:Endpoint"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new InvalidOperationException("Endpoint di traduzione non configurato");

            return endpoint;
        }

        private string GetSubscriptionKey()
        {
            var key = Environment.GetEnvironmentVariable("TRANSLATOR_SUBSCRIPTION_KEY") 
                   ?? _config["TranslatorService:SubscriptionKey"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Chiave di sottoscrizione non configurata");
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
            var fromParam = string.IsNullOrWhiteSpace(fromLanguage) 
                ? "" 
                : $"&from={fromLanguage.ToLower()}";

            return $"{endpoint}/translator/text/v3.0/translate?api-version=3.0{fromParam}&to={toLanguage.ToLower()}";
        }


        private async Task<string> HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore nella risposta: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                
                throw new HttpRequestException(
                    $"Errore nella richiesta di traduzione: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<List<TranslationResult>>(result);

            if (parsed == null || !parsed.Any())
                throw new InvalidOperationException("Nessun risultato di traduzione ricevuto");

            if (parsed != null && parsed.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text != null)
            {
                var firstTranslation = parsed.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text;
                if (string.IsNullOrEmpty(firstTranslation))
                {
                    _logger.LogWarning("Traduzione non trovata nella risposta JSON.");
                    return string.Empty;
                }
                return firstTranslation;
            }
            else
            {
                _logger.LogWarning("Traduzione non trovata nella risposta JSON.");
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