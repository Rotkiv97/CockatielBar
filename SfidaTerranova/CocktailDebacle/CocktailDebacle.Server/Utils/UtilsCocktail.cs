using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using CocktailDebacle.Server.DTOs;
using CocktailDebacle.Server.Models;


namespace CocktailDebacle.Server.Utils
{
    public class UtilsCocktail
    {

        public static readonly Dictionary<string, int> GlassCapacity = new Dictionary<string, int>
        {
            { "Highball glass", 350 },
            { "Cocktail glass", 150 },
            { "Old-fashioned glass", 300 },
            { "Collins glass", 400 },
            { "Margarita glass", 300 },
            { "Pint glass", 500 },
            { "Shot glass", 50 },
            { "Whiskey sour glass", 250 },
            { "Hurricane glass", 400 },
            { "Champagne flute", 200 },
            { "Beer mug", 500 },
            { "Brandy snifter", 300 },
            { "Cordial glass", 100 },
            { "Copper mug", 350 },
            { "Irish coffee cup", 300 }
        };

        public static readonly List<string> MeasurementUnits = new List<string>
        {
            "ml", "oz", "cl", "cup", "tsp", "tbsp", "dash", "part", "shot", "pint", 
            "quart", "gallon", "drop", "slice", "piece", "pinch", "bottle", "can", "splash", 
            "barspoon", "teaspoon", "tablespoon", "liter", "centiliter", "deciliter", "sprig"
        };

