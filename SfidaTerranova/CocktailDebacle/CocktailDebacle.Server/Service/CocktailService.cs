using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CocktailDebacle.Server.Models;
using CocktailDebacle.Server.DTOs;

namespace CocktailDebacle.Server.Service
{
    public class CocktailService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrlthecocktaildb;

        public CocktailService(AppDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _apiUrlthecocktaildb = "https://www.thecocktaildb.com/api/json/v1/1/";
        }

        public async Task ImportCocktailsAsync()
    {
        char[] alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        foreach (char letter in alphabet)
        {
            var response = await _httpClient.GetAsync($"{_apiUrlthecocktaildb}search.php?f={letter}");
            if (!response.IsSuccessStatusCode) continue;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var cocktailsResponse = JsonSerializer.Deserialize<CocktailResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (cocktailsResponse?.Drinks == null) continue;

            foreach (var apiCocktail in cocktailsResponse.Drinks)
            {
                apiCocktail.ExtractIngredients();

                if (!await _context.DbCocktails.AnyAsync(c => c.Name == apiCocktail.StrDrink))
                {
                    var newCocktail = new Cocktail
                    {
                        Name = apiCocktail.StrDrink,
                        Category = apiCocktail.StrCategory,
                        IBA = apiCocktail.StrIBA,
                        Alcoholic = apiCocktail.StrAlcoholic,
                        Glass = apiCocktail.StrGlass,
                        Instructions = apiCocktail.StrInstructions,
                        ImageUrl = apiCocktail.StrDrinkThumb,
                        VideoUrl = apiCocktail.StrVideo,
                        Tags = apiCocktail.StrTags,
                        CreativeCommonsConfirmed = apiCocktail.StrCreativeCommonsConfirmed,
                        DateModified = string.IsNullOrEmpty(apiCocktail.DateModified) ? null : DateTime.Parse(apiCocktail.DateModified),
                        Ingredients = apiCocktail.Ingredients.Select(i => new Ingredient
                        {
                            Name = i.Name,
                            Measure = i.Measure
                        }).ToList()
                    };

                    _context.DbCocktails.Add(newCocktail);
                }
            }
        }

        await _context.SaveChangesAsync();
    }


    }

    public class CocktailResponse
    {
        public List<CocktailApiModel> Drinks { get; set; }
    }
}