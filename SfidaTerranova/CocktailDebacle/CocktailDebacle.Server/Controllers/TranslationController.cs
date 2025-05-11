using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocktailDebacle.Server.Service;
using Microsoft.AspNetCore.Mvc;

namespace CocktailDebacle.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translator;
        private readonly ILogger<TranslationController> _logger;
        public TranslationController(TranslationService translator, ILogger<TranslationController> logger)
        {
            _logger = logger;
            _logger.LogInformation("TranslationController initialized.");
            _logger.LogInformation("TranslationService: " + translator.ToString());
            _translator = translator;
        }

        [HttpPost("translate")]
        public async Task<IActionResult> TranslateText([FromBody] TranslationRequest request)
        {
            if (string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.ToLanguage))
            {
                return BadRequest("Text and target language are required.");
            }

            try
            {
                var translatedText = await _translator.TranslateTextAsync(request.Text, request.ToLanguage, request.FromLanguage);
                _logger.LogInformation("🔍 RICEVUTO: Text = {Text}, ToLanguage = {ToLanguage}", request.Text, request.ToLanguage);
                return Ok(new { result = translatedText});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
    public class TranslationRequest
    {
        public string? Text { get; set; } 
        public string? ToLanguage { get; set; }
        public string? FromLanguage { get; set; }
    }
}