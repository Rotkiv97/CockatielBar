namespace CocktailDebacle.Server.Models
{
    // metodo per calcolare il punteggio di un cocktail in base alla corrispondenza con i suggerimenti dell'utente   
    public enum SuggestionUser
    {
        likeCocktail = 1,
        NameMatch = 2,
        SearchHistoryMatch = 3,
        IngredientMatch = 4,
        CategoryMatch = 5,
        GlassMatch = 6,
        DescriptionMatch = 7,
    }
}