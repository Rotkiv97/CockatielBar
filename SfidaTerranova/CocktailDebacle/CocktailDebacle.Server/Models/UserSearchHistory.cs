using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocktailDebacle.Server.Models
{
    public class UserSearchHistory
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty; // Nome utente dell'utente che ha effettuato la ricerca

        [Required]
        public string SearchText { get; set; } = string.Empty; // Testo della ricerca effettuata

        public DateTime DateCreated { get; set; } // Added DateCreated property
        
        [ForeignKey("UserName")]
        public virtual User? User { get; set; } // Navigational property to the User entity

    }
}