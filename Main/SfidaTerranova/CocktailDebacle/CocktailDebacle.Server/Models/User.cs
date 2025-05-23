﻿﻿using System.ComponentModel.DataAnnotations;
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
    public bool? AcceptCookies { get; set; } = false;

    public bool? CustomSearch { get; set; } = false;

    public string Token { get; set; } = string.Empty;

    public DateTime? TokenExpiration { get; set; }
    // public DateTime? LastLogin { get; set; } = DateTime.UtcNow;

    // [StringLength(100)]
    public string? ImgProfileUrl { get; set; } = string.Empty;

    public string ProfileParallaxImg { get; set; } = string.Empty; // da levare forse

    public string? Bio { get; set; } = string.Empty; // da levare forse

    public string? Bio_link { get; set; } = string.Empty; // da levare forse
    public ICollection<Cocktail> CocktailsLike { get; set; } = new List<Cocktail>();

    public ICollection<User> Followed_Users { get; set; } = new List<User>(); // utenti seguiti

    public ICollection<User> Followers_Users { get; set; } = new List<User>(); // utenti che ti seguono
    
    public bool? IsOfMajorityAge { get; set; } = true; // per la registrazione, se l'utente ha piu di 18 anni

    public string Language { get; set; } = "en";
}