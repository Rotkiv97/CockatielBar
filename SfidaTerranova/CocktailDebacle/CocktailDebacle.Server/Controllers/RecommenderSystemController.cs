using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CocktailDebacle.Server.Models;
using CocktailDebacle.Server.Service;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CocktailDebacle.Server.DTOs;

namespace CocktailDebacle.Server.Controllers
{
    [Route("api/[controller]")]
    public class RecommenderSystemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly RecommenderEngine _recommenderEngine;


        public RecommenderSystemController(AppDbContext context, RecommenderEngine recommenderEngine)
        {
            _context = context;
            _recommenderEngine = recommenderEngine;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommenderSystems([FromQuery] string input)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server. \n {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("Update-RecommenderSystem-Profile")]
        public async Task<IActionResult> UpdateRecommenderSystemProfile()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
                return Unauthorized("Utente non trovato.");

            var profileSources = new List<string>();

            // Estraggo cocktail creati (opzionale, ma possiamo tenerli)
            var createdCocktails = await _context.DbCocktails
                .Where(c => c.UserNameCocktail == userName)
                .ToListAsync();

            profileSources.AddRange(
                createdCocktails
                    .SelectMany(c => new[] {
                        c.StrDrink,
                        c.StrInstructions,
                        c.StrCategory, 
                        c.StrAlcoholic, 
                        c.StrGlass, 
                        c.StrIngredient1, 
                        c.StrIngredient2,
                        c.StrIngredient3,
                        c.StrIngredient4,
                        c.StrIngredient5,
                        c.StrIngredient6 
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .OfType<string>());

            // Estraggo ricerche recenti (mock, o puoi salvarle in tabella ricerche se vuoi)
            var lastProfile = await _context.DbRecommenderSystems
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.LastUpdated)
                .FirstOrDefaultAsync();

            if (lastProfile != null && !string.IsNullOrWhiteSpace(lastProfile.ProfileText))
            {
                profileSources.AddRange(lastProfile.ProfileText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            // Pulisco e preparo il testo
            var profileText = string.Join(", ",
                profileSources
                    .Select(s => s.ToLowerInvariant())
                    .Distinct()
            );

            if (string.IsNullOrWhiteSpace(profileText))
            {
                return Ok(new { Message = "Nessuna preferenza disponibile per aggiornare il profilo." });
            }

            // Richiedo embedding da OpenAI
            var vector = await ;
            var embeddingJson = System.Text.Json.JsonSerializer.Serialize(vector);

            // Aggiorno o creo RecommenderSystems
            var existing = await _context.DbRecommenderSystems.FirstOrDefaultAsync(r => r.UserId == user.Id);
            if (existing != null)
            {
                existing.ProfileText = profileText;
                existing.VectorJsonEmbedding = embeddingJson;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _context.DbRecommenderSystems.Add(new RecommenderSystems
                {
                    UserId = user.Id,
                    ProfileText = profileText,
                    VectorJsonEmbedding = embeddingJson,
                    LastUpdated = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "✅ Profilo aggiornato con successo.",
                ProfileText = profileText,
                LastUpdated = DateTime.UtcNow
            });
        }


        [Authorize]
        [HttpGet("Get-RecommenderSystem-Profile")]
        public async Task<IActionResult> GetRecommendedCocktails([FromQuery] int maxResults = 10)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null) return Unauthorized();

            var profile = await _context.DbRecommenderSystems.FirstOrDefaultAsync(r => r.UserId == user.Id);

            if (profile == null || string.IsNullOrWhiteSpace(profile.VectorJsonEmbedding))
            {
                // Nessun profilo → restituisci cocktail casuali
                var randomCocktails = await _context.DbCocktails
                    .Where(c => c.PublicCocktail == true)
                    .OrderBy(c => Guid.NewGuid())
                    .Take(maxResults)
                    .Select(c => new CocktailDto
                    {
                        IdDrink = c.IdDrink ?? string.Empty,
                        StrDrink = c.StrDrink ?? string.Empty,
                        StrCategory = c.StrCategory ?? string.Empty,
                        StrAlcoholic = c.StrAlcoholic ?? string.Empty,
                        StrGlass = c.StrGlass ?? string.Empty,
                        StrInstructions = c.StrInstructions ?? string.Empty,
                        StrDrinkThumb = c.StrDrinkThumb ?? string.Empty
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Message = "Profilo non disponibile, risultati casuali.",
                    Cocktails = randomCocktails
                });
            }

            // ✅ Profilo esistente → confronta con cocktail pubblici
            var userVector = System.Text.Json.JsonSerializer.Deserialize<float[]>(profile.VectorJsonEmbedding)!;

            var cocktails = await _context.DbCocktails
                .Where(c => c.PublicCocktail == true)
                .Take(100) // per evitare troppe richieste embedding
                .ToListAsync();

            var ranked = await _recommenderEngine.;
            foreach (var cocktail in cocktails)
            {
                var text = string.Join(", ",
                    new[] { cocktail.StrDrink, cocktail.StrCategory, cocktail.StrIngredient1, cocktail.StrIngredient2, cocktail.StrGlass }
                    .Where(s => !string.IsNullOrEmpty(s)));

                var cocktailVec = await;
                var similarity = RecommenderSystemsUtils.CosineSimilarity(userVector, cocktailVec);

                ranked.Add((cocktail, similarity));
            }

            var best = ranked
                .OrderByDescending(r => r.Item2)
                .Take(maxResults)
                .Select(r => new CocktailDto
                {
                    IdDrink = r.Item1.IdDrink ?? string.Empty,
                    StrDrink = r.Item1.StrDrink ?? string.Empty,
                    StrCategory = r.Item1.StrCategory ?? string.Empty,
                    StrAlcoholic = r.Item1.StrAlcoholic ?? string.Empty,
                    StrGlass = r.Item1.StrGlass ?? string.Empty,
                    StrInstructions = r.Item1.StrInstructions ?? string.Empty,
                    StrDrinkThumb = r.Item1.StrDrinkThumb ?? string.Empty
                })
                .ToList();

            return Ok(new
            {
                Message = "Risultati personalizzati basati sul tuo profilo.",
                Cocktails = best
            });
        }
    }
}