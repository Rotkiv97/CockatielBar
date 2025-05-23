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
        private readonly ILogger<CocktailImportService> _logger;

        public CocktailImportService(HttpClient httpClient, AppDbContext context, ILogger<CocktailImportService> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _context = context;
        }

        public async Task ImportCocktailsAsync()
        {
            if(_context.DbCocktails.Any())
            {
                Console.WriteLine("Cocktails already imported, skipping import. ❌");
                return;
            }

            var letters = "abcdefghijklmnopqrstuvwxyz0123456789";
            foreach (var letter in letters)
            {
                var url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?f={letter}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) {
                    Console.WriteLine($"Error fetching cocktails for letter {letter}: {response.StatusCode} ❌");
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CocktailResponse>(content);

                if (result?.Drinks == null)
                    continue;

                foreach (var drink in result.Drinks)
                {
                    if (!_context.DbCocktails.Any(d => d.IdDrink == drink.IdDrink))
                    {
                        CheckCocktails(drink);
                        _context.DbCocktails.Add(drink);
                    }
                }

                await _context.SaveChangesAsync();
                await Task.Delay(1000);
            }
        }

        private void CheckCocktails(Cocktail drink)
        {
            var properties = typeof(Cocktail).GetProperties()
            .Where(p => p.PropertyType == typeof(string));
            foreach (var prop in properties)
            {
                var value = (string?)prop.GetValue(drink);
                if (value == null)
                {
                    prop.SetValue(drink, string.Empty);
                }
            }
        }
    }

    public class CocktailResponse
    {
        public ICollection<Cocktail> Drinks { get; set; } = new List<Cocktail>();
    }
}