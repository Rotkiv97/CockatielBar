using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CocktailDebacle.Server.Models
{
    public class CocktailCreate
    {
        [Key]
        public int Id { get; set; } // Chiave primaria

        public string? UserNameCocktail { get; set; } = string.Empty; // Nome utente che ha creato il cocktail
        
        public bool? PublicCocktail { get; set; } = false; // Indica se il cocktail è pubblico o privato

        public DateTime? dateCreated { get; set; } = DateTime.UtcNow; // Data di creazione del cocktail
        public int Likes { get; set; } = 0; // Indica il numero di like ricevuti dal cocktail

        public string? IdDrink { get; set; } = string.Empty;
        public string? StrDrink { get; set; } = string.Empty;

        public string? StrDrinkAlternate { get; set; } = string.Empty;

        public string? StrTags { get; set; } = string.Empty;

        public string? StrVideo { get; set; } = string.Empty;

        public string? StrCategory { get; set; } = string.Empty;

        public string? StrIBA { get; set; } = string.Empty;

        public string? StrAlcoholic { get; set; } = string.Empty;

        public string? StrGlass { get; set; } = string.Empty;

        public string? StrInstructions { get; set; } = string.Empty;

        public string? StrInstructionsES { get; set; } = string.Empty;

        public string? StrInstructionsDE { get; set; } = string.Empty;

        public string? StrInstructionsFR { get; set; } = string.Empty;

        public string? StrInstructionsIT { get; set; } = string.Empty;

        public string? StrInstructionsZH_HANS { get; set; } = string.Empty;

        public string? StrInstructionsZH_HANT { get; set; } = string.Empty;

        public string? StrDrinkThumb { get; set; } = string.Empty;


        public string? StrIngredient1 { get; set; } = string.Empty;

        public string? StrIngredient2 { get; set; } = string.Empty;

        public string? StrIngredient3 { get; set; } = string.Empty;

        public string? StrIngredient4 { get; set; } = string.Empty;

        public string? StrIngredient5 { get; set; } = string.Empty;

        public string? StrIngredient6 { get; set; } = string.Empty;

        public string? StrIngredient7 { get; set; } = string.Empty;

        public string? StrIngredient8 { get; set; } = string.Empty;

        public string? StrIngredient9 { get; set; } = string.Empty;

        public string? StrIngredient10 { get; set; } = string.Empty;

        public string? StrIngredient11 { get; set; } = string.Empty;

        public string? StrIngredient12 { get; set; } = string.Empty;

        public string? StrIngredient13 { get; set; } = string.Empty;

        public string? StrIngredient14 { get; set; } = string.Empty;

        public string? StrIngredient15 { get; set; } = string.Empty;


        public string? StrMeasure1 { get; set; } = string.Empty;

        public string? StrMeasure2 { get; set; } = string.Empty;

        public string? StrMeasure3 { get; set; } = string.Empty;

        public string? StrMeasure4 { get; set; } = string.Empty;

        public string? StrMeasure5 { get; set; } = string.Empty;

        public string? StrMeasure6 { get; set; } = string.Empty;

        public string? StrMeasure7 { get; set; } = string.Empty;

        public string? StrMeasure8 { get; set; } = string.Empty;

        public string? StrMeasure9 { get; set; } = string.Empty;

        public string? StrMeasure10 { get; set; } = string.Empty;

        public string? StrMeasure11 { get; set; } = string.Empty;

        public string? StrMeasure12 { get; set; } = string.Empty;

        public string? StrMeasure13 { get; set; } = string.Empty;

        public string? StrMeasure14 { get; set; } = string.Empty;

        public string? StrMeasure15 { get; set; } = string.Empty;


        public string? StrImageSource { get; set; } = string.Empty;

        public string? StrImageAttribution { get; set; } = string.Empty;

        public string? StrCreativeCommonsConfirmed { get; set; } = string.Empty;

        public string? DateModified { get; set; } = string.Empty;
    }
}