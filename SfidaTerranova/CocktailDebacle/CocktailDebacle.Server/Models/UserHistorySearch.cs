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
        public string UserName { get; set; } = string.Empty;

        public string? SearchText { get; set; } 

        public DateTime SearchDate { get; set; } = DateTime.UtcNow;
    }
}