        public static readonly List<string> IngredientAlcoholic = new List<string>
        {
            // Distillati classici
            "Rum", "Light rum", "Dark rum", "Añejo rum", "Spiced rum", "Coconut rum", "Overproof rum",
            "Vodka", "Lemon vodka", "Vanilla vodka", "Pepper vodka", "Flavored vodka", "Chocolate vodka", "Espresso vodka",
            "Gin", "Dry gin", "Old Tom gin", "Sloe gin", "Pink gin",
            "Tequila", "Silver tequila", "Gold tequila", "Añejo tequila", "Reposado tequila",
            "Whiskey", "Bourbon", "Rye whiskey", "Scotch", "Blended Scotch", "Single Malt Scotch",
            "Irish whiskey", "Canadian whisky", "Japanese whisky", "Tennessee whiskey",
            "Brandy", "Cognac", "Armagnac", "Calvados", "Pisco", "Grappa", "Applejack", "Kirsch",
            "Schnapps", "Peach schnapps", "Peppermint schnapps", "Sour apple schnapps", "Cinnamon schnapps", "Butterscotch schnapps",
            "Aquavit", "Genever", "Ouzo", "Pastis", "Absinthe", "Pernod", "Raki", "Sambuca",
            "Cachaça", "Mezcal", "Aguardiente", "Poire Williams", "Slivovitz",

            // Amari, Vermouth, Bitter
            "Sweet vermouth", "Dry vermouth", "Red vermouth", "Bianco vermouth", "Rosé vermouth",
            "Aperol", "Campari", "Cynar", "Fernet", "Amaro", "Jägermeister", "Benedictine",
            "Angostura bitters", "Orange bitters", "Peychaud's bitters", "Chocolate bitters",

            // Liquori e creme
            "Triple sec", "Cointreau", "Grand Marnier", "Curacao", "Blue Curacao", "Orange Curacao", "Dry Curacao",
            "Maraschino liqueur", "Cherry Heering", "Midori", "Galliano", "Amaretto", "Advocaat",
            "Chartreuse", "Drambuie", "Frangelico", "Kahlua", "Tia Maria", "Sambuca", "Anisette", "Strega", "Limoncello",
            "Crème de cacao", "White crème de cacao", "Crème de cassis", "Crème de menthe", "Crème de banane",
            "Chambord", "Peach liqueur", "Passoa", "Passion fruit liqueur", "Lychee liqueur",
            "Raspberry liqueur", "Strawberry liqueur", "Blackcurrant liqueur", "Blueberry liqueur", "Mango liqueur", "Watermelon liqueur",
            "Melon liqueur", "Apple liqueur", "Pineapple liqueur", "Coconut liqueur", "Banana liqueur",
            "Chocolate liqueur", "Coffee liqueur", "Espresso liqueur", "Hazelnut liqueur", "Almond liqueur",
            "Cream liqueur", "Irish cream liqueur", "Baileys Irish Cream", "Rum cream", "Whiskey cream liqueur",
            "Butterscotch liqueur", "Caramel liqueur", "Maple liqueur", "Honey liqueur", "Herbal liqueur",
            "Elderflower liqueur", "Rose liqueur", "Lavender liqueur", "Sage liqueur", "Thyme liqueur", "Mint liqueur",
            "Ginger liqueur", "Chili liqueur", "Violet liqueur", "Yuzu liqueur", "Lime liqueur",

            // Bitter aromatici/amari speciali
            "Amaro Averna", "Amaro Montenegro", "Amaro Lucano", "Amaro Ramazzotti", "Amaro del Capo", "Unicum", "Zwack",
            "Becherovka", "Underberg", "Jägermeister", "Fernet Branca", "Fernet Stock", "Picon", "Branca Menta",

            // Vini liquorosi, fortified & aromatizzati
            "Port", "Tawny port", "Ruby port", "White port",
            "Sherry", "Fino sherry", "Manzanilla", "Oloroso", "Pedro Ximénez",
            "Madeira", "Marsala", "Vermouth", "Dubonnet", "Lillet Blanc", "Lillet Rouge", "Cocchi Americano",

            // Brandy e brandy fruttati
            "Apricot brandy", "Peach brandy", "Blackberry brandy", "Cherry brandy", "Fruit brandy", "Raspberry brandy", "Apple brandy", "Pear brandy",

            // Variazioni "spiritose"
            "Fruit liqueur", "Spiced liqueur", "Citrus liqueur", "Nut liqueur", "Herbal liqueur", "Root liqueur",

            // Ingredienti internazionali
            "Soju", "Sake", "Shochu", "Baijiu", "Arrack", "Tepache", "Palm wine", "Toddy",

            // Alcolici dolci/aromatizzati moderni
            "Caramel vodka", "Coconut vodka", "Raspberry vodka", "Strawberry vodka", "Peach vodka", "Watermelon vodka", "Apple vodka", "Grape vodka", "Blueberry vodka", "Pineapple vodka", "Red berry vodka",

            // Amari italiani e digestivi
            "Amaro", "Amaretto", "Cynar", "Fernet Branca", "Lucano", "Montenegro", "Ramazzotti", "Averna", "Unicum", "China Martini", "Braulio",

            // Bitter & digestivi di tutto il mondo
            "Suze", "Gammel Dansk", "Underberg", "Unicum", "Branca Menta",

            // Birra, cider, mead (rari nei cocktail ma usati in alcuni)
            "Beer", "Lager", "Ale", "Stout", "Porter", "Wheat beer", "Cider", "Mead",

            // Vini usati in mixology
            "Red wine", "White wine", "Sparkling wine", "Prosecco", "Champagne", "Rosé wine", "Sauternes",

            // Spirits artigianali/infusi
            "Infused vodka", "Infused gin", "Infused rum", "Barrel-aged gin", "Smoked whiskey", "Bacon-infused bourbon", "Truffle vodka",

            // Esempi di marchi popolari (solo alcuni)
            "Jägermeister", "Campari", "Aperol", "Benedictine", "Galliano", "Strega", "Drambuie", "Suze", "Chartreuse", "Lillet Blanc", "Cointreau", "Grand Marnier"
        };

