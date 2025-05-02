using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CocktailDebacle.Server.Models
{
   public class UserHistorySearch
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; } = null!;
        public string? SearchText { get; set; }
        public DateTime SearchDate { get; set; } = DateTime.UtcNow;
    }
}