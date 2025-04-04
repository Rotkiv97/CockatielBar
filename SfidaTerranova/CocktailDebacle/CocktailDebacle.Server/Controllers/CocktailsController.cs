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


        // http://localhost:5052/api/Cocktails/cocktails
        [HttpGet("cocktails")]
        public async Task<ActionResult<IEnumerable<CocktailDto>>> GetCocktailsList()
        {
            var cocktails = await _context.DbCocktails
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

            return Ok(cocktails);
        }


        // chiamata ipa e come impostarla 
        // http://localhost:5052/api/cocktails/search?ingredient=vodka&glass=Martini glass&alcoholic=Alcoholic&page=1&pageSize=10
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CocktailDto>>> SearchCoctktail(
            [FromQuery] string nameCocktail = "",
            [FromQuery] string glass = "",
            [FromQuery] string ingredient = "",
            [FromQuery] string category = "",
            [FromQuery] string alcoholic = "",
            [FromQuery] string description = "",
            [FromQuery] string username = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            IQueryable<Cocktail> query = _context.DbCocktails.AsQueryable();
            if(!string.IsNullOrEmpty(glass))
            {
                query = query.Where(c => c.StrGlass != null && c.StrGlass.ToLower().Contains(glass.ToLower()));
            }
            if(!string.IsNullOrEmpty(category))
            {
                query = query.Where(c => c.StrCategory != null && c.StrCategory.ToLower().Contains(category.ToLower()));
            }
            if(!string.IsNullOrEmpty(alcoholic))
            {
                query = query.Where(c => c.StrAlcoholic != null && c.StrAlcoholic.ToLower().Contains(alcoholic.ToLower()));
            }
            if(!string.IsNullOrEmpty(nameCocktail))
            {
                query = query.Where(c => c.StrDrink != null && c.StrDrink.ToLower().Contains(nameCocktail.ToLower()));
            }
            else if(!string.IsNullOrEmpty(ingredient))
            {
                query = query.Where(c => c.StrIngredient1 != null && c.StrIngredient1.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient2 != null && c.StrIngredient2.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient3 != null && c.StrIngredient3.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient4 != null && c.StrIngredient4.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient5 != null && c.StrIngredient5.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient6 != null && c.StrIngredient6.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient7 != null && c.StrIngredient7.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient8 != null && c.StrIngredient8.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient9 != null && c.StrIngredient9.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient10 != null && c.StrIngredient10.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient11 != null && c.StrIngredient11.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient12 != null && c.StrIngredient12.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient13 != null && c.StrIngredient13.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient14 != null && c.StrIngredient14.ToLower().Contains(ingredient.ToLower()) ||
                                         c.StrIngredient15 != null && c.StrIngredient15.ToLower().Contains(ingredient.ToLower())
                                    );
            }
            if(!string.IsNullOrEmpty(description))
            {
                query = query.Where(c => c.StrInstructions != null && c.StrInstructions.ToLower().Contains(description.ToLower()));
            }
            if(!string.IsNullOrEmpty(username))
            {
                // itrodurre una logica per filtrare i cocktail in base all'username e alle preferenze
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            //query = query.OrderBy(c => c.StrDrink); // Ordina i cocktail per nome

            if (!string.IsNullOrEmpty(nameCocktail))
            {
                string name = nameCocktail.ToLower();
                query = query
                    .OrderBy(c => c.StrDrink == null || !c.StrDrink.ToLower().StartsWith(name))
                    .ThenBy(c => c.StrDrink);
            }
            else
            {
                query = query.OrderBy(c => c.StrDrink);
            }
            var cocktails = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
            return Ok( new {
                TotalResult = totalItems,
                TotalPages = totalPages,
                CurrentPage = page < 1 ? 1 : page,
                PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize),
                Cocktails = cocktails
            });
        }
    }
}