        public static readonly List<string> IngredientNonAlcoholic = new List<string>
        {
            // Succhi di frutta
            "Orange juice", "Pineapple juice", "Cranberry juice", "Lemon juice", "Lime juice",
            "Apple juice", "Grapefruit juice", "Tomato juice", "Grape juice", "Mango juice",
            "Passion fruit juice", "Peach juice", "Pear juice", "Watermelon juice", "Carrot juice",
            "Pomegranate juice", "Coconut water", "Banana juice", "Strawberry juice", "Blueberry juice",
            "Blackcurrant juice", "Cherry juice", "Apricot juice", "Melon juice",

            // Bibite & Soda
            "Tonic water", "Club soda", "Soda water", "Sparkling water", "Ginger ale", "Ginger beer",
            "Cola", "Lemonade", "Lime soda", "Sprite", "7-Up", "Root beer", "Orange soda",
            "Red Bull", "Energy drink", "Mountain Dew",

            // Sciroppi
            "Grenadine syrup", "Sugar syrup", "Simple syrup", "Agave syrup", "Maple syrup",
            "Honey syrup", "Elderflower syrup", "Orgeat syrup", "Raspberry syrup", "Strawberry syrup",
            "Blueberry syrup", "Blackcurrant syrup", "Vanilla syrup", "Chocolate syrup", "Caramel syrup",
            "Mint syrup", "Ginger syrup", "Coconut syrup", "Almond syrup", "Hazelnut syrup",
            "Cherry syrup", "Passion fruit syrup", "Peach syrup", "Banana syrup",

            // Puree e Frutta fresca
            "Mango puree", "Passion fruit puree", "Strawberry puree", "Peach puree", "Banana puree",
            "Raspberry puree", "Pineapple puree", "Apple puree", "Pear puree", "Blueberry puree",
            "Lime", "Lemon", "Orange", "Pineapple", "Banana", "Strawberry", "Raspberry", "Cherry",
            "Mango", "Watermelon", "Melon", "Grapefruit", "Blackberry", "Cranberry", "Kiwi",
            "Peach", "Grape", "Coconut", "Apple", "Pear", "Apricot",

            // Verdura e garnish
            "Celery", "Cucumber", "Olive", "Mint leaves", "Basil leaves", "Rosemary", "Lime wedge",
            "Lemon wedge", "Orange slice", "Pineapple wedge", "Maraschino cherry", "Orange zest",
            "Lime zest", "Lemon zest", "Cinnamon stick", "Vanilla bean", "Fresh ginger", "Ginger slice",

            // Dairy & Alternatives
            "Milk", "Half-and-half", "Cream", "Heavy cream", "Coconut cream", "Almond milk",
            "Soy milk", "Oat milk", "Yogurt", "Greek yogurt", "Ice cream", "Egg white",

            // Altro
            "Bitters (non-alcoholic)", "Angostura (alcohol-free)", "Egg yolk", "Sour mix", "Sweet and sour mix",
            "Lemon squash", "Lime cordial", "Rose water", "Orange blossom water", "Sparkling apple cider",
            "Ginger juice", "Apple cider", "Aloe vera juice", "Coconut milk", "Coconut water",

            // Sciroppi e aromi esotici
            "Falernum", "Tamarind syrup", "Sarsaparilla", "Lavender syrup", "Rose syrup",
            "Saffron syrup", "Cardamom syrup", "Cinnamon syrup", "Clove syrup", "Pepper syrup",

            // Garnish e decorazioni extra
            "Edible flowers", "Cocoa powder", "Crushed ice", "Cubed ice", "Salt", "Sugar", "Brown sugar",
            "Demarara sugar", "Rock candy", "Lollipop", "Sprinkles", "Chocolate shavings", "Nutmeg",
            "Ground cinnamon", "Ground ginger", "Matcha powder", "Chili powder",

            // Altri ingredienti non alcolici
            "Espresso", "Coffee", "Cold brew coffee", "Tea", "Green tea", "Black tea", "Chai tea",
            "Iced tea", "Earl Grey tea", "Mint tea", "Camomile tea", "Jasmine tea", "Matcha",

            // Bottled flavors
            "Cucumber syrup", "Orange marmalade", "Grape marmalade", "Cherry jam", "Strawberry jam",
            "Apricot jam", "Blueberry jam", "Raspberry jam"
        };

