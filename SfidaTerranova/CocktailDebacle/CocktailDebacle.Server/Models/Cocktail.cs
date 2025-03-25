using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CocktailDebacle.Server.Models
{
    public class Cocktail
    {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Alcoholic { get; set; } = string.Empty;
            public string Glass { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Instructions { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }

    public class Ingredient
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [ForeignKey("Cocktail")]
        public int CocktailId { get; set; }

        public virtual Cocktail Cocktail { get; set; }
    }
}
