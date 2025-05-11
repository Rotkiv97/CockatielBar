using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CocktailDebacle.Server.DTOs
{
    public class CocktailDto
    {
        public int Id { get; set; }
        public string IdDrink { get; set; } = string.Empty;
        public bool? PublicCocktail { get; set; } = true;
        public int UserIdCocktail { get; set; } 
        public string StrDrink { get; set; } = string.Empty;
        public string StrCategory { get; set; } = string.Empty;
        // [Required]
        // [RegularExpression("^(Alcoholic|Non alcoholic|Optional alcohol)$", ErrorMessage = "Must be 'Alcoholic', 'Non alcoholic' or 'Optional alcohol'")]
        public string StrAlcoholic { get; set; } = string.Empty;
        public string StrGlass { get; set; } = string.Empty;
        public string StrInstructions { get; set; } = string.Empty;
        public string StrDrinkThumb { get; set; } = string.Empty;

        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Measures { get; set; } = new List<string>();
        public int Likes { get; set; } = 0;
        public List<int> UserLikes { get; set; } = new List<int>();
        public string StrTags { get; set; } = string.Empty;
    }
}