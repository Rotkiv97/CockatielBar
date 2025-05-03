using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocktailDebacle.Server.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ImgProfileUrl { get; set; } = string.Empty;
        public List<int> Followed_Users { get; set; } = new List<int>();
        public List<int> Followers_Users { get; set; } = new List<int>();

        public string Leanguage { get; set; } = string.Empty;
    }
}