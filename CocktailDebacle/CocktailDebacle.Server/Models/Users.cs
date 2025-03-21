using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocktailDebacle.Server.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Identificativo univoco per la tabella Users
        public ICollection<User> UserList { get; set; } = new List<User>(); // Lista di utenti
    }
}
