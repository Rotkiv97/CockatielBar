using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailDebacle.Server.Models;
using CocktailDebacle.Server.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System;
using Microsoft.AspNetCore.RateLimiting;
using CocktailDebacle.Server.DTOs; // Importa il namespace del DTO

using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CocktailDebacle.Server.Controllers
{
    [Route("api/[controller]")]
    public class CocktailsController : ControllerBase
    {
       
        private readonly AppDbContext _context;
        private readonly CocktailImportService _cocktailImportService;
        private readonly HttpClient _httpClient;
        public CocktailsController(AppDbContext context, CocktailImportService cocktailImportService, HttpClient httpClient)
        {
            _context = context;
            _cocktailImportService = cocktailImportService;
            _httpClient = httpClient;
        }

        //Api cocktail /////////// /////////////////////////////
        [HttpPost("import-cocktails")]
        public async Task<IActionResult> GetCocktails()
        {
            await _cocktailImportService.ImportCocktailsAsync();
            return Ok("Importazione completata!");
        }

        [HttpGet("cocktails")]
        public async Task<ActionResult<IEnumerable<CocktailDto>>> GetCocktailsList()
        {
            var cocktails = await _context.DbCocktails
                .Select(c => new CocktailDto
                {
                    IdDrink = c.IdDrink,
                    StrDrink = c.StrDrink,
                    StrCategory = c.StrCategory,
                    StrAlcoholic = c.StrAlcoholic,
                    StrGlass = c.StrGlass,
                    StrInstructions = c.StrInstructions,
                    StrDrinkThumb = c.StrDrinkThumb
                })
                .ToListAsync();

            return Ok(cocktails);
        }

    }
}