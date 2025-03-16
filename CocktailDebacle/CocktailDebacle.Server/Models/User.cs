using System.Security.Permissions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CocktailDebacle.Server.Models
{
    public class User
    {
        //Informazioni primarie dell'utente
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = 0;
        [Required, StringLength(50)]
        public string UserName { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;


        //Permessi
        public bool PersonalizedExperience { get; set; } = false;
        public bool AcceptCookis { get; set; } = false;

        // Stato
        public bool online { get; set; } = false;

        // gestione preferenze e ricerca intelligente personalizzata
        public ICollection<User> Friends { get; set; } = new List<User>();
        public ICollection<Cocktail> CocktailsLike { get; set; } = new List<Cocktail>();
        public ICollection<Cocktail> CocktailsCreate { get; set; } = new List<Cocktail>();
        public RecommenderSystems? RecommenderSystems { get; set; }

        // Pesonalizzazioni 
        [StringLength(10)]
        public string Leanguage { get; set; } = "en";
        public string ImgProfile {  get; set; } = string.Empty;
    }
}
