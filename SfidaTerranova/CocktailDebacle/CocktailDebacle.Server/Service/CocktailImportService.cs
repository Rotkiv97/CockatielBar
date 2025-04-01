using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CocktailDebacle.Server.Models;

namespace CocktailDebacle.Server.Service
{
    public class CocktailImportService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public CocktailImportService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task ImportCocktailsAsync()
        {
            var letters = "abcdefghijklmnopqrstuvwxyz0123456789";

        foreach (var letter in letters)
        {
            var url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?f={letter}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) continue;

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CocktailResponse>(content);

            if (result?.Drinks == null) continue;

            foreach (var drink in result.Drinks)
            {
                // Optional: evitare duplicati (in base a idDrink)
                if (!_context.Drinks.Any(d => d.IdDrink == drink.IdDrink))
                {
                    _context.Drinks.Add(drink);
                }
            }

            await _context.SaveChangesAsync(); // salva dopo ogni lettera
        }
        }
    }
}