        public static List<string> SearchMeasureType(string search, int max)
        {
            if (string.IsNullOrWhiteSpace(search))
                return new List<string>();

            search = search.Trim().ToLower();

            var startsWith = MeasurementUnits
                .Where(i => i != null && i.ToLower().StartsWith(search))
                .ToList();

            var contains = MeasurementUnits
                .Where(i => i != null && i.ToLower().Contains(search) && !i.ToLower().StartsWith(search))
                .ToList();

            var result = startsWith
                .Concat(contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(max)
                .ToList();

            return result;
        }

        public static List<string> SearchIngredients(string search, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(search))
                return new List<string>();

            search = search.Trim().ToLower();

            // Alcolici
            var startsWithAlcoholic = IngredientAlcoholic
                .Where(i => i != null && i.ToLower().StartsWith(search))
                .ToList();

            var containsAlcoholic = IngredientAlcoholic
                .Where(i => i != null && i.ToLower().Contains(search) && !i.ToLower().StartsWith(search))
                .ToList();

            // Analcolici
            var startsWithNonAlcoholic = IngredientNonAlcoholic
                .Where(i => i != null && i.ToLower().StartsWith(search))
                .ToList();

            var containsNonAlcoholic = IngredientNonAlcoholic
                .Where(i => i != null && i.ToLower().Contains(search) && !i.ToLower().StartsWith(search))
                .ToList();

            var result = startsWithAlcoholic
                .Concat(startsWithNonAlcoholic)
                .Concat(containsAlcoholic)
                .Concat(containsNonAlcoholic)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxResults)
                .ToList();

            return result;
        }


        public static List<string> SearchNonAlcoholicIngredients(string search, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(search))
                return new List<string>();

            search = search.Trim().ToLower();

            var startsWith = IngredientNonAlcoholic
                .Where(i => i != null && i.ToLower().StartsWith(search))
                .ToList();

            var contains = IngredientNonAlcoholic
                .Where(i => i != null && i.ToLower().Contains(search) && !i.ToLower().StartsWith(search))
                .ToList();

