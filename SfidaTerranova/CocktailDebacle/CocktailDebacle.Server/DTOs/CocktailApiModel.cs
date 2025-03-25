using System;
using System.Collections.Generic;
using System.Linq;
using CocktailDebacle.Server.Models;
using System.Threading.Tasks;

namespace CocktailDebacle.Server.DTOs
{
   public class CocktailApiModel
    {
        public string IdDrink { get; set; }
        public string StrDrink { get; set; }
        public string StrCategory { get; set; }
        public string StrIBA { get; set; }
        public string StrAlcoholic { get; set; }
        public string StrGlass { get; set; }
        public string StrInstructions { get; set; }
        public string StrDrinkThumb { get; set; }
        public string StrVideo { get; set; }
        public string StrTags { get; set; }
        public string StrCreativeCommonsConfirmed { get; set; }
        public string DateModified { get; set; }

        // Lista dinamica di ingredienti e misure
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        // Metodo per recuperare gli ingredienti e le misure dal JSON
        public void ExtractIngredients()
        {
            for (int i = 1; i <= 15; i++)
            {
                var ingredient = GetType().GetProperty($"StrIngredient{i}")?.GetValue(this, null)?.ToString();
                var measure = GetType().GetProperty($"StrMeasure{i}")?.GetValue(this, null)?.ToString();

                if (!string.IsNullOrEmpty(ingredient))
                {
                    Ingredients.Add(new Ingredient
                    {
                        Name = ingredient,
                        Measure = measure ?? ""
                    });
                }
            }
        }

        // Proprietà degli ingredienti (da 1 a 15)
        public string StrIngredient1 { get; set; }
        public string StrIngredient2 { get; set; }
        public string StrIngredient3 { get; set; }
        public string StrIngredient4 { get; set; }
        public string StrIngredient5 { get; set; }
        public string StrIngredient6 { get; set; }
        public string StrIngredient7 { get; set; }
        public string StrIngredient8 { get; set; }
        public string StrIngredient9 { get; set; }
        public string StrIngredient10 { get; set; }
        public string StrIngredient11 { get; set; }
        public string StrIngredient12 { get; set; }
        public string StrIngredient13 { get; set; }
        public string StrIngredient14 { get; set; }
        public string StrIngredient15 { get; set; }

        // Proprietà delle misure corrispondenti
        public string StrMeasure1 { get; set; }
        public string StrMeasure2 { get; set; }
        public string StrMeasure3 { get; set; }
        public string StrMeasure4 { get; set; }
        public string StrMeasure5 { get; set; }
        public string StrMeasure6 { get; set; }
        public string StrMeasure7 { get; set; }
        public string StrMeasure8 { get; set; }
        public string StrMeasure9 { get; set; }
        public string StrMeasure10 { get; set; }
        public string StrMeasure11 { get; set; }
        public string StrMeasure12 { get; set; }
        public string StrMeasure13 { get; set; }
        public string StrMeasure14 { get; set; }
        public string StrMeasure15 { get; set; }
    }

}