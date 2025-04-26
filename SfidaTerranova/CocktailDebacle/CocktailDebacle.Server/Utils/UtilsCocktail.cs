using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocktailDebacle.Server.DTOs;
using CocktailDebacle.Server.Models;

namespace CocktailDebacle.Server.Utils
{
    public class UtilsCocktail
    {
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

        // public static CocktailDto CocktailToDto(Cocktail c)
        // {
        //     return new CocktailDto
        //     {
        //         Id = c.Id,
        //         IdDrink = c.IdDrink,
        //         StrDrink = c.StrDrink,
        //         StrCategory = c.StrCategory,
        //         StrAlcoholic = c.StrAlcoholic,
        //         StrGlass = c.StrGlass,
        //         StrInstructions = c.StrInstructions,
        //         StrDrinkThumb = c.StrDrinkThumb,
        //         Ingredients = IngredientToList(c),
        //         Measures = MeasureToList(c),
        //         UserLikes = c.UserLikes.Select(u => u.UserName).ToList() ?? new List<string>(),
        //         Followed_Users = c.Followed_Users.Select(u => u.UserName).ToList() ?? new List<string>(),
        //         Followers_Users = c.Followers_Users.Select(u => u.UserName).ToList() ?? new List<string>(),
        //         StrTags = c.StrTags ?? string.Empty
        //     };
        // }
    }
}