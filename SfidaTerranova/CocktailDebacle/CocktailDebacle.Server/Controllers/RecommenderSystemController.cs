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

namespace CocktailDebacle.Server.Controllers
{
    [Route("api/[controller]")]
    public class RecommenderSystemController : Controller
    {
        private readonly AppDbContext _context; // Aggiungi il contesto del database se necessario
        // private readonly OpenAIService _openAIService; // Aggiungi il servizio OpenAI se necessario


        // public RecommenderSystemController(AppDbContext context, OpenAIService openAIService)
        // {
        //     _context = context;
        //     _openAIService = openAIService;
        // }

        // [HttpGet]
        // public IActionResult GetRecommenderSystems([FromQuery] string input)
        // {
        //     try
        //     {
        //         var recommenderSystems = _openAIService.GetEmbeddingAsync(input); // Assicurati di avere DbSet<RecommenderSystems> in AppDbContext
        //         return Ok(recommenderSystems);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "Errore interno del server.");
        //     }
        // }

        // [Authorize]
        // [HttpPost("Update-RecommenderSystem-Profile")]
        // public async Task<IActionResult> UpdateRecommenderSystemProfile() {
        //     var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);
        //     if (user == null) {
        //         return Unauthorized("Utente non trovato.");
        //     }

        //     var CocktailsLike = 
        // }

        // [Authorize]
        // [HttpGet("Get-RecommenderSystem-Profile")]
        // public async Task<IActionResult> GetCokctailsRecommended(){
        //     var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);
        //     if (user == null) {
        //         return Unauthorized("Utente non trovato.");
        //     }

        //     var profile = await _context.DbRecommenderSystems.FirstOrDefaultAsync(r => r.UserId == user.Id);
        //     if (profile == null) {
        //         return NotFound("Profilo non trovato.");
        //     }

        // }
    }
}