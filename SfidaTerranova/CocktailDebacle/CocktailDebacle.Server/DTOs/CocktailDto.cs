using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocktailDebacle.Server.DTOs
{
    public class CocktailDto
    {
        public string IdDrink { get; set; } = string.Empty;
        public string StrDrink { get; set; } = string.Empty;
        public string StrCategory { get; set; } = string.Empty;
        public string StrAlcoholic { get; set; } = string.Empty;
        public string StrGlass { get; set; } = string.Empty;
        public string StrInstructions { get; set; } = string.Empty;
        public string StrDrinkThumb { get; set; } = string.Empty;

        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Measures { get; set; } = new List<string>();
        public string StrTags { get; set; } = string.Empty;
    }
}