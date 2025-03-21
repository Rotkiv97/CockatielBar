using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using CocktailDebacle.Server.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    //Replace 'Password' with 'PasswordHash' for security
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Relazione con Users (gruppo di utenti)
    [ForeignKey("Users")]
    public int UsersId { get; set; }
    public Users? Users { get; set; }

    // Permessi e preferenze
    public bool PersonalizedExperience { get; set; } = false;
    public bool AcceptCookis { get; set; } = false;
    public bool Online { get; set; } = false;

    // Relazioni
    public ICollection<User> Friends { get; set; } = new List<User>();
    public ICollection<Cocktail> CocktailsLike { get; set; } = new List<Cocktail>();
    public ICollection<Cocktail> CocktailsCreate { get; set; } = new List<Cocktail>();

    public RecommenderSystems? RecommenderSystems { get; set; }

    // Personalizzazioni
    [StringLength(10)]
    public string Leanguage { get; set; } = "en";
    public string ImgProfile { get; set; } = string.Empty;
}

