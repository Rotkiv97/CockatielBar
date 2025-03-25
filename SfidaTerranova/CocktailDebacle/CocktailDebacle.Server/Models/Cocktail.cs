using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CocktailDebacle.Server.Models
{
     public class Cocktail
    {
        [Key]
        public int Id { get; set; }  // Identificativo univoco del cocktail
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string IBA { get; set; } = string.Empty;
        public string Alcoholic { get; set; } = string.Empty;
        public string Glass { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string CreativeCommonsConfirmed { get; set; } = string.Empty;
        public DateTime? DateModified { get; set; }

        // Relazione 1-a-Molti con Ingredient
        public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }

    public class Ingredient
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Measure { get; set; } = string.Empty; // Es: "1 1/2 oz"

        [ForeignKey("Cocktail")]
        public int CocktailId { get; set; } // Collegamento con il Cocktail

        public virtual Cocktail? Cocktail { get; set; }
    }

}