            var result = startsWith
                .Concat(contains)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxResults)
                .ToList();

            return result;
        }

        public static bool CocktailIsAlcoholic(List<string> ingredients)
        {
            return ingredients.Any(i => IngredientAlcoholic.Contains(i, StringComparer.OrdinalIgnoreCase));
        }

        
        public static List<string> IngredientToList(Cocktail c){
            var ingredients = new List<string>
            {
                c.StrIngredient1 ?? string.Empty, c.StrIngredient2 ?? string.Empty, c.StrIngredient3 ?? string.Empty, c.StrIngredient4 ?? string.Empty, c.StrIngredient5 ?? string.Empty,
                c.StrIngredient6 ?? string.Empty, c.StrIngredient7 ?? string.Empty, c.StrIngredient8 ?? string.Empty, c.StrIngredient9 ?? string.Empty, c.StrIngredient10 ?? string.Empty,
                c.StrIngredient11 ?? string.Empty, c.StrIngredient12 ?? string.Empty, c.StrIngredient13 ?? string.Empty, c.StrIngredient14 ?? string.Empty, c.StrIngredient15 ?? string.Empty
            }.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
            return ingredients;
        }

        public static List<string> MeasureToList(Cocktail c){
            var measures = new List<string>
            {
                c.StrMeasure1 ?? string.Empty, c.StrMeasure2 ?? string.Empty, c.StrMeasure3 ?? string.Empty, c.StrMeasure4 ?? string.Empty, c.StrMeasure5 ?? string.Empty,
                c.StrMeasure6 ?? string.Empty, c.StrMeasure7 ?? string.Empty, c.StrMeasure8 ?? string.Empty, c.StrMeasure9 ?? string.Empty, c.StrMeasure10 ?? string.Empty,
                c.StrMeasure11 ?? string.Empty, c.StrMeasure12 ?? string.Empty, c.StrMeasure13 ?? string.Empty, c.StrMeasure14 ?? string.Empty, c.StrMeasure15 ?? string.Empty
            }.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
            return measures;
        }


        public static CocktailDto CocktailToDto(Cocktail c)
        {
            return new CocktailDto
            {
                Id = c.Id,
                IdDrink = c.IdDrink ?? string.Empty,
                StrDrink = c.StrDrink ?? string.Empty,
                StrCategory = c.StrCategory ?? string.Empty,
                StrAlcoholic = c.StrAlcoholic ?? string.Empty,
                StrGlass = c.StrGlass ?? string.Empty,
                StrInstructions = c.StrInstructions ?? string.Empty,
                StrDrinkThumb = c.StrDrinkThumb ?? string.Empty,
                Ingredients = IngredientToList(c),
                Measures = MeasureToList(c),
                UserLikes = c.UserLikes.Select(u => u.UserName).ToList() ?? new List<string>(),
                StrTags = c.StrTags ?? string.Empty
            };
        }

        public static Cocktail CreateNewCocktail(CocktailCreate cocktailCreate, string username)
        {
            return new Cocktail
            {
                UserNameCocktail = username,
                PublicCocktail = cocktailCreate.PublicCocktail,
                dateCreated = DateTime.UtcNow,
                Likes = 0,
                IdDrink = cocktailCreate.IdDrink,
                StrDrink = cocktailCreate.StrDrink,
                StrDrinkAlternate = cocktailCreate.StrDrinkAlternate,
                StrTags = cocktailCreate.StrTags,
                StrVideo = cocktailCreate.StrVideo,
                StrCategory = cocktailCreate.StrCategory,
                StrIBA = cocktailCreate.StrIBA,
                StrAlcoholic = cocktailCreate.StrAlcoholic,
                StrGlass = cocktailCreate.StrGlass,
                StrInstructions = cocktailCreate.StrInstructions,
                StrInstructionsES = cocktailCreate.StrInstructionsES,
                StrInstructionsDE = cocktailCreate.StrInstructionsDE,
                StrInstructionsFR = cocktailCreate.StrInstructionsFR,
                StrInstructionsIT = cocktailCreate.StrInstructionsIT,
                StrInstructionsZH_HANS = cocktailCreate.StrInstructionsZH_HANS,
                StrInstructionsZH_HANT = cocktailCreate.StrInstructionsZH_HANT,
                StrDrinkThumb = cocktailCreate.StrDrinkThumb,

                // Aggiungi gli ingredienti e le misure
                StrIngredient1 = cocktailCreate.StrIngredient1,
                StrIngredient2 = cocktailCreate.StrIngredient2,
                StrIngredient3 = cocktailCreate.StrIngredient3,
                StrIngredient4 = cocktailCreate.StrIngredient4,
                StrIngredient5 = cocktailCreate.StrIngredient5,
                StrIngredient6 = cocktailCreate.StrIngredient6,
                StrIngredient7 = cocktailCreate.StrIngredient7,
                StrIngredient8 = cocktailCreate.StrIngredient8,
                StrIngredient9 = cocktailCreate.StrIngredient9,
                StrIngredient10 = cocktailCreate.StrIngredient10,
                StrIngredient11 = cocktailCreate.StrIngredient11,
                StrIngredient12 = cocktailCreate.StrIngredient12,
                StrIngredient13 = cocktailCreate.StrIngredient13,
                StrIngredient14 = cocktailCreate.StrIngredient14,
                StrIngredient15 = cocktailCreate.StrIngredient15,
                StrMeasure1 = cocktailCreate.StrMeasure1,
                StrMeasure2 = cocktailCreate.StrMeasure2,
                StrMeasure3 = cocktailCreate.StrMeasure3,
                StrMeasure4 = cocktailCreate.StrMeasure4,
                StrMeasure5 = cocktailCreate.StrMeasure5,
                StrMeasure6 = cocktailCreate.StrMeasure6,
                StrMeasure7 = cocktailCreate.StrMeasure7,
                StrMeasure8 = cocktailCreate.StrMeasure8,
                StrMeasure9 = cocktailCreate.StrMeasure9,
                StrMeasure10 = cocktailCreate.StrMeasure10,
                StrMeasure11 = cocktailCreate.StrMeasure11,
                StrMeasure12 = cocktailCreate.StrMeasure12,
                StrMeasure13 = cocktailCreate.StrMeasure13,
                StrMeasure14 = cocktailCreate.StrMeasure14,
                StrMeasure15 = cocktailCreate.StrMeasure15
            };
        }

        public static void UpdateCocktail(Cocktail cocktail, CocktailCreate dto)
        {
            // Aggiorna le proprietà del cocktail esistente con i nuovi valori
            cocktail.StrDrink = dto.StrDrink ?? cocktail.StrDrink;
            cocktail.StrCategory = dto.StrCategory ?? cocktail.StrCategory;
            cocktail.StrAlcoholic = dto.StrAlcoholic ?? cocktail.StrAlcoholic;
            cocktail.StrGlass = dto.StrGlass ?? cocktail.StrGlass;
            cocktail.StrInstructions = dto.StrInstructions ?? cocktail.StrInstructions;  
            cocktail.StrDrinkThumb = dto.StrDrinkThumb ?? cocktail.StrDrinkThumb;
            cocktail.StrTags = dto.StrTags ?? cocktail.StrTags;
            cocktail.StrVideo = dto.StrVideo ?? cocktail.StrVideo;
            cocktail.StrIBA = dto.StrIBA ?? cocktail.StrIBA;
            cocktail.DateModified = DateTime.UtcNow.ToString("yyyy-MM-dd"); // Aggiorna la data di modifica
            cocktail.PublicCocktail = dto.PublicCocktail; // Aggiorna la visibilità del cocktail
            cocktail.StrDrinkAlternate = dto.StrDrinkAlternate ?? cocktail.StrDrinkAlternate;
            cocktail.StrInstructionsES = dto.StrInstructionsES ?? cocktail.StrInstructionsES;
            cocktail.StrInstructionsDE = dto.StrInstructionsDE ?? cocktail.StrInstructionsDE;
            cocktail.StrInstructionsFR = dto.StrInstructionsFR ?? cocktail.StrInstructionsFR;
            cocktail.StrInstructionsIT = dto.StrInstructionsIT ?? cocktail.StrInstructionsIT;
            cocktail.StrInstructionsZH_HANS = dto.StrInstructionsZH_HANS ?? cocktail.StrInstructionsZH_HANS;
            cocktail.StrInstructionsZH_HANT = dto.StrInstructionsZH_HANT ?? cocktail.StrInstructionsZH_HANT;
            for (int i = 1; i <= 15; i++)
            {
                typeof(Cocktail).GetProperty($"StrIngredient{i}")?.SetValue(cocktail, 
                    typeof(CocktailCreate).GetProperty($"StrIngredient{i}")?.GetValue(dto));

                typeof(Cocktail).GetProperty($"StrMeasure{i}")?.SetValue(cocktail, 
                    typeof(CocktailCreate).GetProperty($"StrMeasure{i}")?.GetValue(dto));
            }
        }

        public static double ConvertToMilliliters(string? measure)
        {
            if (string.IsNullOrWhiteSpace(measure)) return 0;

            measure = measure.ToLower().Trim();
            double value = 0;

            // Supporto per formati come "1/2 oz", "1.5 cl", "2 cups"
            var parts = measure.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length >= 1)
            {
                // Parse quantity (supporta frazioni e decimali)
                if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
                {
                    value = parsed;
                }
                else if (parts[0].Contains('/'))
                {
                    var fractionParts = parts[0].Split('/');
                    if (fractionParts.Length == 2 &&
                        double.TryParse(fractionParts[0], out double numerator) &&
                        double.TryParse(fractionParts[1], out double denominator) &&
                        denominator != 0)
                    {
                        value = numerator / denominator;
                    }
                }

                // Se non c'è unità, assumiamo ml
                if (parts.Length == 1)
                {
                    return value;
                }

                // Convert unit (cerca l'unità nelle parti rimanenti)
                string unit = string.Join(" ", parts.Skip(1)).ToLower();

                if (unit.Contains("ml")) return value;
                if (unit.Contains("oz") || unit.Contains("ounce")) return value * 29.5735;
                if (unit.Contains("cl")) return value * 10;
                if (unit.Contains("cup")) return value * 240;
                if (unit.Contains("tsp") || unit.Contains("teaspoon")) return value * 5;
                if (unit.Contains("tbsp") || unit.Contains("tablespoon")) return value * 15;
                if (unit.Contains("dash")) return value * 0.92;
                if (unit.Contains("shot")) return value * 30; // 1 shot ≈ 30ml
                if (unit.Contains("part")) return value * 30; // 1 part ≈ 30ml (standard in mixology)
            }

            return 0; // Formato non riconosciuto
        }

        public static string? ValidateVolumeClassCocktail(Cocktail cocktail, Dictionary<string, int> glassCapacity)
        {
            double totalMl = 0;
            for (int i = 1; i <= 15; i++)
            {
                var measure = typeof(Cocktail).GetProperty($"StrMeasure{i}")?.GetValue(cocktail)?.ToString();
                var ml = ConvertToMilliliters(measure);
                
                if (ml <= 0 && !string.IsNullOrEmpty(measure))
                {
                    return $"Invalid measure format for ingredient {i}: '{measure}'. Please use ml or a convertible unit.";
                }
                totalMl += ml;
            }

            var glass = cocktail.StrGlass ?? "Cocktail glass";
            int maxCapacity = glassCapacity.TryGetValue(glass, out var capacity) ? capacity : 250;
            
            if (totalMl > maxCapacity * 1.1) // 10% di tolleranza
            {
                return $"The cocktail exceeds the maximum glass capacity ({maxCapacity} ml). Total: {totalMl:F1} ml";
            }
            
            return null;
        }

        public static string? ValidateIngredientMeasureConsistency(Cocktail cocktail)
        {
            for (int i = 1; i <= 15; i++)
            {
                var ingredient = typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(cocktail)?.ToString();
                var measure = typeof(Cocktail).GetProperty($"StrMeasure{i}")?.GetValue(cocktail)?.ToString();

                if (string.IsNullOrWhiteSpace(ingredient) && !string.IsNullOrWhiteSpace(measure))
                {
                    return $"Errore: la misura {i} è impostata ma manca l'ingrediente corrispondente.";
                }
            }

            return null;
        }


        public static int GetSuggestionScore(Cocktail c, User user, List<string> searchHistory, List<Cocktail> likedCocktails)
        {
            int score = 0;

            var filterWeight = new Dictionary<SuggestionUser, int>{
                { SuggestionUser.NameMatch, 4 },
                { SuggestionUser.IngredientMatch, 2 },
                { SuggestionUser.CategoryMatch, 2 },
                { SuggestionUser.GlassMatch, 3 },
                { SuggestionUser.DescriptionMatch, 2 },
                { SuggestionUser.SearchHistoryMatch, 6 },
                { SuggestionUser.likeCocktail, 7 }
            };


            var ingredients = Enumerable.Range(1, 15)
                .Select(i => typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(c)?.ToString()?.ToLower())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();


            // Ingredienti più presenti nei like
            var topIngredientLikes = likedCocktails
                .SelectMany(l => Enumerable.Range(1, 15)
                    .Select(i => typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(l)?.ToString()?.ToLower())
                    .Where(i => !string.IsNullOrWhiteSpace(i)))
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Take(5) // Prendi i primi 3 ingredienti più comuni
                .Select(g => g.Key)
                .ToList();

            if(ingredients.Any(i => topIngredientLikes.Contains(i))) 
                score += filterWeight[SuggestionUser.IngredientMatch];
            // MATCH con LIKE
            foreach (var liked in likedCocktails)
            {
                if (liked.Id == c.Id) {
                    score += filterWeight[SuggestionUser.likeCocktail];
                    continue; 
                }
                // Ingredient match
                var likedIngredients = Enumerable.Range(1, 15)
                    .Select(i => typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(liked)?.ToString()?.ToLower())
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .ToList();

                if (ingredients.Any(i => likedIngredients.Contains(i))) 
                    score +=  filterWeight[SuggestionUser.IngredientMatch];

                // Categoria, bicchiere
                if (!string.IsNullOrEmpty(c.StrCategory) && c.StrCategory == liked.StrCategory)
                    score +=  filterWeight[SuggestionUser.CategoryMatch];;
                if (!string.IsNullOrEmpty(c.StrGlass) && c.StrGlass == liked.StrGlass)
                    score +=  filterWeight[SuggestionUser.GlassMatch];
            }

            // MATCH con ricerche recenti
            if (searchHistory.Any(s => !string.IsNullOrEmpty(c.StrDrink) && c.StrDrink.ToLower().Contains(s)))
                score +=  filterWeight[SuggestionUser.NameMatch];
            if (searchHistory.Any(s => ingredients.Any(i => i != null && i.Contains(s))))
                score +=  filterWeight[SuggestionUser.IngredientMatch];
            if (searchHistory.Any(s => c.StrCategory?.ToLower().Contains(s) == true))
                score +=  filterWeight[SuggestionUser.CategoryMatch];
            if (searchHistory.Any(s => c.StrGlass?.ToLower().Contains(s) == true))
                score +=  filterWeight[SuggestionUser.GlassMatch];
            if (searchHistory.Any(s => c.StrInstructions?.ToLower().Contains(s) == true))
                score +=  filterWeight[SuggestionUser.DescriptionMatch];
            return score;
        }

    }
}