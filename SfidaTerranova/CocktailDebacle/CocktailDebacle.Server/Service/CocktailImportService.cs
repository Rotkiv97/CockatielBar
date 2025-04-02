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
            _logger.LogInformation("CocktailImportService initialized.✅");
            _httpClient = httpClient;
            _context = context;
        }

        public async Task ImportCocktailsAsync()
        {
            if(_context.DbCocktails.Any())
            {
                Console.WriteLine("Cocktails already imported, skipping import. ❌");
                return; // Se ci sono già cocktail nel database, non fare nulla
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

                if (result?.Drinks == null) continue;

                foreach (var drink in result.Drinks)
                {
                    // Optional: evitare duplicati (in base a idDrink)
                    if (!_context.DbCocktails.Any(d => d.IdDrink == drink.IdDrink))
                    {
                        CheckCocktails(drink); // Controlla i campi nulli e assegna valori predefiniti
                        _context.DbCocktails.Add(drink);
                        Console.WriteLine($"Cocktail {drink.StrDrink} added to the database! 🍹");
                    }
                }

                await _context.SaveChangesAsync(); // salva dopo ogni lettera
                await Task.Delay(1000); // Aspetta 1 secondo tra le richieste per evitare di sovraccaricare l'API
            }
            Console.WriteLine("Cocktails imported successfully! ✅");
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